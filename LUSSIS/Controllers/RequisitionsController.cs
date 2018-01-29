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

namespace LUSSIS.Controllers
{
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

        //TODO: Add authroization - DepartmentHead or Delegate only
        // GET: Requisition
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

        //TODO: Add authroization - DepartmentHead or Delegate only
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


        //TODO: Add authorization - DepartmentHead or Delegate only
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


        //TODO: return create page, only showing necessary fields
        // GET: Requisition/Create
        //???
        [DelegateStaffCustomAuth("staff")]
        public ActionResult Create()
        {
            return View();
        }

        // TODO: 1. create new requisition, 2. its status set to pending, 3. send notification to departmenthead
        // [employee page] POST: Requisition/Create
        [DelegateStaffCustomAuth("staff", "rep")]
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // TODO: only implement once main project is done. Enable editing if status is pending
        // [employee page]  GET: Requisition/Edit/5
        [DelegateStaffCustomAuth("staff", "rep")]
        public ActionResult Edit(int id)
        {
            return View();
        }

        // TODO: only enable editing if status is pending
        // [employee page]  POST: Requisition/Edit/5
        [DelegateStaffCustomAuth("staff", "rep")]
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // POST: Requisition/Delete/5
        [HttpPost]
        [Authorize(Roles = "clerk")]
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

        // POST: /Requisitions/AddToCart
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

        // GET: /Requisitions/MyRequisitions
        [DelegateStaffCustomAuth("staff", "rep")]
        public ActionResult MyRequisitions(string currentFilter, int? page)
        {
            var empNum = Convert.ToInt32(Request.Cookies["Employee"]?["EmpNum"]);

            var reqlist = _requistionRepo.GetRequisitionsByEmpNum(empNum)
                .OrderByDescending(s => s.RequisitionDate).ThenByDescending(s => s.RequisitionId).ToList();

            return View(reqlist.ToPagedList(pageNumber: page ?? 1, pageSize: 15));
        }

