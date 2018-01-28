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
using LUSSIS.Exceptions;
using LUSSIS.Models.WebDTO;
using PagedList;
using LUSSIS.Emails;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using LUSSIS.CustomAuthority;
using System.Text;
using System.Data;
using System.Data.SqlClient;

using System.IO;

namespace LUSSIS.Controllers
{
    [Authorize(Roles = "head, staff, clerk, rep")]
    public class RequisitionsController : Controller
    {

        private RequisitionRepository _requistionRepo = new RequisitionRepository();
        private EmployeeRepository _employeeRepo = new EmployeeRepository();
        private DisbursementRepository _disbursementRepo = new DisbursementRepository();
        private StationeryRepository _stationeryRepo = new StationeryRepository();
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
            if (req != null)
            {
                if (req.Status == "pending")
                {
                    ViewBag.Pending = "Pending";
                }
                return View(req);
            }
            else
            {
                return new HttpNotFoundResult();
            }

        }

        [CustomAuthorize("head", "staff")]
        public ActionResult All(string searchString, string currentFilter, int? page)
        {
            List<Requisition> requistions = new List<Requisition>();
            Employee self = _employeeRepo.GetCurrentUser();
            if (searchString != null)
            { page = 1; }
            else
            {
                searchString = currentFilter;
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                requistions = _requistionRepo.GetAllRequisitionsSearch(searchString, self);
            }
            else
            {
                requistions = _requistionRepo.GetAllRequisitionsForCurrentUser(self);
            }
            int pageSize = 15;
            int pageNumber = (page ?? 1);

            var reqAll = requistions.ToPagedList(pageNumber, pageSize);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_All", reqAll);
            }

