using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LUSSIS.Models;

namespace LUSSIS.Controllers
{
    //TODO: THIS CLASS IS NOT COMPLETED

    public class DisbursementController : Controller
    {
        private LUSSISContext db = new LUSSISContext();

        // GET: Disbursement
        public ActionResult Index()
        {
            var disbursements = db.Disbursements.Include(d => d.AcknowledgeEmployee).Include(d => d.CollectionPoint).Include(d => d.Department);
            return View(disbursements.ToList());
        }

        // GET: Disbursement/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Disbursement disbursement = db.Disbursements.Find(id);
            if (disbursement == null)
            {
                return HttpNotFound();
            }
            return View(disbursement);
        }

        // GET: Disbursement/Create
        public ActionResult Create()
        {
            ViewBag.AcknowledgeEmpNum = new SelectList(db.Employees, "EmpNum", "Title");
            ViewBag.CollectionPointId = new SelectList(db.CollectionPoints, "CollectionPointId", "CollectionName");
            ViewBag.DeptCode = new SelectList(db.Departments, "DeptCode", "DeptName");
            return View();
        }

        // POST: Disbursement/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "DisbursementId,CollectionDate,CollectionPointId,DeptCode,AcknowledgeEmpNum,Status")] Disbursement disbursement)
        {
            if (ModelState.IsValid)
            {
                db.Disbursements.Add(disbursement);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AcknowledgeEmpNum = new SelectList(db.Employees, "EmpNum", "Title", disbursement.AcknowledgeEmpNum);
            ViewBag.CollectionPointId = new SelectList(db.CollectionPoints, "CollectionPointId", "CollectionName", disbursement.CollectionPointId);
            ViewBag.DeptCode = new SelectList(db.Departments, "DeptCode", "DeptName", disbursement.DeptCode);
            return View(disbursement);
        }

        // GET: Disbursement/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Disbursement disbursement = db.Disbursements.Find(id);
            if (disbursement == null)
            {
                return HttpNotFound();
            }
            ViewBag.AcknowledgeEmpNum = new SelectList(db.Employees, "EmpNum", "Title", disbursement.AcknowledgeEmpNum);
            ViewBag.CollectionPointId = new SelectList(db.CollectionPoints, "CollectionPointId", "CollectionName", disbursement.CollectionPointId);
            ViewBag.DeptCode = new SelectList(db.Departments, "DeptCode", "DeptName", disbursement.DeptCode);
            return View(disbursement);
        }

        // POST: Disbursement/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: Disbursement/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Disbursement disbursement = db.Disbursements.Find(id);
            if (disbursement == null)
            {
                return HttpNotFound();
            }
            return View(disbursement);
        }

        // POST: Disbursement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Disbursement disbursement = db.Disbursements.Find(id);
            db.Disbursements.Remove(disbursement);
            db.SaveChanges();
            return RedirectToAction("Index");
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
