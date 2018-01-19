using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LUSSIS.Models;
using LUSSIS.Repositories;

namespace LUSSIS.Views
{
    public class DisbursementsController : Controller
    {
        private LUSSISContext db = new LUSSISContext();
        private DisbursementRepository dr;

        // GET: Disbursements
        public ActionResult Index()
        {
            return View(dr.GetInProcessDisbursements());
        }

        // GET: Disbursements/Details/5
        public ActionResult Details(int id)
        {
            Disbursement disbursement = dr.GetById(id);
            if (disbursement == null)
            {
                return HttpNotFound();
            }
            return View(disbursement);
        }

       // GET: Disbursements/Edit/5
        public ActionResult Edit(int id)
        {
            Disbursement disbursement = dr.GetById(id);
            if (disbursement == null)
            {
                return HttpNotFound();
            }
            //TODO: activate edit on the same page
            ViewBag.AcknowledgeEmpNum = new SelectList(db.Employees, "EmpNum", "Title", disbursement.AcknowledgeEmpNum);
            ViewBag.CollectionPointId = new SelectList(db.CollectionPoints, "CollectionPointId", "CollectionName", disbursement.CollectionPointId);
            ViewBag.DeptCode = new SelectList(db.Departments, "DeptCode", "DeptName", disbursement.DeptCode);
            return View(disbursement);
        }

        // POST: Disbursements/Edit/5
        // TODO: update the specific properties we want to bind to
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "DisbursementId,CollectionDate,CollectionPointId,DeptCode,AcknowledgeEmpNum,Status")] Disbursement disbursement)
        {
            
            if (ModelState.IsValid)
            {
                db.Entry(disbursement).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AcknowledgeEmpNum = new SelectList(db.Employees, "EmpNum", "Title", disbursement.AcknowledgeEmpNum);
            ViewBag.CollectionPointId = new SelectList(db.CollectionPoints, "CollectionPointId", "CollectionName", disbursement.CollectionPointId);
            ViewBag.DeptCode = new SelectList(db.Departments, "DeptCode", "DeptName", disbursement.DeptCode);
            return View(disbursement);
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
