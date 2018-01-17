using LUSSIS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace LUSSIS.Controllers
{
    public class StockAdjustmentController : Controller
    {
        private LUSSISContext db = new LUSSISContext();

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
