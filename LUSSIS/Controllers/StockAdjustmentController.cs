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
using System.Text;
using LUSSIS.Emails;

namespace LUSSIS.Controllers
{
    [Authorize(Roles = "clerk, supervisor, manager")]
    public class StockAdjustmentController : Controller
    {
        private LUSSISContext db = new LUSSISContext();
        private readonly StationeryRepository _stationeryRepo = new StationeryRepository();
        private readonly StockAdjustmentRepository _adjustmentRepo = new StockAdjustmentRepository();
        private readonly EmployeeRepository _employeeRepo = new EmployeeRepository();

        // GET: StockAdjustment
        public ActionResult Index()
        {
            return RedirectToAction("History");
        }

        public ActionResult History(string searchString, string currentFilter, int? page)
        {
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            var adjustments = !string.IsNullOrEmpty(searchString)
                ? _adjustmentRepo.GetAllAdjVoucherSearch(searchString)
                : _adjustmentRepo.GetAll().ToList();

            var reqAll = adjustments.ToPagedList(pageNumber: page ?? 1, pageSize: 15);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_History", reqAll);
            }

            return View(reqAll);
        }

        [Authorize(Roles = "supervisor, manager")]
        public ActionResult Approve(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            AdjVoucher adjVoucher = _adjustmentRepo.GetById((int) id);
            if (adjVoucher == null)
            {
                return HttpNotFound();
            }

            ViewBag.ApprovalEmpNum =
                new SelectList(_employeeRepo.GetAll(), "EmpNum", "Title", adjVoucher.ApprovalEmpNum);
            ViewBag.RequestEmpNum = new SelectList(_employeeRepo.GetAll(), "EmpNum", "Title", adjVoucher.RequestEmpNum);
            ViewBag.ItemNum = new SelectList(_employeeRepo.GetAll(), "ItemNum", "Description", adjVoucher.ItemNum);
            return View(adjVoucher);
        }

        [Authorize(Roles = "supervisor, manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Approve(
            [Bind(Include =
                "AdjVoucherId,ItemNum,ApprovalEmpNum,Quantity,Reason,CreateDate,ApprovalDate,RequestEmpNum,Status,Remark")]
            AdjVoucher adjVoucher)
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
            return View(_adjustmentRepo.GetPendingAdjustmentList());
        }

        [Authorize(Roles = "clerk")]
        [HttpGet]
        public ActionResult CreateAdjustments()
        {
            var adjVoucherColView = new AdjVoucherColView();
            var adjustmentVoucherDtos = new List<AdjustmentVoucherDTO>();
            var adjustmentVoucherDto = new AdjustmentVoucherDTO();
            adjustmentVoucherDtos.Add(adjustmentVoucherDto);
            adjVoucherColView.MyList = adjustmentVoucherDtos;
            return View("CreateAdjustments", adjVoucherColView);
        }

        [Authorize(Roles = "clerk")]
        [HttpPost]
        public ActionResult CreateAdjustments(AdjVoucherColView adjVoucherColView)
        {
            var empNum = Convert.ToInt32(Request.Cookies["Employee"]?["EmpNum"]);
            var self = _employeeRepo.GetById(empNum);

            if (ModelState.IsValid)
            {
                if (adjVoucherColView.MyList != null)
                {
                    var vouchers = new List<AdjVoucher>();
                    foreach (var adjVoucherDto in adjVoucherColView.MyList)
                    {
                        if (adjVoucherDto.Sign == false)
                        {
                            adjVoucherDto.Quantity = adjVoucherDto.Quantity * -1;
                        }

                        var adjustment = new AdjVoucher
                        {
                            ItemNum = adjVoucherDto.ItemNum,
                            Quantity = adjVoucherDto.Quantity,
                            Reason = adjVoucherDto.Reason,
                            Status = "pending",
                            RequestEmpNum = empNum,
                            CreateDate = DateTime.Today
                        };

                        _adjustmentRepo.Add(adjustment);

                        vouchers.Add(adjustment);
                    }

                    //Although there is a threshold of $250, both supervisor and manager will be informed of all adjustments regardless of price
                    //If desired, the threshold can be applied by getting price * quantity and setting if (total price > 250) 
                    var managerEmail = _employeeRepo.GetStoreManager().EmailAddress;
                    var supervisorEmail = _employeeRepo.GetStoreSupervisor().EmailAddress;
                    var email1 = new LUSSISEmail.Builder().From(self.EmailAddress)
                        .To(managerEmail).ForStockAdjustments(self.FullName, vouchers).Build();
                    var email2 = new LUSSISEmail.Builder().From(self.EmailAddress)
                        .To(supervisorEmail).ForStockAdjustments(self.FullName, vouchers).Build();

                    EmailHelper.SendEmail(email1);
                    EmailHelper.SendEmail(email2);

                    return RedirectToAction("History");
                }

                return View(adjVoucherColView);
            }

            return View(adjVoucherColView);
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

            var adj = new AdjustmentVoucherDTO
            {
                ItemNum = id,
                Stationery = _stationeryRepo.GetById(id)
            };

            return View(adj);
        }

