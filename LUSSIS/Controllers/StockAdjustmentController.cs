using LUSSIS.Models;
using LUSSIS.Models.WebDTO;
using LUSSIS.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace LUSSIS.Controllers
{
    public class StockAdjustmentController : Controller
    {
        private LUSSISContext db = new LUSSISContext();
        private StationeryRepository sr = new StationeryRepository();
        private StockAdjustmentRepository sar = new StockAdjustmentRepository();
        private EmployeeRepository er = new EmployeeRepository();

        // GET: StockAdjustment
        public async Task<ActionResult> History()
        {
            return View(await db.AdjVouchers.ToListAsync());
        }

        // GET: StockAdjustment/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: StockAdjustment/Create
        public ActionResult CreateAdjustments()
        {
            return View();

        }


        [HttpGet]
        public ActionResult CreateAdjustment(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                AdjustmentVoucherDTO adj = new AdjustmentVoucherDTO();
                adj.ItemNum = id;
                adj.Stationery = sr.GetById(id);
                return View(adj);
            }
        }

        [HttpPost]
        public ActionResult CreateAdjustment([Bind(Include = "Quantity,Reason,ItemNum,Sign")]AdjustmentVoucherDTO adjVoucher)
        {
            if (ModelState.IsValid)
            {
                var adj = new AdjVoucher();
                adj.RequestEmpNum = er.GetCurrentUser().EmpNum;
                adj.ItemNum = adjVoucher.ItemNum;
                adj.CreateDate = DateTime.Today;
                if (adjVoucher.Sign == 1)
                { adjVoucher.Quantity = adjVoucher.Quantity * -1; }
                adj.Quantity = adjVoucher.Quantity;
                adj.Reason = adjVoucher.Reason;
                sar.Add(adj);
                return RedirectToAction("index");
            }
            else
            {
                adjVoucher.Stationery = sr.GetById(adjVoucher.ItemNum);
                return View(adjVoucher);
            }

        }



        // POST: StockAdjustment/Create
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

        // GET: StockAdjustment/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: StockAdjustment/Edit/5
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

        // GET: StockAdjustment/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: StockAdjustment/Delete/5
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
