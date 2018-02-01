using LUSSIS.Models;
using LUSSIS.Models.WebDTO;
using LUSSIS.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LUSSIS.Constants;
using PagedList;
using LUSSIS.Emails;
using static LUSSIS.Constants.AdjustmentVoucherStatus;

namespace LUSSIS.Controllers
{
    //Authors: Koh Meng Guan, May Zin Ko
    [Authorize(Roles = "clerk, supervisor, manager")]
    public class StockAdjustmentController : Controller
    {
        private readonly StationeryRepository _stationeryRepo = new StationeryRepository();
        private readonly StockAdjustmentRepository _stockAdjustmentRepo = new StockAdjustmentRepository();
        private readonly EmployeeRepository _employeeRepo = new EmployeeRepository();

        // GET: StockAdjustment
        public ActionResult Index()
        {
            return RedirectToAction("History");
        }

        //Author: Koh Meng Guan
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

            ViewBag.CurrentFilter = searchString;

            var adjustments = !string.IsNullOrEmpty(searchString)
                ? _stockAdjustmentRepo.FindAdjVoucherByText(searchString).Reverse().ToList()
                : _stockAdjustmentRepo.GetAll().Reverse().ToList();

            var reqAll = adjustments.ToPagedList(pageNumber: page ?? 1, pageSize: 15);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_History", reqAll);
            }

            return View(reqAll);
        }


        //Author: Koh Meng Guan
        [Authorize(Roles = Role.Clerk)]
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

        //Author: Koh Meng Guan
        [Authorize(Roles = Role.Clerk)]
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
                            Status = Pending,
                            RequestEmpNum = empNum,
                            CreateDate = DateTime.Today
                        };

                        adjustment.Stationery = _stockAdjustmentRepo.AddStockAdjustment(adjustment);
                        vouchers.Add(adjustment);
                    }
                    //Although there is a threshold of $250, both supervisor and manager will be informed of all adjustments regardless of price
                    //If desired, the threshold can be applied by getting price * quantity and setting if (total price > 250) 
                    foreach (AdjVoucher av in vouchers)
                    {
                        av.Stationery = _stationeryRepo.GetById(av.ItemNum);
                    }
                    var managerEmail = _employeeRepo.GetStoreManager().EmailAddress;
                    var supervisorEmail = _employeeRepo.GetStoreSupervisor().EmailAddress;
                    var email1 = new LUSSISEmail.Builder().From(self.EmailAddress)
                        .To(managerEmail).ForNewStockAdjustments(self.FullName, vouchers).Build();
                    var email2 = new LUSSISEmail.Builder().From(self.EmailAddress)
                        .To(supervisorEmail).ForNewStockAdjustments(self.FullName, vouchers).Build();

                    new System.Threading.Thread(delegate () { EmailHelper.SendEmail(email1); }).Start();
                    new System.Threading.Thread(delegate () { EmailHelper.SendEmail(email2); }).Start();

                    return RedirectToAction("History");
                }

                return View(adjVoucherColView);
            }

            return View(adjVoucherColView);
        }

        //Author: Koh Meng Guan
        [Authorize(Roles = Role.Clerk)]
        public PartialViewResult _CreateAdjustments()
        {
            return PartialView("_CreateAdjustments", new AdjustmentVoucherDTO());
        }

        //Author: Koh Meng Guan
        [Authorize(Roles = Role.Clerk)]
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

        //Author: Koh Meng Guan
        [Authorize(Roles = Role.Clerk)]
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
                    Status = Pending,
                    RequestEmpNum = empNum,
                    CreateDate = DateTime.Today
                   
                };

                adjustment.Stationery = _stockAdjustmentRepo.AddStockAdjustment(adjustment);

                var managerEmail = _employeeRepo.GetStoreManager().EmailAddress;
                var supervisorEmail = _employeeRepo.GetStoreSupervisor().EmailAddress;
                var email1 = new LUSSISEmail.Builder().From(self.EmailAddress)
                    .To(managerEmail).ForNewStockAdjustment(self.FullName, adjustment).Build();
                var email2 = new LUSSISEmail.Builder().From(self.EmailAddress)
                    .To(supervisorEmail).ForNewStockAdjustment(self.FullName, adjustment).Build();

                new System.Threading.Thread(delegate () { EmailHelper.SendEmail(email1); }).Start();
                new System.Threading.Thread(delegate () { EmailHelper.SendEmail(email2); }).Start();

                return RedirectToAction("History");
            }

            adjVoucherDto.Stationery = _stationeryRepo.GetById(adjVoucherDto.ItemNum);
            return View(adjVoucherDto);
        }

        //Author: Koh Meng Guan
        [Authorize(Roles = Role.Clerk)]
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


        //Author: May Zin Ko
        [Authorize(Roles = "manager,supervisor")]
        public ActionResult ViewPendingStockAdj()
        {
            var role = User.IsInRole(Role.Manager) ? Role.Manager : Role.Supervisor;
            ViewBag.Message = role;
            return View(_stockAdjustmentRepo.GetPendingAdjustmentByRole(role));
        }

        //Author: May Zin Ko
        [Authorize(Roles = "manager,supervisor")]
        [HttpGet]
        public ActionResult ApproveReject(string list, string status)
        {
            //  List<AdjVoucher> list = _stockAdjustmentRepo.GetAdjustmentById(List);
            ViewBag.checkList = list;
            ViewBag.status = status;
            return PartialView("ApproveReject");
        }

        //Author: May Zin Ko
        [Authorize(Roles = "manager,supervisor")]
        [HttpPost]
        public ActionResult ApproveReject(string checkList, string comment, string status)
        {
            var list = checkList.Split(',');
            var idList = new int[list.Length];
            for (int i = 0; i < idList.Length; i++)
            {
                idList[i] = int.Parse(list[i]);
            }
            HttpCookie cookie = HttpContext.Request.Cookies.Get("Employee");
            String empNum = cookie["EmpNum"];
            foreach (var id in idList)
            {
                
                var adjustment = _stockAdjustmentRepo.GetById(id);
                adjustment.Status = status;
                String item = adjustment.ItemNum;
                Stationery st = new Stationery();
                    st=_stationeryRepo.GetById(item);
              
                    st.AvailableQty = st.AvailableQty+adjustment.Quantity;
                    st.CurrentQty = st.CurrentQty+adjustment.Quantity;
               
                _stationeryRepo.Update(st);

                adjustment.Remark = comment;
                adjustment.ApprovalDate = DateTime.Today;
                adjustment.ApprovalEmpNum = Convert.ToInt32(empNum);
                _stockAdjustmentRepo.Update(adjustment);
            }

            return PartialView();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stockAdjustmentRepo.Dispose();
                _stationeryRepo.Dispose();
                _employeeRepo.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}