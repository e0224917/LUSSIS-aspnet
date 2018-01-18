using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LUSSIS.Models;
using LUSSIS.Repositories;

namespace LUSSIS.Controllers
{
    public class StockAdjustmentController : Controller
    {
        private LUSSISContext db = new LUSSISContext();
        StockAdjustmentRepository repo = new StockAdjustmentRepository();

        // GET: StockAdjustment
        public async Task<ActionResult> Index()
        {
            
            return View(await repo.GetAllAsync());
        }
        public async Task<ActionResult> History()
        {
            return View(await db.AdjVouchers.ToListAsync());
        }
       
        public async Task<ActionResult> Approve(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AdjVoucher adjVoucher = repo.GetById((int)id);
            if (adjVoucher == null)
            {
                return HttpNotFound();
            }
            

            ViewBag.ApprovalEmpNum = new SelectList(db.Employees, "EmpNum", "Title", adjVoucher.ApprovalEmpNum);
            ViewBag.RequestEmpNum = new SelectList(db.Employees, "EmpNum", "Title", adjVoucher.RequestEmpNum);
            ViewBag.ItemNum = new SelectList(db.Stationeries, "ItemNum", "Description", adjVoucher.ItemNum);
            return View(adjVoucher);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Approve([Bind(Include = "AdjVoucherId,ItemNum,ApprovalEmpNum,Quantity,Reason,CreateDate,ApprovalDate,RequestEmpNum,Status,Remark")] AdjVoucher adjVoucher)
        {
            if (ModelState.IsValid)
            {
                db.Entry(adjVoucher).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.ApprovalEmpNum = new SelectList(db.Employees, "EmpNum", "Title", adjVoucher.ApprovalEmpNum);
            ViewBag.RequestEmpNum = new SelectList(db.Employees, "EmpNum", "Title", adjVoucher.RequestEmpNum);
            ViewBag.ItemNum = new SelectList(db.Stationeries, "ItemNum", "Description", adjVoucher.ItemNum);
            return View(adjVoucher);
        }


        public async Task<ActionResult> AdjustmentApproveReject()
        {
            return View(repo.GetPendingAdjustmentList());
        }

      
        

        // GET: StockAdjustment/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AdjVoucher adjVoucher = await db.AdjVouchers.FindAsync(id);
            if (adjVoucher == null)
            {
                return HttpNotFound();
            }
            return View(adjVoucher);
        }

        // GET: StockAdjustment/Create
        public ActionResult Create()
        {
            ViewBag.ApprovalEmpNum = new SelectList(db.Employees, "EmpNum", "Title");
            ViewBag.RequestEmpNum = new SelectList(db.Employees, "EmpNum", "Title");
            ViewBag.ItemNum = new SelectList(db.Stationeries, "ItemNum", "Description");
            return View();
        }

        // POST: StockAdjustment/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "AdjVoucherId,ItemNum,ApprovalEmpNum,Quantity,Reason,CreateDate,ApprovalDate,RequestEmpNum,Status,Remark")] AdjVoucher adjVoucher)
        {
            if (ModelState.IsValid)
            {
                db.AdjVouchers.Add(adjVoucher);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.ApprovalEmpNum = new SelectList(db.Employees, "EmpNum", "Title", adjVoucher.ApprovalEmpNum);
            ViewBag.RequestEmpNum = new SelectList(db.Employees, "EmpNum", "Title", adjVoucher.RequestEmpNum);
            ViewBag.ItemNum = new SelectList(db.Stationeries, "ItemNum", "Description", adjVoucher.ItemNum);
            return View(adjVoucher);
        }

        // GET: StockAdjustment/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AdjVoucher adjVoucher = await db.AdjVouchers.FindAsync(id);
            if (adjVoucher == null)
            {
                return HttpNotFound();
            }
            ViewBag.ApprovalEmpNum = new SelectList(db.Employees, "EmpNum", "Title", adjVoucher.ApprovalEmpNum);
            ViewBag.RequestEmpNum = new SelectList(db.Employees, "EmpNum", "Title", adjVoucher.RequestEmpNum);
            ViewBag.ItemNum = new SelectList(db.Stationeries, "ItemNum", "Description", adjVoucher.ItemNum);
            return View(adjVoucher);
        }

        // POST: StockAdjustment/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "AdjVoucherId,ItemNum,ApprovalEmpNum,Quantity,Reason,CreateDate,ApprovalDate,RequestEmpNum,Status,Remark")] AdjVoucher adjVoucher)
        {
            if (ModelState.IsValid)
            {
                db.Entry(adjVoucher).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.ApprovalEmpNum = new SelectList(db.Employees, "EmpNum", "Title", adjVoucher.ApprovalEmpNum);
            ViewBag.RequestEmpNum = new SelectList(db.Employees, "EmpNum", "Title", adjVoucher.RequestEmpNum);
            ViewBag.ItemNum = new SelectList(db.Stationeries, "ItemNum", "Description", adjVoucher.ItemNum);
            return View(adjVoucher);
        }

        // GET: StockAdjustment/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AdjVoucher adjVoucher = await db.AdjVouchers.FindAsync(id);
            if (adjVoucher == null)
            {
                return HttpNotFound();
            }
            return View(adjVoucher);
        }

        // POST: StockAdjustment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            AdjVoucher adjVoucher = await db.AdjVouchers.FindAsync(id);
            db.AdjVouchers.Remove(adjVoucher);
            await db.SaveChangesAsync();
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
