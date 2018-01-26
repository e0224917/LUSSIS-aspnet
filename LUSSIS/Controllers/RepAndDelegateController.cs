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

namespace LUSSIS.Controllers
{
    [CustomAuthorize("head", "staff")]
    public class RepAndDelegateController : Controller
    {
        EmployeeRepository employeeRepo = new EmployeeRepository();
        RepAndDelegateDTO radDto = new RepAndDelegateDTO();
        DelegateRepository delegateRepo = new DelegateRepository();
        DeptHeadDashBoardDTO dbDto = new DeptHeadDashBoardDTO();
        RequisitionRepository reqRepo = new RequisitionRepository();

        //for delegate and head only
        public ActionResult Index()
        {
            dbDto.GetCurrentLoggedIn = employeeRepo.GetCurrentUser();
            dbDto.Department = employeeRepo.GetDepartmentByUser(dbDto.GetCurrentLoggedIn);
            dbDto.GetDelegate = employeeRepo.GetFutureDelegate(dbDto.Department, DateTime.Now.Date);
            dbDto.GetStaffRepByDepartment = employeeRepo.GetStaffRepByDepartment(dbDto.Department);
            dbDto.GetRequisitionListCount = reqRepo.GetPendingListForHead(dbDto.Department.DeptCode).Count();
            dbDto.GetTodaysDelegate = employeeRepo.GetDelegateByDate(dbDto.Department, DateTime.Now.Date);

            return View(dbDto);
        }

        [HeadWithDelegateAuth("head", "staff")]
        public ActionResult DeptRep()
        {
            radDto.Department = employeeRepo.GetDepartmentByUser(employeeRepo.GetCurrentUser());
            radDto.GetStaffRepByDepartment = employeeRepo.GetStaffRepByDepartment(radDto.Department);
            return View(radDto);
        }

        [HttpGet]
        public JsonResult GetEmpJson(string prefix)
        {
            radDto.Department = employeeRepo.GetDepartmentByUser(employeeRepo.GetCurrentUser());
            var selectedlist = employeeRepo.GetSelectionByDepartment(prefix, radDto.Department);
            var selectedEmp = selectedlist.Select(x => new
            {
                FullName = x.FullName,
                EmpNum = x.EmpNum
            });   
            
            return Json(selectedEmp, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize("head")]
        [HttpGet]
        public JsonResult GetEmpForDelJson(string prefix)
        {
            radDto.Department = employeeRepo.GetDepartmentByUser(employeeRepo.GetCurrentUser());
            var selectedlist = employeeRepo.GetDelSelectionByDepartment(prefix, radDto.Department);
            var selectedEmp = selectedlist.Select(x => new
            {
                FullName = x.FullName,
                EmpNum = x.EmpNum
            });
            return Json(selectedEmp, JsonRequestBehavior.AllowGet);
        }

        [HeadWithDelegateAuth("head", "staff")]
        [HttpPost]
        public ActionResult UpdateRep(string repEmp)
        {
            if (ModelState.IsValid)
            {
                string employeeDept = employeeRepo.GetCurrentUser().DeptCode;
                Department department = employeeRepo.GetDepartmentByUser(employeeRepo.GetCurrentUser());
                if (department.RepEmployee == null)
                {
                    employeeRepo.AddRep(department, repEmp);
                }
                else
                {
                    string emailRepOld = department.RepEmployee.EmailAddress;
                    var context = new ApplicationDbContext();
                    var user = context.Users.FirstOrDefault(u => u.Email == emailRepOld);
                    var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                    userManager.RemoveFromRole(user.Id, "rep");
                    userManager.AddToRole(user.Id, "staff");
                    employeeRepo.ChangeRep(department, repEmp);
                    string emailRepNew = department.RepEmployee.EmailAddress;
                    var user2 = context.Users.FirstOrDefault(u => u.Email == emailRepNew);
                    userManager.RemoveFromRole(user2.Id, "staff");
                    userManager.AddToRole(user2.Id, "rep");
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
                string employeeDept = employeeRepo.GetCurrentUser().DeptCode;
                Department department = employeeRepo.GetDepartmentByUser(employeeRepo.GetCurrentUser());
                Models.Delegate del = new Models.Delegate();
                del.EmpNum = Convert.ToInt32(delegateEmp);
                var startDate = DateTime.ParseExact(from, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                var endDate = DateTime.ParseExact(to, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                del.StartDate = startDate;
                del.EndDate = endDate;
                delegateRepo.Add(del);      
            }
            return RedirectToAction("MyDelegate");
        }

        [CustomAuthorize("head")]
        [HttpPost]
        public ActionResult DeleteDelegate()
        {
            if(ModelState.IsValid)
            {
                string employeeDept = employeeRepo.GetCurrentUser().DeptCode;
                Department department = employeeRepo.GetDepartmentByUser(employeeRepo.GetCurrentUser());
                employeeRepo.DeleteDelegate(department);
            }
            return RedirectToAction("MyDelegate");
        }

        [CustomAuthorize("head")]
        [HttpPost]
        public ActionResult DeleteDelegateFromDB()
        {
            if (ModelState.IsValid)
            {
                string employeeDept = employeeRepo.GetCurrentUser().DeptCode;
                Department department = employeeRepo.GetDepartmentByUser(employeeRepo.GetCurrentUser());
                employeeRepo.DeleteDelegate(department);
            }
            return RedirectToAction("Index");
        }

        [CustomAuthorize("head")]
        public ActionResult MyDelegate()
        {
            radDto.Department = employeeRepo.GetDepartmentByUser(employeeRepo.GetCurrentUser());
            radDto.GetDelegate = employeeRepo.GetFutureDelegate(radDto.Department, DateTime.Now.Date);
            return View(radDto);
        }

        [HttpPost]
        public ActionResult DirectToRequisitons()
        {
            return RedirectToAction("_ApproveReq", "Requisitions");
        }

        // GET: RepAndDelegate/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: RepAndDelegate/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: RepAndDelegate/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: RepAndDelegate/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: RepAndDelegate/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: RepAndDelegate/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: RepAndDelegate/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
