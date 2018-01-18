using LUSSIS.Models;
using LUSSIS.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace LUSSIS.Controllers
{
    public class RequisitionController : Controller
    {
        private LUSSISContext db = new LUSSISContext();
        private RequisitionRepository rr = new RequisitionRepository();
        private EmployeeRepository er = new EmployeeRepository();

        // GET: Requisition
        public ActionResult Pending()
        {
            return View(rr.GetRequisitionsByStatus("pending"));
        }


        [HttpGet]
        public async Task<ActionResult> Detail(int reqId)
        {
            var req = await rr.GetByIdAsync(reqId);
            if (req != null)
            {
                return View(req);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest); ;
        }
        [HttpPost]
        public async Task<ActionResult> Detail([Bind(Include = "RequisitionId,RequisitionEmpNum,RequisitionDate,RequestRemarks,ApprovalRemarks")] Requisition requisition, string SubmitButton)
        {
            if (ModelState.IsValid)
            {
                requisition.Status = "approved";
                requisition.ApprovalEmpNum = er.GetCurrentUser().EmpNum;
                requisition.ApprovalDate = DateTime.Today;
                if (SubmitButton == "Approve")
                {

                    await rr.UpdateAsync(requisition);
                    return RedirectToAction("index");
                }

                if (SubmitButton == "Reject")
                {

                    await rr.UpdateAsync(requisition);
                    return RedirectToAction("index");
                }
            }
            return RedirectToAction("index");
        }


        // GET: Requisition/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Requisition/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Requisition/Create
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

        // GET: Requisition/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Requisition/Edit/5
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

        // GET: Requisition/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Requisition/Delete/5
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
        //Stock Clerk's page
        public ActionResult Consolidated()
        {
            return View(rr.GetConsolidatedRequisition());
        }
    }
}