            return View(reqAll);
        }


        //TODO: Add authorization - DepartmentHead or Delegate only
        [CustomAuthorize("head", "staff")]
        [HttpPost]
        public async Task<ActionResult> Details([Bind(Include = "RequisitionId,RequisitionEmpNum,RequisitionDate,RequestRemarks,ApprovalRemarks,Status,DeptCode")] Requisition requisition, string SubmitButton)
        {
            if (requisition.Status == "pending")
            {//requisition must be pending for any approval and reject
                Employee self = _employeeRepo.GetCurrentUser();

                if ((self.JobTitle == "head" && !HasDelegate) || HasDelegate)
                {//if (user is head and there is no delegate) or (user is currently delegate)
                    if(self.DeptCode != _employeeRepo.GetDepartmentByEmpNum(requisition.RequisitionEmpNum).DeptCode)
                    {//if user is trying to approve for other department
                        return View("_unauthoriseAccess");
                    }
                    if ((self.EmpNum == requisition.RequisitionEmpNum))
                    {//if user is trying to self approve (delegate's old requistion)
                        return View("_unauthoriseAccess");
                    }
                    else
                    {
                        if (ModelState.IsValid)
                        {
                            requisition.ApprovalEmpNum = _employeeRepo.GetCurrentUser().EmpNum;
                            requisition.ApprovalDate = DateTime.Today;
                            if (SubmitButton == "Approve")
                            {
                                requisition.Status = "approved";
                                await _requistionRepo.UpdateAsync(requisition);
                                return RedirectToAction("Pending");
                            }

                            if (SubmitButton == "Reject")
                            {
                                requisition.Status = "rejected";
                                await _requistionRepo.UpdateAsync(requisition);
                                return RedirectToAction("Pending");
                            }
                        }
                        else
                        {
                            return View(requisition);
                        }
                    }
                }
                return View("_hasDelegate");
            }
            else
            {
                return new HttpUnauthorizedResult();
            }
        }


        //TODO: return create page, only showing necessary fields
        // GET: Requisition/Create
        //???
        [DelegateStaffCustomAuth("staff")]
        public ActionResult Create()
        {
            return View();
        }

        // TODO: 1. create new requisition, 2. it's status set to pending, 3. send notification to departmenthead
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

        // GET: Requisition/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
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
            List<Stationery> stationerys = _stationeryRepo.GetAll().ToList<Stationery>();
            if (searchString != null)
            { page = 1; }
            else
            {
                searchString = currentFilter;
            }
            if (!String.IsNullOrEmpty(searchString))
            {
                stationerys = _stationeryRepo.GetByDescription(searchString).ToList();
                //if no result, display no result therefore the next 4 lines can be deleted
                //if (stationerys.Count == 0)
                //{
                //    stationerys = _stationeryRepo.GetAll().ToList();
                //}
            }
            else
            {
                stationerys = _stationeryRepo.GetAll().ToList();
            }
            int pageSize = 15;
            int pageNumber = (page ?? 1);

            var stationeryList = stationerys.ToPagedList(pageNumber, pageSize);

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

        //GET: MyRequisitions
        //public async Task<ActionResult> EmpReq(int EmpNum)
        //{
        //    return View(_requistionRepo.GetRequisitionByEmpNum(EmpNum));
        //}
        [DelegateStaffCustomAuth("staff", "rep")]
        public ActionResult MyRequisitions(string currentFilter, int? page)
        {
            int id = _employeeRepo.GetCurrentUser().EmpNum;
            List<Requisition> reqlist = _requistionRepo.GetRequisitionByEmpNum(id).OrderByDescending(s => s.RequisitionDate).OrderByDescending(s => s.RequisitionId).ToList();
            int pageSize = 15;
            int pageNumber = (page ?? 1);
            return View(reqlist.ToPagedList(pageNumber, pageSize));
        }
        // GET: Requisitions/EmpReqDetail/5
        [DelegateStaffCustomAuth("staff", "rep")]
        [HttpGet]
        public ActionResult MyRequisitionDetails(int id)
        {
            List<RequisitionDetail> requisitionDetail = _requistionRepo.GetRequisitionDetail(id).ToList<RequisitionDetail>();
            return View(requisitionDetail);
        }

        [DelegateStaffCustomAuth("staff", "rep")]
        [HttpPost]
        public ActionResult SubmitReq()
        {
            var itemNum = (List<string>)Session["itemNub"];
            var itemQty = (List<int>)Session["itemQty"];
            Employee self = _employeeRepo.GetCurrentUser();
            int reqEmp = self.EmpNum;
            string body = "Description".PadRight(30, ' ') + "\t\t" + "UOM".PadRight(30, ' ') + "\t\t" + "Quantity".PadRight(30, ' ') + "\n";
            DateTime reqDate = System.DateTime.Now.Date;
            string status = "pending";
            string remarks = Request["remarks"];
            string deptCode = self.DeptCode;
            if (itemNum != null)
            {
                Requisition requisition = new Requisition()
                {
                    RequestRemarks = remarks,
                    RequisitionDate = reqDate,
                    RequisitionEmpNum = reqEmp,
                    Status = status,
                    DeptCode = deptCode
                };
                _requistionRepo.Add(requisition);
                for (int i = 0; i < itemNum.Count; i++)
                {
                    RequisitionDetail requisitionDetail = new RequisitionDetail()
                    {
                        RequisitionId = requisition.RequisitionId,
                        ItemNum = itemNum[i],
                        Quantity = itemQty[i]
                    };
                    _requistionRepo.AddRequisitionDetail(requisitionDetail);
                    body += _stationeryRepo.GetById(requisitionDetail.ItemNum).Description.PadRight(30, ' ') + "\t\t" + _stationeryRepo.GetById(requisitionDetail.ItemNum).UnitOfMeasure.PadRight(30, ' ') + "\t\t" + requisitionDetail.Quantity.ToString().PadRight(30, ' ') + "\n";
                }
                Session["itemNub"] = null;
                Session["itemQty"] = null;
                Session["MyCart"] = new ShoppingCart();
                //return View();
                //send email
                //invalid email address
                //string destinationEmail = erepo.GetById(erepo.GetDepartmentByUser(erepo.GetCurrentUser()).DeptHeadNum.ToString().ToString()).EmailAddress;
                string destinationEmail = "cuirunzesg@gmail.com";
                string subject = self.FullName + " requested stationeries";
                EmailHelper.SendEmail(destinationEmail, subject, body);
                return RedirectToAction("MyRequisitions");
            }

            return RedirectToAction("MyCart");
        }

        [DelegateStaffCustomAuth("staff", "rep")]
        public ActionResult MyCart()
        {
            ShoppingCart mycart = (ShoppingCart)Session["MyCart"];
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

            ShoppingCart mycart = Session["MyCart"] as ShoppingCart;
            Cart c = new Cart();
            foreach (Cart cart in mycart.shoppingCart)
            {
                if (cart.stationery.ItemNum == id)
                {
                     c = cart;
                    cart.quantity = qty;
                    break;
                }
            }
            if (c.quantity<=0)
            {
                mycart.shoppingCart.Remove(c);
            }
            return RedirectToAction("MyCart");
        }
        //Stock Clerk's page
        [Authorize(Roles = "clerk")]
        public ActionResult Consolidated()
        {

            return View(new RetrievalItemsWithDateDTO
            {
                retrievalItems = _requistionRepo.GetConsolidatedRequisition().ToList(),
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
                DateTime selectedDate = DateTime.ParseExact(listWithDate.collectionDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                _requistionRepo.ArrangeRetrievalAndDisbursement(selectedDate);
                return RedirectToAction("RetrievalInProcess");
            }

            return View("Consolidated", new RetrievalItemsWithDateDTO
            {
                retrievalItems = _requistionRepo.GetConsolidatedRequisition().ToList(),
                collectionDate = DateTime.Today.ToString("dd/MM/yyyy"),
                hasInprocessDisbursement = _disbursementRepo.hasInprocessDisbursements()
            });
        }

        //TODO: A method to display in process Retrieval
        [Authorize(Roles = "clerk")]
        public ActionResult RetrievalInProcess()
        {
            return View(_requistionRepo.GetRetrievalInPorcess());
        }

        [CustomAuthorize("head", "staff")]
        [HttpGet]
        public PartialViewResult _ApproveReq(int Id, String Status)
        {
            ReqApproveRejectDTO reqDTO = new ReqApproveRejectDTO
            {
                RequisitionId = Id,
                Status = Status
            };
            var empNum = Convert.ToInt32(Request.Cookies["Employee"]?["EmpNum"]);
            var isDelegated = _delegateRepo.FindCurrentByEmpNum(empNum) != null;
            if ((User.IsInRole("head") && !HasDelegate) || isDelegated)
            {
                return PartialView("_ApproveReq", reqDTO);
            }

            return PartialView("_hasDelegate");
        }


        [CustomAuthorize("head", "staff")]
        [HttpPost]
        public PartialViewResult _ApproveReq([Bind(Include = "RequisitionId,ApprovalRemarks,Status")]ReqApproveRejectDTO RADTO)
        {

            Requisition req = _requistionRepo.GetById(RADTO.RequisitionId);
            if (req != null)
            {
                if (req.Status == "pending")
                {//must be pending for approval and reject
                    Employee self = _employeeRepo.GetCurrentUser();
                    if ((self.JobTitle == "head" && !HasDelegate) || HasDelegate)
                    {//if (user is head and there is no delegate) or (user is currently delegate)
                        if (self.DeptCode != _employeeRepo.GetDepartmentByEmpNum(req.RequisitionEmpNum).DeptCode)
                        {//if user is trying to approve for other department
                            return PartialView("_unauthoriseAccess");
                        }
                        if ((self.EmpNum == req.RequisitionEmpNum))
                        {//if user is trying to self approve 
                            return PartialView("_unauthoriseAccess");
                        }
                        if (ModelState.IsValid)
                        {
                            if (req.ApprovalEmpNum == _employeeRepo.GetCurrentUser().EmpNum)
                            {

                                return PartialView("_unuthoriseAccess");
                            }
                            else
                            {
                                req.Status = RADTO.Status;
                                req.ApprovalRemarks = RADTO.ApprovalRemarks;
                                req.ApprovalEmpNum = _employeeRepo.GetCurrentUser().EmpNum;
                                req.ApprovalDate = DateTime.Today;
                                _requistionRepo.Update(req);

                                string destinationEmail = req.RequisitionEmployee.EmailAddress;         
                                string subject = "Requistion " + req.RequisitionId.ToString() + " made on " + req.RequisitionDate.ToString() + " has been " + RADTO.Status;
                                StringBuilder body = new StringBuilder("Your Requisition " + req.RequisitionId.ToString() + " made on " + req.RequisitionDate.ToString() + " has been " + RADTO.Status + " by " + req.ApprovalEmployee.FullName);
                                body.AppendLine("Requested: ");
                                List<RequisitionDetail> rd = _requistionRepo.GetRequisitionDetail(RADTO.RequisitionId).ToList();
                                if (rd != null)
                                {
                                    foreach (RequisitionDetail r in rd)
                                    {
                                        body.AppendLine("Item: " +r.Stationery.Description);
                                        body.AppendLine("Quantity: "+ r.Quantity.ToString());
                                        body.AppendLine("");
                                    }
                                }
                                else
                                {
                                    body.AppendLine("Nothing found");
                                }
                                EmailHelper.SendEmail(destinationEmail, subject, body.ToString());


                                return PartialView();


                            }
                        }
                        else { return PartialView(RADTO); } //invalid modelstate

                    }
                    else { return PartialView("_hasDelegate"); }
                }
                else { return PartialView("_unuthoriseAccess"); }
            }
            return PartialView("_unuthoriseAccess");
        }

       
        private class RetrievalItemDTO
        {
            public string BinNum { get; set; }
            public string Description { get; set; }
            public string UnitOfMeasure { get; set; }
            public int RequestedQty { get; set; }
            public int AvailableQty{ get; set; }

    }
    }
}