        [Authorize(Roles = "clerk")]
        [HttpPost]
        public ActionResult CreateAdjustment([Bind(Include = "Quantity,Reason,ItemNum,Sign")]
            AdjustmentVoucherDTO adjVoucherDto)
        {
            if (ModelState.IsValid)
            {
                var empNum = Convert.ToInt32(Request.Cookies["Employee"]?["EmpNum"]);
                var self = _employeeRepo.GetById(empNum);

                if (adjVoucherDto.Sign == false)
                {
                    adjVoucherDto.Quantity = adjVoucherDto.Quantity * -1;
                }

                var adjustment = new AdjVoucher
                {
                    ItemNum = adjVoucherDto.ItemNum,
                    Quantity = adjVoucherDto.Quantity,
                    Reason = adjVoucherDto.Reason,
                    Status = "pending",
                    RequestEmpNum = empNum,
                    CreateDate = DateTime.Today
                };

                _adjustmentRepo.Add(adjustment);

                var managerEmail = _employeeRepo.GetStoreManager().EmailAddress;
                var supervisorEmail = _employeeRepo.GetStoreSupervisor().EmailAddress;
                var email1 = new LUSSISEmail.Builder().From(self.EmailAddress)
                    .To(managerEmail).ForStockAdjustment(self.FullName, adjustment).Build();
                var email2 = new LUSSISEmail.Builder().From(self.EmailAddress)
                    .To(supervisorEmail).ForStockAdjustment(self.FullName, adjustment).Build();

                EmailHelper.SendEmail(email1);
                EmailHelper.SendEmail(email2);

                return RedirectToAction("History");
            }

            adjVoucherDto.Stationery = _stationeryRepo.GetById(adjVoucherDto.ItemNum);
            return View(adjVoucherDto);
        }

        [Authorize(Roles = "clerk")]
        [HttpGet]
        public JsonResult GetItemNum(string term)
        {
            List<String> itemList;
            if (string.IsNullOrEmpty(term))
            {
                itemList = _stationeryRepo.GetAllItemNum().ToList();
            }
            else
            {
                itemList = _stationeryRepo.GetAllItemNum().ToList()
                    .FindAll(x => x.StartsWith(term, StringComparison.OrdinalIgnoreCase));
            }

            return Json(itemList, JsonRequestBehavior.AllowGet);
        }


        [Authorize(Roles = "manager,supervisor")]
        public ActionResult ViewPendingStockAdj()
        {
            var user = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            ViewBag.Message = user.GetRoles(System.Web.HttpContext.Current.User.Identity.GetUserId()).First()
                .ToString();
            return View(_adjustmentRepo.ViewPendingStockAdj(ViewBag.Message));
        }

        [Authorize(Roles = "manager,supervisor")]
        [HttpGet]
        public ActionResult ApproveReject(String List, String Status)
        {
            //  List<AdjVoucher> list = _adjustmentRepo.GetAdjustmentById(List);
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
                _adjustmentRepo.UpDateAdjustmentStatus(i, status, comment);
            }

            return PartialView();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _adjustmentRepo.Dispose();
                _stationeryRepo.Dispose();
                _employeeRepo.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}