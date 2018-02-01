using LUSSIS.Models;
using LUSSIS.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using LUSSIS.Constants;
using LUSSIS.Models.WebDTO;
using PagedList;
using LUSSIS.Emails;
using LUSSIS.CustomAuthority;
using static LUSSIS.Constants.RequisitionStatus;
using static LUSSIS.Constants.DisbursementStatus;

namespace LUSSIS.Controllers
{
    //Authors: Cui Runze, Tang Xiaowen, Koh Meng Guan
    [Authorize(Roles = "head, staff, clerk, rep")]
    public class RequisitionsController : Controller
    {
        private readonly RequisitionRepository _requisitionRepo = new RequisitionRepository();
        private readonly EmployeeRepository _employeeRepo = new EmployeeRepository();
        private readonly DisbursementRepository _disbursementRepo = new DisbursementRepository();
        private readonly StationeryRepository _stationeryRepo = new StationeryRepository();
        private readonly DepartmentRepository _departmentRepo = new DepartmentRepository();
        private readonly DelegateRepository _delegateRepo = new DelegateRepository();
        private readonly CollectionRepository _collectionRepo = new CollectionRepository();


        /// <summary>
        /// Return true if a department has available delegate, including in the future.
        /// </summary>
        private bool HasDelegate
        {
            get
            {
                var deptCode = Request.Cookies["Employee"]?["DeptCode"];
                var current = _delegateRepo.FindCurrentByDeptCode(deptCode);
                return current != null;
            }
        }

        /// <summary>
        /// Return true if the employee is delegated, including in the future.
        /// </summary>
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
        [CustomAuthorize(Role.DepartmentHead, Role.Staff)]
        public ActionResult Pending()
        {
            var deptCode = Request.Cookies["Employee"]?["DeptCode"];
            var req = _requisitionRepo.GetPendingListForHead(deptCode);

            //If user is head and there is delegate
            if (User.IsInRole(Role.DepartmentHead) && HasDelegate)
            {
                ViewBag.HasDelegate = HasDelegate;
            }

            return View(req);
        }

        //Authors: Koh Meng Guan
        [CustomAuthorize(Role.DepartmentHead, Role.Staff)]
        [HttpGet]
        public ActionResult Details(int reqId)
        {
            //If user is head and there is delegate
            if (User.IsInRole(Role.DepartmentHead) && HasDelegate)
            {
                ViewBag.HasDelegate = HasDelegate;
            }

            var req = _requisitionRepo.GetById(reqId);
            if (req == null) return new HttpNotFoundResult();

            ViewBag.Pending = req.Status == RequisitionStatus.Pending ? "Pending" : null;
            return View(req);
        }

        [CustomAuthorize(Role.DepartmentHead, Role.Staff)]
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

            ViewBag.CurrentFilter = searchString;

            var requistions = !string.IsNullOrEmpty(searchString)
                ? _requisitionRepo.FindRequisitionsByDeptCodeAndText(searchString, deptCode)
                : _requisitionRepo.GetAllByDeptCode(deptCode);

            var reqAll = requistions.ToPagedList(pageNumber: page ?? 1, pageSize: 15);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_All", reqAll);
            }

