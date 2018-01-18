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
    
[Authorize]
    public class CollectionController : Controller
    {
        DisbursementRepository disbursementRepo = new DisbursementRepository();
        EmployeeRepository employeeRepo = new EmployeeRepository();
        Repository<CollectionPoint, int> repo = new Repository<CollectionPoint, int>();

        public ActionResult Index()
        {
            string userName = User.Identity.GetUserName();
            string employeeDept = employeeRepo.GetEmployeeByEmail(userName).DeptCode;          
            Disbursement disbursement = disbursementRepo.GetByDateAndDeptCode(DateTime.Now, employeeDept);
            return View(disbursement);
        }

        public ActionResult SetCollection()
        {
            ManageCollectionDTO mcdto = new ManageCollectionDTO();
            string userName = User.Identity.GetUserName();
            string employeeDept = employeeRepo.GetEmployeeByEmail(userName).DeptCode;
            mcdto.Disbursement = disbursementRepo.GetByDateAndDeptCode(DateTime.Now, employeeDept);
            mcdto.CollectionPoint = disbursementRepo.GetCollectionPointByDeptCode(employeeDept);
            mcdto.GetAll = repo.GetAll();
  
            return View(mcdto);
        }

        // GET: Collection/Create
        public ActionResult Create()
        {
            return View();
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
