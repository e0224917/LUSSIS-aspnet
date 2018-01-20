using LUSSIS.Models;
using LUSSIS.Models.WebDTO;
using LUSSIS.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LUSSIS.Controllers
{
    public class RepAndDelegateController : Controller
    {
        EmployeeRepository employeeRepo = new EmployeeRepository();
        RepAndDelegateDTO raddto = new RepAndDelegateDTO();

        // GET: RepAndDelegate
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DeptRep()
        {
            raddto.Department = employeeRepo.GetDepartmentByUser(employeeRepo.GetCurrentUser());
            raddto.GetAllByDepartment = employeeRepo.GetAllByDepartment(raddto.Department);
            return View(raddto);
        }

        [HttpGet]
        public JsonResult GetEmpJson(string prefix)
        {
            raddto.Department = employeeRepo.GetDepartmentByUser(employeeRepo.GetCurrentUser());
            //List<Employee> empList = employeeRepo.GetAllByDepartment(raddto.Department);
            raddto.GetAllByDepartment = employeeRepo.GetAllByDepartment(raddto.Department);
            var list = raddto.GetAllByDepartment.Where(x => x.FullName.Contains(prefix)).ToList();

            var selectedEmp = list.Select(x => new
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