        // GET: Requisitions/EmpReqDetail/5
        [DelegateStaffCustomAuth("staff", "rep")]
        [HttpGet]
        public ActionResult MyRequisitionDetails(int id)
        {
            var requisitionDetail = _requistionRepo.GetRequisitionDetailsById(id).ToList();
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

        //Store Clerk's page
        [Authorize(Roles = "clerk")]
        public ActionResult Consolidated(int? page)
        {
            int pageSize = 15;
            int pageNumber = (page ?? 1);

            var itemsList = CreateRetrievalList().List.ToPagedList(pageNumber, pageSize);


            return View(new RetrievalItemsWithDateDTO
            {
                retrievalItems = itemsList,
                collectionDate = DateTime.Today.ToString("dd/MM/yyyy"),
                hasInprocessDisbursement = _disbursementRepo.hasInprocessDisbursements()
            });
        }

        //TODO: Add authorization - Stock Clerk only 
        //click on generate button - post with date selected
        [HttpPost]
        [Authorize(Roles = "clerk")]
        [ValidateAntiForgeryToken]
        public ActionResult Retrieve([Bind(Include = "collectionDate")] RetrievalItemsWithDateDTO listWithDate)
        {
            if (ModelState.IsValid)
            {
                var selectedDate = DateTime.ParseExact(listWithDate.collectionDate, "dd/MM/yyyy",
                    CultureInfo.InvariantCulture);

                var disbursements = CreateDisbursement(selectedDate);

                foreach (var disbursement in disbursements)
                {
                    var repEmail = _employeeRepo.GetRepByDeptCode(disbursement.DeptCode);
                    var email = new LUSSISEmail.Builder().From(User.Identity.Name).To(repEmail)
                        .ForNewDisbursement(disbursement).Build();
                    EmailHelper.SendEmail(email);
                }

                return RedirectToAction("RetrievalInProcess");
            }

            
            return View("Consolidated", new RetrievalItemsWithDateDTO
            {
                retrievalItems = CreateRetrievalList().List.ToPagedList(1, 15),
                collectionDate = DateTime.Today.ToString("dd/MM/yyyy"),
                hasInprocessDisbursement = _disbursementRepo.hasInprocessDisbursements()
            });
        }

        private RetrievalListDTO CreateRetrievalList()
        {
            var itemsToRetrieve = new RetrievalListDTO();

            var approvedRequisitionDetails = _requistionRepo.GetRequisitionDetailsByStatus("approved");
            itemsToRetrieve.AddRange(ConsolidateNewRequisitions(approvedRequisitionDetails));

            var unfulfilledDisbursementDetails = _disbursementRepo.GetUnfulfilledDisbursementDetailList();
            itemsToRetrieve.AddRange(ConsolidateUnfulfilledDisbursements(unfulfilledDisbursementDetails));

            return itemsToRetrieve;
        }

        public List<Disbursement> CreateDisbursement(DateTime collectionDate)
        {
            var disbursements = new List<Disbursement>();

            //group requisition requests by dept and create disbursement list based on it
            var approvedRequisitions = _disbursementRepo.GetApprovedRequisitions().ToList();

            List<List<Requisition>> reqGroupByDept = approvedRequisitions.GroupBy(r => r.RequisitionEmployee.DeptCode)
                .Select(grp => grp.ToList()).ToList();

            foreach (var requisitions in reqGroupByDept)
            {
                //Get all approved requisition details in one department
                var deptCode = requisitions.First().DeptCode;
                var requisitionDetails = _disbursementRepo.GetApprovedRequisitionDetailsByDeptCode(deptCode);

                //convert requisitions to disbursment
                var disbursement = new Disbursement(requisitionDetails, collectionDate);

                disbursements.Add(disbursement);

                //when disbursement is created, update the requisition status
                foreach (var requisition in requisitions)
                {
                    requisition.Status = "processed";
                    _requistionRepo.Update(requisition);
                }

            }

            //unfulfilled disburstment from last time will be added to this time's disbursment
            var unfufilledDisbursementDetails = _disbursementRepo.GetUnfulfilledDisbursementDetailList();

            foreach (var detail in unfufilledDisbursementDetails)
            {
                //convert the unfulfilled to a new disbursement detail
                var unfulfilledDetail = new DisbursementDetail(detail);
                var isDisbursementExisted = false;
                foreach (var d in disbursements)
                {
                    if (d.DeptCode == detail.Disbursement.DeptCode)
                    {
                        d.Add(unfulfilledDetail);
                        isDisbursementExisted = true;
                        break;
                    }
                }

                if (isDisbursementExisted) continue;

                var disbursement = new Disbursement(detail.Disbursement, collectionDate);
                disbursement.Add(unfulfilledDetail);
                disbursements.Add(disbursement);
            }

            foreach (var detail in unfufilledDisbursementDetails)
            {
                //the unfulfill detail now need to update its requested quantity to match its actual quantity
                //the requested qty will tally with the newly made disbursement detail
                detail.RequestedQty = detail.ActualQty;
                _disbursementRepo.UpdateDisbursementDetail(detail);
            }

            //persist to database
            foreach (var d in disbursements)
            {
                _disbursementRepo.Add(d);
            }

            var unfulfilledDisList = _disbursementRepo.GetDisbursementByStatus("unfulfilled").ToList();
            foreach (var unfd in unfulfilledDisList)
            {
                unfd.Status = "fulfilled";
                _disbursementRepo.Update(unfd);
            }

            return disbursements;
        }

        /*
         * helper method to consolidate each [approved requisitions for one item] into [one RetrievalItemDTO]
        */
        private static RetrievalListDTO ConsolidateNewRequisitions(IEnumerable<RequisitionDetail> requisitionDetailList)
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

        /*
         * helper method to consolidate each [unfullfilled Disbursements for one item] add to / into [one RetrievalItemDTO]
        */
        private static RetrievalListDTO ConsolidateUnfulfilledDisbursements(IEnumerable<DisbursementDetail> unfullfilledDisDetailList)
        {
            var itemsToRetrieve = new RetrievalListDTO();

            //group DisbursementDetail list by item: e.g.: List<DisDetail>-for-pen, List<DisDetail>-for-Paper, and store these lists in List:
            List<List<DisbursementDetail>> groupedDisListByItem = unfullfilledDisDetailList.GroupBy(rd => rd.ItemNum).Select(grp => grp.ToList()).ToList();

            //each list merge into ONE RetrievalItemDTO. e.g.: List<DisDetail>-for-pen to be converted into ONE RetrievalItemDTO. 
            foreach (List<DisbursementDetail> disListForOneItem in groupedDisListByItem)
            {
                var retrievalItem = new RetrievalItemDTO(disListForOneItem);

                itemsToRetrieve.Add(retrievalItem);
            }

            return itemsToRetrieve;
        }


        //TODO: A method to display in process Retrieval
        [Authorize(Roles = "clerk")]
        public ActionResult RetrievalInProcess()
        {
            return View(_requistionRepo.GetRetrievalInProcess());
        }

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