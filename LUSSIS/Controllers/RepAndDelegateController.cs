using LUSSIS.Models;
using LUSSIS.Models.WebDTO;
using LUSSIS.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System.Web.Security;
using LUSSIS.Constants;
using LUSSIS.DAL;
using LUSSIS.CustomAuthority;
using LUSSIS.Extensions;
using LUSSIS.Emails;

namespace LUSSIS.Controllers
{
    //Authors: Ong Xin Ying
    [CustomAuthorize(Role.DepartmentHead, Role.Staff)]
    public class RepAndDelegateController : Controller
    {
        private readonly EmployeeRepository _employeeRepo = new EmployeeRepository();
        private readonly DelegateRepository _delegateRepo = new DelegateRepository();
        private readonly RequisitionRepository _requisitionRepo = new RequisitionRepository();
        private readonly DepartmentRepository _departmentRepo = new DepartmentRepository();

        private bool ExistDelegate
        {
            get
            {
                var deptCode = Request.Cookies["Employee"]?["DeptCode"];
                var current = _delegateRepo.FindExistingByDeptCode(deptCode);
                return current != null;
            }
        }

        //for delegate and head only
        // GET: /RepAndDelegate/
        public ActionResult Index()
        {
            var deptCode = Request.Cookies["Employee"]?["DeptCode"];

            var department = _departmentRepo.GetById(deptCode);
            var existingDelegate = _delegateRepo.FindExistingByDeptCode(deptCode);
            var reqListCount = _requisitionRepo.GetPendingListForHead(deptCode).Count();
            var haveDelegateToday = false;
            if (ExistDelegate)
                haveDelegateToday = existingDelegate.StartDate <= DateTime.Today;

            var dbDto = new DeptHeadDashBoardDTO
            {
                Department = department,
                CurrentDelegate = existingDelegate,
                RequisitionListCount = reqListCount,
                HaveDelegateToday = haveDelegateToday
            };

            return View(dbDto);
        }

        // GET: /RepAndDelegate/DeptRep
        [HeadWithDelegateAuth(Role.DepartmentHead, Role.Staff)]
        public ActionResult DeptRep()
        {
            var deptCode = Request.Cookies["Employee"]?["DeptCode"];
            var department = _departmentRepo.GetById(deptCode);

            var radDto = new RepAndDelegateDTO
            {
                Department = department,
            };

            return View(radDto);
        }

