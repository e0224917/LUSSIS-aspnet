using LUSSIS.Models;
using LUSSIS.Models.WebDTO;
using LUSSIS.Repositories;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LUSSIS.Controllers
{

[Authorize(Roles = "rep")]
    public class CollectionController : Controller
    {
        DisbursementRepository disbursementRepo = new DisbursementRepository();
        EmployeeRepository employeeRepo = new EmployeeRepository();
        CollectionRepository collectionRepo = new CollectionRepository();
 
        ManageCollectionDTO mcdto = new ManageCollectionDTO();

        public ActionResult Index()
        {
            string employeeDept = employeeRepo.GetCurrentUser().DeptCode;          
            Disbursement disbursement = disbursementRepo.GetByDateAndDeptCode(DateTime.Now, employeeDept);
            return View(disbursement);
        }

        public ActionResult SetCollection()
        {
            string employeeDept = employeeRepo.GetCurrentUser().DeptCode;
            mcdto.CollectionPoint = disbursementRepo.GetCollectionPointByDeptCode(employeeDept);
            mcdto.GetAll = collectionRepo.GetAll();

            return View(mcdto);
        }



        [HttpPost]
        public ActionResult UpdateCollection(ManageCollectionDTO mcdto)
        {
            if (ModelState.IsValid)
            {
                string employeeDept = employeeRepo.GetCurrentUser().DeptCode;
                Department department = employeeRepo.GetDepartmentByUser(employeeRepo.GetCurrentUser());
                department.CollectionPointId = mcdto.DeptCollectionPointID;
                employeeRepo.UpdateDepartment(department);
            }
            
            return RedirectToAction("SetCollection");
        }


        // POST: Collection/Create
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

        // GET: Collection/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Collection/Edit/5
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

        // GET: Collection/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Collection/Delete/5
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
