using LUSSIS.Models;
using LUSSIS.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using LUSSIS.Models.WebDTO;
using PagedList;
using LUSSIS.Emails;
using LUSSIS.CustomAuthority;
using System.Text;

namespace LUSSIS.Controllers
{
    //Authors: Cui Runze, Tang Xiaowen, Koh Meng Guan
    [Authorize(Roles = "head, staff, clerk, rep")]
    public class RequisitionsController : Controller
    {
        private readonly RequisitionRepository _requistionRepo = new RequisitionRepository();
        private readonly EmployeeRepository _employeeRepo = new EmployeeRepository();
        private readonly DisbursementRepository _disbursementRepo = new DisbursementRepository();
        private readonly StationeryRepository _stationeryRepo = new StationeryRepository();
        private readonly DepartmentRepository _departmentRepo = new DepartmentRepository();
        private readonly DelegateRepository _delegateRepo = new DelegateRepository();

        private bool HasDelegate
        {
            get
            {
                var deptCode = Request.Cookies["Employee"]?["DeptCode"];
                var current = _delegateRepo.FindCurrentByDeptCode(deptCode);
                return current != null;
            }
        }

        private bool IsDelegate
        {
            get
            {
                var empNum = Convert.ToInt32(Request.Cookies["Employee"]?["EmpNum"]);
                var isDelegate = _delegateRepo.FindCurrentByEmpNum(empNum);
                return isDelegate != null;
            }
        }

        // GET: Requisition
        //Authors: Koh Meng Guan
        [CustomAuthorize("head", "staff")]
        public ActionResult Pending()
        {
            var deptCode = Request.Cookies["Employee"]?["DeptCode"];
            var req = _requistionRepo.GetPendingListForHead(deptCode);

            //If user is head and there is delegate
            if (User.IsInRole("head") && HasDelegate)
            {
                ViewBag.HasDelegate = HasDelegate;
            }

            return View(req);
        }

        //Authors: Koh Meng Guan
        [CustomAuthorize("head", "staff")]
        [HttpGet]
        public ActionResult Details(int reqId)
        {
            //If user is head and there is delegate
            if (User.IsInRole("head") && HasDelegate)
            {
                ViewBag.HasDelegate = HasDelegate;
            }

            var req = _requistionRepo.GetById(reqId);
            if (req != null && req.Status == "pending")
            {
                ViewBag.Pending = "Pending";
                return View(req);
            }

            return new HttpNotFoundResult();
        }

        [CustomAuthorize("head", "staff")]
        //Authors: Koh Meng Guan
        public ActionResult All(string searchString, string currentFilter, int? page)
        {
            var deptCode = Request.Cookies["Employee"]?["DeptCode"];

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            var requistions = !string.IsNullOrEmpty(searchString)
                ? _requistionRepo.FindRequisitionsByDeptCodeAndText(searchString, deptCode)
                : _requistionRepo.GetAllByDeptCode(deptCode);

            var reqAll = requistions.ToPagedList(pageNumber: page ?? 1, pageSize: 15);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_All", reqAll);
            }

            return View(reqAll);
        }


        //Authors: Koh Meng Guan
        [CustomAuthorize("head", "staff")]
        [HttpPost]
        public async Task<ActionResult> Details(
            [Bind(Include =
                "RequisitionId,RequisitionEmpNum,RequisitionDate,RequestRemarks,ApprovalRemarks,Status,DeptCode")]
            Requisition requisition, string status)
        {
            if (requisition.Status == "pending")
            {
                //requisition must be pending for any approval and reject
                var deptCode = Request.Cookies["Employee"]?["DeptCode"];
                var empNum = Convert.ToInt32(Request.Cookies["Employee"]?["EmpNum"]);

                if (User.IsInRole("head") && !HasDelegate || IsDelegate)
                {
                    //if (user is head and there is no delegate) or (user is currently delegate)
                    if (deptCode != _departmentRepo.GetDepartmentByEmpNum(requisition.RequisitionEmpNum).DeptCode)
                    {
                        //if user is trying to approve for other department
                        return View("_unauthoriseAccess");
                    }

                    if (empNum == requisition.RequisitionEmpNum)
                    {
                        //if user is trying to self approve (delegate's old requistion)
                        return View("_unauthoriseAccess");
                    }

                    if (ModelState.IsValid)
                    {
                        requisition.ApprovalEmpNum = empNum;
                        requisition.ApprovalDate = DateTime.Today;
                        requisition.Status = status;
                        await _requistionRepo.UpdateAsync(requisition);
                        return RedirectToAction("Pending");
                    }

                    return View(requisition);
                }

                return View("_hasDelegate");
            }

            return new HttpUnauthorizedResult();
        }