            return View(reqAll);
        }


        //Authors: Koh Meng Guan
        [CustomAuthorize(Role.DepartmentHead, Role.Staff)]
        [HttpPost]
        public ActionResult Details(
            [Bind(Include =
                "RequisitionId,RequisitionEmpNum,RequisitionDate,RequestRemarks,ApprovalRemarks,Status,DeptCode")]
            Requisition requisition, string statuses)
        {
            if (requisition.Status == RequisitionStatus.Pending)
            {
                //requisition must be pending for any approval and reject
                var deptCode = Request.Cookies["Employee"]?["DeptCode"];
                var empNum = Convert.ToInt32(Request.Cookies["Employee"]?["EmpNum"]);

                if (User.IsInRole(Role.DepartmentHead) && !HasDelegate || IsDelegate)
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
                        requisition.Status = statuses;
                        _requisitionRepo.Update(requisition);
                        if (requisition.Status == "approved")
                        {
                            Requisition req = _requisitionRepo.GetById(requisition.RequisitionId);
                            foreach (var requisitionDetail in req.RequisitionDetails)
                            {
                                var stationery = _stationeryRepo.GetById(requisitionDetail.ItemNum);
                                stationery.AvailableQty = stationery.AvailableQty - requisitionDetail.Quantity;
                                _stationeryRepo.Update(stationery);
                            }
                        }

                        return RedirectToAction("Pending");
                    }

                    return View(requisition);
                }

                return View("_hasDelegate");
            }

            return new HttpUnauthorizedResult();
        }


        // GET: DeptEmpReqs
        [DelegateStaffCustomAuth(Role.Staff, Role.Representative)]
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

            ViewBag.CurrentFilter = searchString;

            var stationerys = string.IsNullOrEmpty(searchString)
                ? _stationeryRepo.GetAll().ToList() : _stationeryRepo.GetByDescription(searchString).ToList();

            var stationeryList = stationerys.ToPagedList(pageNumber: page ?? 1, pageSize: 15);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_Index", stationeryList);
            }

            return View(stationeryList);
        }

        // POST: /Requisitions/AddToCart
        [DelegateStaffCustomAuth(Role.Staff, Role.Representative)]
        [HttpPost]
        public ActionResult AddToCart(string id, int qty)
        {
            var item = _stationeryRepo.GetById(id);
            var cart = new CartDTO(item, qty);
            var shoppingCart = Session["MyCart"] as ShoppingCartDTO;
            shoppingCart?.AddToCart(cart);
            return Json(shoppingCart?.GetCartItemCount());
        }

        // GET: /Requisitions/MyRequisitions
        [DelegateStaffCustomAuth(Role.Staff, Role.Representative)]
        public ActionResult MyRequisitions(string currentFilter, int? page)
        {
            var empNum = Convert.ToInt32(Request.Cookies["Employee"]?["EmpNum"]);

            var reqlist = _requisitionRepo.GetRequisitionsByEmpNum(empNum)
                .OrderByDescending(s => s.RequisitionDate).ThenByDescending(s => s.RequisitionId).ToList();

            return View(reqlist.ToPagedList(pageNumber: page ?? 1, pageSize: 15));
        }

        // GET: Requisitions/EmpReqDetail/5
        [DelegateStaffCustomAuth(Role.Staff, Role.Representative)]
        [HttpGet]
        public ActionResult MyRequisitionDetails(int id)
        {
            var requisitionDetail = _requisitionRepo.GetRequisitionDetailsById(id).ToList();
            return View(requisitionDetail);
        }

        // POST: Requisitions/SubmitReq
        [DelegateStaffCustomAuth(Role.Staff, Role.Representative)]
        [HttpPost]
        public ActionResult SubmitReq()
        {
            var itemNums = (List<string>)Session["itemNums"];
            var itemQty = (List<int>)Session["itemQty"];

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
                    Status = RequisitionStatus.Pending,
                    DeptCode = deptCode
                };
                _requisitionRepo.Add(requisition);

                for (var i = 0; i < itemNums.Count; i++)
                {
                    var requisitionDetail = new RequisitionDetail()
                    {
                        RequisitionId = requisition.RequisitionId,
                        ItemNum = itemNums[i],
                        Quantity = itemQty[i]
                    };
                    requisitionDetail.Stationery = _requisitionRepo.AddRequisitionDetail(requisitionDetail);
                }

                Session["itemNums"] = null;
                Session["itemQty"] = null;
                Session["MyCart"] = new ShoppingCartDTO();

                //Send email
                var headEmail = _employeeRepo.GetDepartmentHead(deptCode).EmailAddress;
                var email = new LUSSISEmail.Builder().From(User.Identity.Name)
                    .To(headEmail).ForNewRequistion(fullName, requisition).Build();
                new Thread(delegate() { EmailHelper.SendEmail(email); }).Start();

                return RedirectToAction("MyRequisitions");
            }

            return RedirectToAction("MyCart");
        }

        // GET: Requisitions/MyCart
        [DelegateStaffCustomAuth(Role.Staff, Role.Representative)]
        public ActionResult MyCart()
        {
            var mycart = (ShoppingCartDTO)Session["MyCart"];
            return View(mycart.GetAllCartItem());
        }

        // POST: Requisitions/DeleteCartItem
        [DelegateStaffCustomAuth(Role.Staff, Role.Representative)]
        [HttpPost]
        public ActionResult DeleteCartItem(string id)
        {
            var myCart = Session["MyCart"] as ShoppingCartDTO;
            myCart?.DeleteCart(id);

            var count = (Session["MyCart"] as ShoppingCartDTO).GetCartItemCount();

            return Json(count);
        }

        // POST: Requisitions/UpdateCartItem
        [DelegateStaffCustomAuth(Role.Staff, Role.Representative)]
        [HttpPost]
        public ActionResult UpdateCartItem(string id, int qty)
        {
            var mycart = Session["MyCart"] as ShoppingCartDTO;
           
            foreach (var cart in mycart.shoppingCart)
            {
                if (cart.stationery.ItemNum != id) continue;
               
                cart.quantity = qty;
                break;
            }

            return Json((Session["MyCart"] as ShoppingCart).GetCartItemCount());
        }

        [Authorize(Roles = Role.Clerk)]
        public ActionResult Consolidated(int? page)
        {
            int pageSize = 15;
            int pageNumber = (page ?? 1);

            return View(new RetrievalItemsWithDateDTO
            {
                RetrievalItems = CreateRetrievalList().List.ToPagedList(pageNumber, pageSize),
                CollectionDate = DateTime.Today.ToString("dd/MM/yyyy"),
                //enables arranging for disbursement in UI once there is no inprocess disbursement
                HasInprocessDisbursement = _disbursementRepo.HasInprocessDisbursements()
            });
        }

        [HttpPost]
        [Authorize(Roles = Role.Clerk)]
        [ValidateAntiForgeryToken]
        public ActionResult Retrieve([Bind(Include = "CollectionDate")] RetrievalItemsWithDateDTO listWithDate)
        {
            if (ModelState.IsValid)
            {
                var selectedDate = DateTime.ParseExact(listWithDate.CollectionDate, "dd/MM/yyyy",
                    CultureInfo.InvariantCulture);

                var disbursements = CreateDisbursement(selectedDate);

                foreach (var disbursement in disbursements)
                {
                    var repEmail = _employeeRepo.GetRepByDeptCode(disbursement.DeptCode);
                    var collectionPoint = _collectionRepo.GetById((int)disbursement.CollectionPointId);
                    var email = new LUSSISEmail.Builder().From(User.Identity.Name).To(repEmail)
                        .ForNewDisbursement(disbursement, collectionPoint).Build();
                    new Thread(delegate () { EmailHelper.SendEmail(email); }).Start();
                }

                return RedirectToAction("RetrievalInProcess");
            }


            return View("Consolidated", new RetrievalItemsWithDateDTO
            {
                RetrievalItems = CreateRetrievalList().List.ToPagedList(1, 15),
                CollectionDate = DateTime.Today.ToString("dd/MM/yyyy"),
                HasInprocessDisbursement = _disbursementRepo.HasInprocessDisbursements()
            });
        }

        [Authorize(Roles = Role.Clerk)]
        public ActionResult RetrievalInProcess()
        {
            return View(_disbursementRepo.GetRetrievalInProcess());
        }

        //Authors: Koh Meng Guan
        [CustomAuthorize(Role.DepartmentHead, Role.Staff)]
        [HttpGet]
        public PartialViewResult ApproveReq(int id, string status)
        {
            var reqDto = new ReqApproveRejectDTO
            {
                RequisitionId = id,
                Status = status
            };

            if (User.IsInRole(Role.DepartmentHead) && !HasDelegate || IsDelegate)
            {
                return PartialView("_ApproveReq", reqDto);
            }

            return PartialView("_hasDelegate");
        }


        //Authors: Koh Meng Guan
        [CustomAuthorize(Role.DepartmentHead, Role.Staff)]
        [HttpPost]
        public ActionResult ApproveReq(ReqApproveRejectDTO reqApprovalDto)
        {
            var req = _requisitionRepo.GetById(reqApprovalDto.RequisitionId);
            if (req == null || req.Status != RequisitionStatus.Pending) return PartialView("_unauthoriseAccess");

            var deptCode = Request.Cookies["Employee"]?["DeptCode"];
            var empNum = Convert.ToInt32(Request.Cookies["Employee"]?["EmpNum"]);

            //must be pending for approval and reject
            if (User.IsInRole(Role.DepartmentHead) && !HasDelegate || IsDelegate)
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
                    req.Status = reqApprovalDto.Status;
                    req.ApprovalRemarks = reqApprovalDto.ApprovalRemarks;
                    req.ApprovalEmpNum = empNum;
                    req.ApprovalDate = DateTime.Today;


                    if (reqApprovalDto.Status == Approved)
                    {
                        foreach (var requisitionDetail in req.RequisitionDetails)
                        {
                            var stationery = _stationeryRepo.GetById(requisitionDetail.ItemNum);
                            stationery.AvailableQty = stationery.AvailableQty - requisitionDetail.Quantity;
                            _stationeryRepo.Update(stationery);
                        }
                    }

                    _requisitionRepo.Update(req);

                    //Send email
                    var toEmail = req.RequisitionEmployee.EmailAddress;
                    var email = new LUSSISEmail.Builder().From(User.Identity.Name)
                        .To(toEmail).ForRequisitionApproval(req).Build();
                    new Thread(delegate () { EmailHelper.SendEmail(email); }).Start();

                    return RedirectToAction("Pending");
                }

                return PartialView("_ApproveReq",reqApprovalDto);
            }

            return PartialView("_hasDelegate");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _requisitionRepo.Dispose();
                _disbursementRepo.Dispose();
                _employeeRepo.Dispose();
                _stationeryRepo.Dispose();
                _departmentRepo.Dispose();
                _collectionRepo.Dispose();
                _delegateRepo.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Create Retrieval List / Consolidated Requisition List logic

        private RetrievalListDTO CreateRetrievalList()
        {
            var itemsToRetrieve = new RetrievalListDTO();

            //adding remaining qty from unfulfilled disbursement
            var unfulfilledDisbursementDetails = _disbursementRepo.GetUnfulfilledDisbursementDetails().ToList();
            var consolidatedUnfulfilledDisbursements = ConsolidateUnfulfilledDisbursements(unfulfilledDisbursementDetails);
            itemsToRetrieve.AddRange(consolidatedUnfulfilledDisbursements);

            //adding requested qty from newly approved requisitions
            var approvedRequisitionDetails = _requisitionRepo.GetApprovedRequisitionDetails().ToList();
            var consolidatedNewRequisitions = ConsolidateNewRequisitions(approvedRequisitionDetails);
            itemsToRetrieve.AddRange(consolidatedNewRequisitions);

            return itemsToRetrieve;
        }

        /*
         * helper method to consolidate each [unfullfilled Disbursements for one item] add to / into [one RetrievalItemDTO]
        */
        private static RetrievalListDTO ConsolidateUnfulfilledDisbursements(
            List<DisbursementDetail> unfullfilledDisDetailList)
        {
            var itemsToRetrieve = new RetrievalListDTO();

            //group DisbursementDetail list by item: e.g.: List<DisDetail>-for-pen, List<DisDetail>-for-Paper, and store these lists in List:
            List<List<DisbursementDetail>> groupedDisListByItem =
                unfullfilledDisDetailList.GroupBy(rd => rd.ItemNum).Select(grp => grp.ToList()).ToList();

            //each list merge into ONE RetrievalItemDTO. e.g.: List<DisDetail>-for-pen to be converted into ONE RetrievalItemDTO. 
            foreach (List<DisbursementDetail> disListForOneItem in groupedDisListByItem)
            {
                var retrievalItem = new RetrievalItemDTO(disListForOneItem);

                itemsToRetrieve.Add(retrievalItem);
            }

            return itemsToRetrieve;
        }
        
        /*
         * helper method to consolidate each [approved requisitions for one item] into [one RetrievalItemDTO]
        */
        private static RetrievalListDTO ConsolidateNewRequisitions(List<RequisitionDetail> requisitionDetailList)
        {
            var itemsToRetrieve = new RetrievalListDTO();

            //group RequisitionDetail list by item: e.g.: List<ReqDetail>-for-pen, List<ReqDetail>-for-Paper, and store these lists in List:
            List<List<RequisitionDetail>> groupedReqListByItem = requisitionDetailList
                .GroupBy(rd => rd.ItemNum).Select(grp => grp.ToList()).ToList();

            //each list merge into ONE RetrievalItemDTO. e.g.: List<ReqDetail>-for-pen to be converted into ONE RetrievalItemDTO. 
            foreach (List<RequisitionDetail> reqListForOneItem in groupedReqListByItem)
            {
                var retrievalItem = new RetrievalItemDTO(reqListForOneItem);

                itemsToRetrieve.Add(retrievalItem);
            }

            return itemsToRetrieve;
        }

        #endregion

        #region Create Disburesement logic
        
        public List<Disbursement> CreateDisbursement(DateTime collectionDate)
        {
            var disbursements = new List<Disbursement>();
            
            GenerateDisbursementsWithApprovedRequisitions(disbursements, collectionDate);

            GenerateDisbursementsWithUnfullfilled(disbursements, collectionDate);

            //persist to database
            foreach (var d in disbursements)
            {
                _disbursementRepo.Add(d);
            }

            return disbursements;
        }

        /*
          * helper method step 1
          */
        private List<Disbursement> GenerateDisbursementsWithApprovedRequisitions(List<Disbursement> targetDisbursementsList, DateTime collectionDate)
        {
            List<List<Requisition>> reqGroupByDept = _disbursementRepo.GetApprovedRequisitions()
                .GroupBy(r => r.RequisitionEmployee.DeptCode)
                .Select(grp => grp.ToList()).ToList();

            // and create disbursement list based on it
            foreach (var requisitionsForOneDept in reqGroupByDept)
            {
                //Get all approved requisition details in one department
                var deptCode = requisitionsForOneDept.First().DeptCode;
                var requisitionDetailsForOneDept = _disbursementRepo.GetApprovedRequisitionDetailsByDeptCode(deptCode);

                //construct disbursment based on the requisition list
                var disbursement = new Disbursement(requisitionDetailsForOneDept, collectionDate);

                targetDisbursementsList.Add(disbursement);

                //as respective disbursement is created, update the requisition status
                foreach (var requisition in requisitionsForOneDept)
                {
                    requisition.Status = RequisitionStatus.Processed;
                    _disbursementRepo.UpdateRequisition(requisition);
                }
            }

            return targetDisbursementsList;
        }

        /*
        * helper method step 2
        */
        private List<Disbursement> GenerateDisbursementsWithUnfullfilled(List<Disbursement> targetDisbursementsList, DateTime collectionDate)
        {
            var unfufilledDisbursementDetails = _disbursementRepo.GetUnfulfilledDisbursementDetails().ToList();
            foreach (var ufdDetail in unfufilledDisbursementDetails)
            {
                //refactor the remaining qty from unfulfilled details into the new disbursement details
                var unfulfilledDetail = new DisbursementDetail(ufdDetail);
                var isDisbursementForSameDept = false;
                foreach (var d in targetDisbursementsList)
                {
                    if (d.DeptCode == ufdDetail.Disbursement.DeptCode)
                    {
                        unfulfilledDetail.Stationery = ufdDetail.Stationery;
                        //depends on itemNum, add qty or add as new detail
                        d.Add(unfulfilledDetail);
                        isDisbursementForSameDept = true;
                        break;
                    }
                }

                if (isDisbursementForSameDept) continue;

                //if not for same dept, construct a new disbursement
                var disbursement = new Disbursement(ufdDetail.Disbursement, collectionDate);
                disbursement.Add(unfulfilledDetail);
                targetDisbursementsList.Add(disbursement);
            }

            //since the remaining qty (if any) has been factored into the new disbursement details' requested qty 
            //mark processed unfulfilled disbursements fulfilled, and their detail lists' requested qty to be updated to tally status

            foreach (var detail in unfufilledDisbursementDetails)
            {
                detail.RequestedQty = detail.ActualQty;
                _disbursementRepo.UpdateDisbursementDetail(detail);
            }
            
            var unfulfilledDisList = _disbursementRepo.GetUnfulfilledDisbursements().ToList();
            foreach (var unfd in unfulfilledDisList)
            {
                unfd.Status = DisbursementStatus.Fulfilled;
                _disbursementRepo.Update(unfd);
            }
            
            return targetDisbursementsList;
        }


        #endregion
    }
}