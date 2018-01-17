using LUSSIS.Models;
using LUSSIS.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace LUSSIS.Controllers
{
    public class RequisitionController : Controller
    {
        private LUSSISContext db = new LUSSISContext();
        private RequisitionRepository rr = new RequisitionRepository();
        // GET: Requisition
        public ActionResult PendingRequisition()
        {
            List<Requisition> PendingReq = rr.GetPendingRequisitions();
            return View(PendingReq);
        }

        public ActionResult Detail(int reqId)
        {
            var req = rr.GetById(reqId);
            if (req != null)
            {
                return View(req);
            }
            return HttpNotFound();
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
    }
}
