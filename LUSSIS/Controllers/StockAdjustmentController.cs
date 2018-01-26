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
using PagedList;

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
        public ActionResult Index()
        {
            return RedirectToAction("History");
        }

        public ActionResult History(string searchString, string currentFilter, int? page)
        {
            List<AdjVoucher> adjustments = new List<AdjVoucher>();
            if (searchString != null)
            { page = 1; }
            else
            {
                searchString = currentFilter;
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                adjustments = sar.GetAllAdjVoucherSearch(searchString);
            }
            else
            {
                adjustments = sar.GetAll().ToList();
            }
            int pageSize = 15;
            int pageNumber = (page ?? 1);

            var reqAll = adjustments.ToPagedList(pageNumber, pageSize);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_History", reqAll);
            }

            return View(reqAll);
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
                return RedirectToAction("History");
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
                        if (AVDTO.Sign == false)
                        {
                            AVDTO.Quantity = AVDTO.Quantity * -1;
                        }
                        AdjVoucher Adj = new AdjVoucher
                        {
                            ItemNum = AVDTO.ItemNum,
                            Quantity = AVDTO.Quantity,
                            Reason = AVDTO.Reason,
                            RequestEmpNum = ENum,
                            CreateDate = todayDate,
                            Status = "pending"
                        };
                        sar.Add(Adj);
                    }
                }
                return RedirectToAction("History");
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
                if (adjVoucher.Sign == false)
                { adjVoucher.Quantity = adjVoucher.Quantity * -1; }
                var adj = new AdjVoucher
                {
                    RequestEmpNum = er.GetCurrentUser().EmpNum,
                    ItemNum = adjVoucher.ItemNum,
                    CreateDate = DateTime.Today,
                    Quantity = adjVoucher.Quantity,
                    Reason = adjVoucher.Reason,
                    Status = "pending"
                };
                sar.Add(adj);
                return RedirectToAction("History");
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
