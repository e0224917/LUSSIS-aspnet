using LUSSIS.Models;
using LUSSIS.Models.WebDTO;
using LUSSIS.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;

namespace LUSSIS.Controllers
{
    [Authorize(Roles = "clerk, supervisor, manager")]
    public class StockAdjustmentController : Controller
    {
        private LUSSISContext db = new LUSSISContext();
        private StationeryRepository sr = new StationeryRepository();
        private StockAdjustmentRepository sar = new StockAdjustmentRepository();
        private EmployeeRepository er = new EmployeeRepository();

        // GET: StockAdjustment
        public async Task<ActionResult> Index()
        {

            return View(await sar.GetAllAsync());
        }
        public async Task<ActionResult> History()
        {
            return View(await db.AdjVouchers.ToListAsync());
        }
        public ActionResult Details(int id)
        {
            return View();
        }

        [Authorize(Roles = "supervisor, manager")]
        public ActionResult Approve(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AdjVoucher adjVoucher = sar.GetById((int)id);
            if (adjVoucher == null)
            {
                return HttpNotFound();
            }


            ViewBag.ApprovalEmpNum = new SelectList(db.Employees, "EmpNum", "Title", adjVoucher.ApprovalEmpNum);
            ViewBag.RequestEmpNum = new SelectList(db.Employees, "EmpNum", "Title", adjVoucher.RequestEmpNum);
            ViewBag.ItemNum = new SelectList(db.Stationeries, "ItemNum", "Description", adjVoucher.ItemNum);
            return View(adjVoucher);
        }

        [Authorize(Roles = "supervisor, manager")]
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

        [Authorize(Roles = "supervisor, manager")]
        public ActionResult AdjustmentApproveReject()
        {
            return View(sar.GetPendingAdjustmentList());
        }


        // GET: StockAdjustment/Create
        [Authorize(Roles = "clerk")]
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
        [Authorize(Roles = "clerk")]
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
        //???? think this is autogene
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
        //autogen?
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
        //autogen?
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
        //???
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
        [Authorize(Roles = "clerk")]
        [HttpGet]
        public ActionResult CreateAdjustments()
        {
            AdjVoucherColView aVCV = new AdjVoucherColView();
            List<AdjustmentVoucherDTO> aVlist = new List<AdjustmentVoucherDTO>();
            AdjustmentVoucherDTO aV = new AdjustmentVoucherDTO();
            //aV.ItemNum = "C001";
            //aV.Quantity = 3;
            aVlist.Add(aV);
            aVCV.MyList = aVlist;
            return View("CreateAdjustments", aVCV);
        }
        [Authorize(Roles = "clerk")]
        [HttpPost]
        public ActionResult CreateAdjustments(AdjVoucherColView kk)
        {
            int ENum = er.GetCurrentUser().EmpNum;
            DateTime todayDate = DateTime.Today;
            if (ModelState.IsValid)
            {
                if (kk.MyList != null)
                {
                    foreach (AdjustmentVoucherDTO AVDTO in kk.MyList)
                    {
                        AdjVoucher Adj = new AdjVoucher();
                        if (AVDTO.Sign == false)
                        {
                            AVDTO.Quantity = AVDTO.Quantity * -1;
                        }
                        Adj.ItemNum = AVDTO.ItemNum;
                        Adj.Quantity = AVDTO.Quantity;
                        Adj.Reason = AVDTO.Reason;
                        Adj.RequestEmpNum = ENum;
                        Adj.CreateDate = todayDate;
                        Adj.Status = "pending";
                        sar.Add(Adj);
                    }
                }
                return RedirectToAction("index");
            }
            return View(kk);
        }

        [Authorize(Roles = "clerk")]
        public PartialViewResult _CreateAdjustments()
        {
            return PartialView("_CreateAdjustments", new AdjustmentVoucherDTO());

        }

        [Authorize(Roles = "clerk")]
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
        [Authorize(Roles = "clerk")]
        [HttpPost]
        public ActionResult CreateAdjustment([Bind(Include = "Quantity,Reason,ItemNum,Sign")]AdjustmentVoucherDTO adjVoucher)
        {
            if (ModelState.IsValid)
            {
                var adj = new AdjVoucher();
                adj.RequestEmpNum = er.GetCurrentUser().EmpNum;
                adj.ItemNum = adjVoucher.ItemNum;
                adj.CreateDate = DateTime.Today;
                if (adjVoucher.Sign == false)
                { adjVoucher.Quantity = adjVoucher.Quantity * -1; }
                adj.Quantity = adjVoucher.Quantity;
                adj.Reason = adjVoucher.Reason;
                adj.Status = "pending";
                sar.Add(adj);
                return RedirectToAction("index");
            }
            else
            {
                adjVoucher.Stationery = sr.GetById(adjVoucher.ItemNum);
                return View(adjVoucher);
            }

        }

        [Authorize(Roles = "clerk")]
        [HttpGet]
        public JsonResult GetItemNum(string term)
        {
            List<String> itemList;
            if (string.IsNullOrEmpty(term))
            {
                itemList = sr.GetAllItemNum().ToList();
            }
            else
            {

                itemList = sr.GetAllItemNum().ToList().FindAll(x => x.StartsWith(term, StringComparison.OrdinalIgnoreCase));
            }
            return Json(itemList, JsonRequestBehavior.AllowGet);
        }


        [Authorize(Roles = "manager,supervisor")]
        public ActionResult ViewPendingStockAdj()
        {
            var user = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            ViewBag.Message = user.GetRoles(System.Web.HttpContext.Current.User.Identity.GetUserId()).First().ToString();
           
                return View(sar.ViewPendingStockAdj(ViewBag.Message));
            
          

        }
        [Authorize(Roles = "manager,supervisor")]
        [HttpGet]
        public ActionResult ApproveReject(String List, String Status)
        {
            //  List<AdjVoucher> list = sar.GetAdjustmentById(List);
            ViewBag.checkList = List;
            ViewBag.status = Status;
            return PartialView("ApproveReject");
        }
        [Authorize(Roles = "manager,supervisor")]
        [HttpPost]
        public ActionResult ApproveReject(String checkList, String comment, String status)
        {
            String[] list = checkList.Split(',');
            int[] idList = new int[list.Length];
            for (int i = 0; i < idList.Length; i++)
            {
                idList[i] = Int32.Parse(list[i]);
            }
            foreach (int i in idList)
            {
                sar.UpDateAdjustmentStatus(i, status, comment);
            }
            return PartialView();
        }






    }
    }