        // GET: /RepAndDelegate/GetEmpJson
        [HttpGet]
        public JsonResult GetEmpJson(string prefix)
        {
            var deptCode = Request.Cookies["Employee"]?["DeptCode"];
            var department = _departmentRepo.GetById(deptCode);
            var staffAndRepList = _employeeRepo.GetStaffRepByDeptCode(deptCode);

            if (ExistDelegate)
            {
                var staffDelegate = _employeeRepo.GetById(_delegateRepo.FindExistingByDeptCode(deptCode).EmpNum);
                staffAndRepList.Remove(staffDelegate);
            }

            var selectedList = staffAndRepList
                .Where(e => e.FullName.Contains(prefix, StringComparison.OrdinalIgnoreCase)).ToList();

            var selectedEmps = selectedList.Select(x => new
            {
                x.FullName,
                x.EmpNum
            });

            return Json(selectedEmps, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Role.DepartmentHead)]
        [HttpGet]
        public JsonResult GetEmpForDelJson(string prefix)
        {
            var deptCode = Request.Cookies["Employee"]?["DeptCode"];
            var department = _departmentRepo.GetById(deptCode);
            var staffList = _employeeRepo.GetStaffByDeptCode(deptCode);
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
        [HeadWithDelegateAuth(Role.DepartmentHead, Role.Staff)]
        [HttpPost]
        public ActionResult UpdateRep(string repEmp)
        {
            if (ModelState.IsValid)
            {
                var context = new ApplicationDbContext();
                var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

                var deptCode = Request.Cookies["Employee"]?["DeptCode"];
                var department = _departmentRepo.GetById(deptCode);
                var oldRepEmpNum = department.RepEmpNum != null;

                var loginUser = Convert.ToInt32(Request.Cookies["Employee"]?["EmpNum"]);
                var sender = _employeeRepo.GetById(loginUser);

                var newRepEmpNum = Convert.ToInt32(repEmp);
                var newRep = _employeeRepo.GetById(newRepEmpNum);
                var newRepEmailAdd = newRep.EmailAddress;
                newRep.JobTitle = Role.Representative;

                if (!oldRepEmpNum)
                {
                    //update two tables: Employee and Department
                    _employeeRepo.Update(newRep);
                    department.RepEmpNum = newRepEmpNum;
                    _departmentRepo.Update(department);
                    
                    //update aspnetroles
                    var newRepUser = context.Users.FirstOrDefault(u => u.Email == newRep.EmailAddress);
                    userManager.RemoveFromRole(newRepUser?.Id, Role.Staff);
                    userManager.AddToRole(newRepUser?.Id, Role.Representative);
                }
                else
                {
                    //switch roles rep and staff. update both aspnetroles and jobtitles
                    var oldEmpNum = department.RepEmpNum;
                    var oldRep = _employeeRepo.GetById((int)oldEmpNum);
                    var oldRepEmailAdd = oldRep.EmailAddress;
                    oldRep.JobTitle = Role.Staff;

                    var oldRepUser = context.Users.FirstOrDefault(u => u.Email == oldRep.EmailAddress);
                    userManager.RemoveFromRole(oldRepUser?.Id, Role.Representative);
                    userManager.AddToRole(oldRepUser?.Id, Role.Staff);

                    _employeeRepo.Update(newRep);
                    _employeeRepo.Update(oldRep);

                    department.RepEmpNum = newRep.EmpNum;
                    _departmentRepo.Update(department);

                    var newRepUser = context.Users.FirstOrDefault(u => u.Email == newRep.EmailAddress);
                    userManager.RemoveFromRole(newRepUser?.Id, Role.Staff);
                    userManager.AddToRole(newRepUser?.Id, Role.Representative);

                    //email to old rep
                    var emailToOldRep = new LUSSISEmail.Builder().From(sender.EmailAddress)
                    .To(oldRepEmailAdd).ForOldRepresentative().Build();
                    new System.Threading.Thread(delegate () { EmailHelper.SendEmail(emailToOldRep); }).Start();
                }

                //email to new rep
                var emailToNewRep = new LUSSISEmail.Builder().From(sender.EmailAddress)
                    .To(newRepEmailAdd).ForNewRepresentative().Build();
                new System.Threading.Thread(delegate () { EmailHelper.SendEmail(emailToNewRep); }).Start();

            }

            return RedirectToAction("DeptRep");
        }

        [CustomAuthorize(Role.DepartmentHead)]
        [HttpPost]
        public ActionResult AddDelegate(string delegateEmp, string from, string to)
        {
            if (ModelState.IsValid)
            {
                var empNum = Convert.ToInt32(delegateEmp);
                var newDelegate = _employeeRepo.GetById(empNum);
                var newDelegateEmailAdd = newDelegate.EmailAddress;
                var startDate = DateTime.ParseExact(from, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                var endDate = DateTime.ParseExact(to, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                var loginUser = Convert.ToInt32(Request.Cookies["Employee"]?["EmpNum"]);
                var sender = _employeeRepo.GetById(loginUser);

                var del = new Models.Delegate()
                {
                    EmpNum = empNum,
                    StartDate = startDate,
                    EndDate = endDate
                };

                _delegateRepo.Add(del);

                //email to new delegate
                var emailToNewDelegate = new LUSSISEmail.Builder().From(sender.EmailAddress)
                    .To(newDelegateEmailAdd).ForNewDelegate().Build();

                new System.Threading.Thread(delegate () { EmailHelper.SendEmail(emailToNewDelegate); }).Start();
            }

            return RedirectToAction("MyDelegate");
        }

        [CustomAuthorize(Role.DepartmentHead)]
        [HttpPost]
        public ActionResult DeleteDelegate()
        {
            if (ModelState.IsValid)
            {
                var deptCode = Request.Cookies["Employee"]?["DeptCode"];
                var oldDelegate = _delegateRepo.FindExistingByDeptCode(deptCode);
                var oldDelegateEmailAdd = oldDelegate.Employee.EmailAddress;

                var loginUser = Convert.ToInt32(Request.Cookies["Employee"]?["EmpNum"]);
                var sender = _employeeRepo.GetById(loginUser);

                //email to old delegate
                var emailToOldDelegate = new LUSSISEmail.Builder().From(sender.EmailAddress)
                    .To(oldDelegateEmailAdd).ForOldDelegate().Build();

                new System.Threading.Thread(delegate () { EmailHelper.SendEmail(emailToOldDelegate); }).Start();

                _delegateRepo.DeleteByDeptCode(deptCode);             
            }

            return RedirectToAction("Index");
        }

        [CustomAuthorize(Role.DepartmentHead)]
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