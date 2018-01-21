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

namespace LUSSIS.Controllers
{

    [Authorize(Roles = "head")]
    public class RepAndDelegateController : Controller
    {
        EmployeeRepository employeeRepo = new EmployeeRepository();
        RepAndDelegateDTO raddto = new RepAndDelegateDTO();
        DelegateRepository delegateRepo = new DelegateRepository();

        
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DeptRep()
        {
            raddto.Department = employeeRepo.GetDepartmentByUser(employeeRepo.GetCurrentUser());
            raddto.GetStaffRepByDepartment = employeeRepo.GetStaffRepByDepartment(raddto.Department);
            return View(raddto);
        }

        [HttpGet]
        public JsonResult GetEmpJson(string prefix)
        {
            raddto.Department = employeeRepo.GetDepartmentByUser(employeeRepo.GetCurrentUser());
            var selectedlist = employeeRepo.GetSelectionByDepartment(prefix, raddto.Department);
            var selectedEmp = selectedlist.Select(x => new
            {
                FullName = x.FullName,
                EmpNum = x.EmpNum
            });   
            
            return Json(selectedEmp, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetEmpForDelJson(string prefix)
        {
            raddto.Department = employeeRepo.GetDepartmentByUser(employeeRepo.GetCurrentUser());
            var selectedlist = employeeRepo.GetDelSelectionByDepartment(prefix, raddto.Department);
            var selectedEmp = selectedlist.Select(x => new
            {
                FullName = x.FullName,
                EmpNum = x.EmpNum
            });
            return Json(selectedEmp, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateRep(string repEmp)
        {
            if (ModelState.IsValid)
            {
                string employeeDept = employeeRepo.GetCurrentUser().DeptCode;
                Department department = employeeRepo.GetDepartmentByUser(employeeRepo.GetCurrentUser());
                employeeRepo.ChangeRep(department, repEmp);       
            }
            return RedirectToAction("DeptRep");
        }

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
            return RedirectToAction("DeptDelegate");
        }

        [HttpPost]
        public ActionResult DeleteDelegate()
        {
            if(ModelState.IsValid)
            {
                string employeeDept = employeeRepo.GetCurrentUser().DeptCode;
                Department department = employeeRepo.GetDepartmentByUser(employeeRepo.GetCurrentUser());
                employeeRepo.DeleteDelegate(department);
            }
            return RedirectToAction("DeptDelegate");
        }

        public ActionResult DeptDelegate()
        {
            raddto.Department = employeeRepo.GetDepartmentByUser(employeeRepo.GetCurrentUser());
            raddto.GetDelegate = employeeRepo.GetCurrentDelegate(raddto.Department);
            return View(raddto);
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