        // GET: DeptEmpReqs
        [DelegateStaffCustomAuth("staff", "rep")]
        public ActionResult Index(string searchString, string currentFilter, int? page)
        {
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            var stationerys = !string.IsNullOrEmpty(searchString)
                ? _stationeryRepo.GetByDescription(searchString).ToList()
                : _stationeryRepo.GetAll().ToList();

            var stationeryList = stationerys.ToPagedList(pageNumber: page ?? 1, pageSize: 15);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_Index", stationeryList);
            }

            return View(stationeryList);
        }

        // /Requisitions/AddToCart
        [DelegateStaffCustomAuth("staff", "rep")]
        [HttpPost]
        public ActionResult AddToCart(string id, int qty)
        {
            var item = _stationeryRepo.GetById(id);
            var cart = new Cart(item, qty);
            var shoppingCart = Session["MyCart"] as ShoppingCart;
            shoppingCart?.addToCart(cart);
            return Json(shoppingCart?.GetCartItemCount());
        }
        
        [DelegateStaffCustomAuth("staff", "rep")]
        public ActionResult MyRequisitions(string currentFilter, int? page)
        {
            var empNum = Convert.ToInt32(Request.Cookies["Employee"]?["EmpNum"]);

            var reqlist = _requistionRepo.GetRequisitionByEmpNum(empNum)
                .OrderByDescending(s => s.RequisitionDate).ThenByDescending(s => s.RequisitionId).ToList();

            return View(reqlist.ToPagedList(pageNumber: page ?? 1, pageSize: 15));
        }

        // GET: Requisitions/EmpReqDetail/5
        [DelegateStaffCustomAuth("staff", "rep")]
        [HttpGet]
        public ActionResult MyRequisitionDetails(int id)
        {
            var requisitionDetail = _requistionRepo.GetRequisitionDetail(id).ToList();
            return View(requisitionDetail);
        }

        [DelegateStaffCustomAuth("staff", "rep")]
        [HttpPost]
        public ActionResult SubmitReq()
        {
            var itemNums = (List<string>) Session["itemNums"];
            var itemQty = (List<int>) Session["itemQty"];

            var deptCode = Request.Cookies["Employee"]?["DeptCode"];
            var empNum = Convert.ToInt32(Request.Cookies["Employee"]?["EmpNum"]);
            var fullName = Request.Cookies["Employee"]?["Name"];

            if (itemNums != null)
            {
                var requisition = new Requisition()
                {
                    RequestRemarks = Request["remarks"],
                    RequisitionDate = DateTime.Today,
                    RequisitionEmpNum = empNum,
                    Status = "pending",
                    DeptCode = deptCode
                };
                _requistionRepo.Add(requisition);

                var stationerys = new List<Stationery>();
                for (var i = 0; i < itemNums.Count; i++)
                {
                    var requisitionDetail = new RequisitionDetail()
                    {
                        RequisitionId = requisition.RequisitionId,
                        ItemNum = itemNums[i],
                        Quantity = itemQty[i]
                    };
                    _requistionRepo.AddRequisitionDetail(requisitionDetail);

                    stationerys.Add(_stationeryRepo.GetById(requisitionDetail.ItemNum));
                }

                Session["itemNums"] = null;
                Session["itemQty"] = null;
                Session["MyCart"] = new ShoppingCart();

                var headEmail = _employeeRepo.GetDepartmentHead(deptCode).EmailAddress;

                var email = new LUSSISEmail.Builder().From(User.Identity.Name)
                    .To(headEmail).ForNewRequistion(fullName, requisition, stationerys).Build();

                EmailHelper.SendEmail(email);

                return RedirectToAction("MyRequisitions");
            }

            return RedirectToAction("MyCart");
        }

        [DelegateStaffCustomAuth("staff", "rep")]
        public ActionResult MyCart()
        {
            var mycart = (ShoppingCart) Session["MyCart"];
            return View(mycart.GetAllCartItem());
        }

        [DelegateStaffCustomAuth("staff", "rep")]
        [HttpPost]
        public ActionResult DeleteCartItem(string id, int qty)
        {
            var myCart = Session["MyCart"] as ShoppingCart;
            myCart?.deleteCart(id);

            return Json(id);
        }

        [DelegateStaffCustomAuth("staff", "rep")]
        [HttpPost]
        public ActionResult UpdateCartItem(string id, int qty)
        {
            var mycart = Session["MyCart"] as ShoppingCart;
            var c = new Cart();
            foreach (var cart in mycart.shoppingCart)
            {
                if (cart.stationery.ItemNum == id)
                {
                    c = cart;
                    cart.quantity = qty;
                    break;
                }
            }

            if (c.quantity <= 0)
            {
                mycart.shoppingCart.Remove(c);
            }

            return RedirectToAction("MyCart");
        }

        [Authorize(Roles = "clerk")]
        public ActionResult Consolidated(int? page)
        {
            int pageSize = 15;
            int pageNumber = (page ?? 1);

            var itemsList = _requistionRepo.GetConsolidatedRequisition().ToList().ToPagedList(pageNumber, pageSize);


            return View(new RetrievalItemsWithDateDTO
            {
                retrievalItems = itemsList,
                collectionDate = DateTime.Today.ToString("dd/MM/yyyy"),
                hasInprocessDisbursement = _disbursementRepo.hasInprocessDisbursements()
            });
        }

        [HttpPost]
        [Authorize(Roles = "clerk")]
        [ValidateAntiForgeryToken]
        public ActionResult Retrieve([Bind(Include = "collectionDate")] RetrievalItemsWithDateDTO listWithDate)
        {
            if (ModelState.IsValid)
            {
                DateTime selectedDate = DateTime.ParseExact(listWithDate.collectionDate, "dd/MM/yyyy",
                    CultureInfo.InvariantCulture);
                _requistionRepo.ArrangeRetrievalAndDisbursement(selectedDate);
                return RedirectToAction("RetrievalInProcess");
            }

            
            return View("Consolidated", new RetrievalItemsWithDateDTO
            {
                retrievalItems = _requistionRepo.GetConsolidatedRequisition().ToList().ToPagedList(1, 15),
                collectionDate = DateTime.Today.ToString("dd/MM/yyyy"),
                hasInprocessDisbursement = _disbursementRepo.hasInprocessDisbursements()
            });
        }

        
        [Authorize(Roles = "clerk")]
        public ActionResult RetrievalInProcess()
        {
            return View(_requistionRepo.GetRetrievalInPorcess());
        }

        //Authors: Koh Meng Guan
        [CustomAuthorize("head", "staff")]
        [HttpGet]
        public PartialViewResult _ApproveReq(int Id, String Status)
        {
            var reqDto = new ReqApproveRejectDTO
            {
                RequisitionId = Id,
                Status = Status
            };
            
            if (User.IsInRole("head") && !HasDelegate || IsDelegate)
            {
                return PartialView("_ApproveReq", reqDto);
            }

            return PartialView("_hasDelegate");
        }


        //Authors: Koh Meng Guan
        [CustomAuthorize("head", "staff")]
        [HttpPost]
        public PartialViewResult _ApproveReq([Bind(Include = "RequisitionId,ApprovalRemarks,Status")]
            ReqApproveRejectDTO RADTO)
        {
            var req = _requistionRepo.GetById(RADTO.RequisitionId);
            if (req == null || req.Status != "pending") return PartialView("_unauthoriseAccess");

            var deptCode = Request.Cookies["Employee"]?["DeptCode"];
            var empNum = Convert.ToInt32(Request.Cookies["Employee"]?["EmpNum"]);

            //must be pending for approval and reject
            if (User.IsInRole("head") && !HasDelegate || IsDelegate)
            {
                //if (user is head and there is no delegate) or (user is currently delegate)
                if (deptCode != _departmentRepo.GetDepartmentByEmpNum(req.RequisitionEmpNum).DeptCode)
                {
                    //if user is trying to approve for other department
                    return PartialView("_unauthoriseAccess");
                }

                if (empNum == req.RequisitionEmpNum)
                {
                    //if user is trying to self approve 
                    return PartialView("_unauthoriseAccess");
                }

                if (ModelState.IsValid)
                {
                    if (req.ApprovalEmpNum == empNum)
                    {
                        return PartialView("_unauthoriseAccess");
                    }

                    req.Status = RADTO.Status;
                    req.ApprovalRemarks = RADTO.ApprovalRemarks;
                    req.ApprovalEmpNum = empNum;
                    req.ApprovalDate = DateTime.Today;

                    _requistionRepo.Update(req);

                    var toEmail = req.RequisitionEmployee.EmailAddress;

                    var email = new LUSSISEmail.Builder().From(User.Identity.Name)
                        .To(toEmail).ForRequisitionApproval(req).Build();
                            
                    EmailHelper.SendEmail(email);

                    return PartialView();
                }

                return PartialView(RADTO);
            }

            return PartialView("_hasDelegate");

        }
    }
}