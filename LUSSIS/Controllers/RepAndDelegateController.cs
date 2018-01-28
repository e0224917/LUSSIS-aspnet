using LUSSIS.Models;
using LUSSIS.Models.WebDTO;
using LUSSIS.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LUSSIS.Exceptions;
using System.Globalization;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System.Web.Security;
using LUSSIS.DAL;
using LUSSIS.CustomAuthority;
using LUSSIS.Extensions;

namespace LUSSIS.Controllers
{
    [CustomAuthorize("head", "staff")]
    public class RepAndDelegateController : Controller
    {
        private readonly EmployeeRepository _employeeRepo = new EmployeeRepository();
        private readonly DelegateRepository _delegateRepo = new DelegateRepository();
        private readonly RequisitionRepository _requisitionRepo = new RequisitionRepository();
        private readonly DepartmentRepository _departmentRepo = new DepartmentRepository();

        //for delegate and head only
        // GET: /RepAndDelegate/
        public ActionResult Index()
        {
            var deptCode = Request.Cookies["Employee"]?["DeptCode"];

            var department = _departmentRepo.GetById(deptCode);
            var existingDelegate = _delegateRepo.FindExistingByDeptCode(deptCode);
            var staffAndRepList = department.Employees
                .Where(e => e.JobTitle == "staff" || e.JobTitle == "rep").ToList();
            var reqListCount = _requisitionRepo.GetPendingListForHead(deptCode).Count();
            var haveDelegateToday = false;
            if (existingDelegate != null)
                haveDelegateToday = existingDelegate.StartDate <= DateTime.Today;

            var dbDto = new DeptHeadDashBoardDTO
            {
                Department = department,
                CurrentDelegate = existingDelegate,
                StaffRepByDepartment = staffAndRepList,
                RequisitionListCount = reqListCount,
                HaveDelegateToday = haveDelegateToday
            };

            return View(dbDto);
        }

        // GET: /RepAndDelegate/DeptRep
        [HeadWithDelegateAuth("head", "staff")]
        public ActionResult DeptRep()
        {
            var deptCode = Request.Cookies["Employee"]?["DeptCode"];
            var department = _departmentRepo.GetById(deptCode);
            var staffAndRepList = department.Employees
                .Where(e => e.JobTitle == "staff" || e.JobTitle == "rep").ToList();

            var radDto = new RepAndDelegateDTO
            {
                Department = department,
                StaffAndRepList = staffAndRepList,
            };

            return View(radDto);
        }

        // GET: /RepAndDelegate/GetEmpJson
        [HttpGet]
        public JsonResult GetEmpJson(string prefix)
        {
            var deptCode = Request.Cookies["Employee"]?["DeptCode"];
            var department = _departmentRepo.GetById(deptCode);
            var staffAndRepList = department.Employees
                .Where(e => e.JobTitle == "staff" || e.JobTitle == "rep").ToList();
            var selectedList = staffAndRepList
                .Where(e => e.FullName.Contains(prefix, StringComparison.OrdinalIgnoreCase)).ToList();

            var selectedEmps = selectedList.Select(x => new
            {
                x.FullName,
                x.EmpNum
            });

            return Json(selectedEmps, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize("head")]
        [HttpGet]
        public JsonResult GetEmpForDelJson(string prefix)
        {
            var deptCode = Request.Cookies["Employee"]?["DeptCode"];
            var department = _departmentRepo.GetById(deptCode);
            var staffList = department.Employees
                .Where(e => e.JobTitle == "staff").ToList();
            var selectedlist = staffList
                .Where(e => e.FullName.Contains(prefix, StringComparison.OrdinalIgnoreCase)).ToList();

            var selectedEmp = selectedlist.Select(x => new
            {
                x.FullName,
                x.EmpNum
            });

            return Json(selectedEmp, JsonRequestBehavior.AllowGet);
        }

        // POST: /RepAndDelegate/UpdateRep
        [HeadWithDelegateAuth("head", "staff")]
        [HttpPost]
        public ActionResult UpdateRep(string repEmp)
        {
            if (ModelState.IsValid)
            {
                var deptCode = Request.Cookies["Employee"]?["DeptCode"];
                var department = _departmentRepo.GetById(deptCode);

                var repEmpNum = Convert.ToInt32(repEmp);
                var newRep = _employeeRepo.GetById(repEmpNum);
                newRep.JobTitle = "rep";

                var oldRep = department.RepEmployee;


                if (oldRep == null)
                {
                    _employeeRepo.Update(newRep);
                    _departmentRepo.Update(department);
                }
                else
                {
                    var context = new ApplicationDbContext();
                    var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

                    oldRep.JobTitle = "staff";

                    var oldRepUser = context.Users.FirstOrDefault(u => u.Email == oldRep.EmailAddress);
                    userManager.RemoveFromRole(oldRepUser?.Id, "rep");
                    userManager.AddToRole(oldRepUser?.Id, "staff");

                    _employeeRepo.Update(newRep);
                    _employeeRepo.Update(oldRep);
                    _departmentRepo.Update(department);

                    var newRepUser = context.Users.FirstOrDefault(u => u.Email == newRep.EmailAddress);
                    userManager.RemoveFromRole(newRepUser?.Id, "staff");
                    userManager.AddToRole(newRepUser?.Id, "rep");
                }
            }

            return RedirectToAction("DeptRep");
        }

        [CustomAuthorize("head")]
        [HttpPost]
        public ActionResult AddDelegate(string delegateEmp, string from, string to)
        {
            if (ModelState.IsValid)
            {
                var empNum = Convert.ToInt32(delegateEmp);
                var startDate = DateTime.ParseExact(from, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                var endDate = DateTime.ParseExact(to, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                var del = new Models.Delegate()
                {
                    EmpNum = empNum,
                    StartDate = startDate,
                    EndDate = endDate
                };

                _delegateRepo.Add(del);
            }

            return RedirectToAction("MyDelegate");
        }

        [CustomAuthorize("head")]
        [HttpPost]
        public ActionResult DeleteDelegate()
        {
            if (ModelState.IsValid)
            {
                var deptCode = Request.Cookies["Employee"]?["DeptCode"];
                _delegateRepo.DeleteByDeptCode(deptCode);
                return RedirectToAction("Index");
            }
            var actionName = ControllerContext.RouteData.Values["action"].ToString();

            return RedirectToAction(actionName);
        }

        [CustomAuthorize("head")]
        public ActionResult MyDelegate()
        {
            var deptCode = Request.Cookies["Employee"]?["DeptCode"];
            var department = _departmentRepo.GetById(deptCode);
            var myDelegate = _delegateRepo.FindExistingByDeptCode(deptCode);

            var radDto = new RepAndDelegateDTO
            {
                Department = department,
                MyDelegate = myDelegate
            };

            return View(radDto);
        }

        [HttpPost]
        public ActionResult DirectToRequisitons()
        {
            return RedirectToAction("_ApproveReq", "Requisitions");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _employeeRepo.Dispose();
                _delegateRepo.Dispose();
                _departmentRepo.Dispose();
                _requisitionRepo.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}