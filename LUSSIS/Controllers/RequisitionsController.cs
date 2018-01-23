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

namespace LUSSIS.Controllers
{
    public class RequisitionsController : Controller
    {

        private RequisitionRepository reqRepo = new RequisitionRepository();
        private EmployeeRepository empRepo = new EmployeeRepository();
        private StationeryRepository statRepo = new StationeryRepository();
        private DisbursementRepository disRepo = new DisbursementRepository();

        //TODO: Add authroization - DepartmentHead or Delegate only
        // GET: Requisition
        public ActionResult Pending()
        {
            List<Requisition> req = reqRepo.GetPendingRequisitions();
            if (empRepo.GetCurrentUser().JobTitle == "head")
            {
                if (empRepo.CheckIfUserDepartmentHasDelegate())
                {
                    //If user is head and there is delegate
                    ViewBag.Message = "Delegate";
                }
                else
                {
                    //If user is head and there is not delegate
                    ViewBag.Message = "IsDelegateOrNoDelegate";
                }
            }
            else if (empRepo.CheckIfLoggedInUserIsDelegate())
            {
                ViewBag.Message = "IsDelegateOrNoDelegate";
            }
            else
            {
                return new HttpUnauthorizedResult();
            }
            return View(req);
        }

        //TODO: Add authroization - DepartmentHead or Delegate only
        [HttpGet]
        public ActionResult Detail(int reqId)
        {

            if (empRepo.GetCurrentUser().JobTitle == "head")
            {
                if (empRepo.CheckIfUserDepartmentHasDelegate())
                {
                    //If user is head and there is delegate
                    ViewBag.Message = "Delegate";
                }
                else
                {
                    //If user is head and there is not delegate
                    ViewBag.Message = "IsDelegateOrNoDelegate";
                }
            }
            else if (empRepo.CheckIfLoggedInUserIsDelegate())
            {
                ViewBag.Message = "IsDelegateOrNoDelegate";
            }
            else
            {//if user is staff but not delegate
                return new HttpUnauthorizedResult();
            }
            //other scenario will load the req to check if it is pending or not, non pending cases will be redirected to PDetail (pastDetails)
            var req = reqRepo.GetById(reqId);
            if (req != null)
            {
                if (req.Status == "pending")
                {
                    return View(req);
                }
                else
                {
                    return RedirectToAction("PDetails", "Requisitions", new { id = reqId });
                }
            }
            else
            {
                return new HttpNotFoundResult();
            }

        }



        //TODO: View requisition details (without approve and reject button)
        // [employee page] GET: Requisition/Details/5
        public ActionResult PDetail(int id)
        {
            if (empRepo.GetCurrentUser().JobTitle == "head")
            {
                if (empRepo.CheckIfUserDepartmentHasDelegate())
                {
                    //If user is head and there is delegate
                    ViewBag.Message = "Delegate";
                }
                else
                {
                    //If user is head and there is no delegate
                    ViewBag.Message = "IsDelegateOrNoDelegate";
                }
            }
            else if (empRepo.CheckIfLoggedInUserIsDelegate())
            {
                ViewBag.Message = "IsDelegateOrNoDelegate";
            }
            else
            {//if user is staff but not delegate
                return new HttpUnauthorizedResult();
            }
            var req = reqRepo.GetById(id);
            return View(req);
        }

        [HttpGet]
        public ActionResult All()
        {
            return View(reqRepo.GetAll());
        }


        //TODO: Add authroization - DepartmentHead or Delegate only
        [HttpPost]
        public async Task<ActionResult> Detail([Bind(Include = "RequisitionId,RequisitionEmpNum,RequisitionDate,RequestRemarks,ApprovalRemarks")] Requisition requisition, string SubmitButton)
        {
            if (requisition.Status == "pending")
            {
                if (empRepo.GetCurrentUser().JobTitle == "head" || empRepo.CheckIfLoggedInUserIsDelegate())
                {
                    if (ModelState.IsValid)
                    {
                        requisition.ApprovalEmpNum = empRepo.GetCurrentUser().EmpNum;
                        requisition.ApprovalDate = DateTime.Today;
                        if (SubmitButton == "Approve")
                        {
                            requisition.Status = "approved";
                            await reqRepo.UpdateAsync(requisition);
                            return RedirectToAction("index");
                        }

                        if (SubmitButton == "Reject")
                        {
                            requisition.Status = "reject";
                            await reqRepo.UpdateAsync(requisition);
                            return RedirectToAction("index");
                        }
                    }
                    else
                    {
                        return RedirectToAction("index");
                    }

                }
            }
            return new HttpUnauthorizedResult();
        }








        //TODO: return create page, only showing necessary fields
        // GET: Requisition/Create
        public ActionResult Create()
        {
            return View();
        }

        // TODO: 1. create new requisition, 2. it's status set to pending, 3. send notification to departmenthead
        // [employee page] POST: Requisition/Create
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
        public ActionResult Edit(int id)
        {
            return View();
        }

        // TODO: only enable editing if status is pending
        // [employee page]  POST: Requisition/Edit/5
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
        private RequisitionRepository reqrepo = new RequisitionRepository();
        private StationeryRepository strepo = new StationeryRepository();
        private EmployeeRepository erepo = new EmployeeRepository();
        // GET: DeptEmpReqs

        public ActionResult Index(string searchString, string currentFilter, int? page)
        {
            List<Stationery> stationerys = strepo.GetAll().ToList<Stationery>();
            if (searchString != null)
            { page = 1; }
            else
            {
                searchString = currentFilter;
            }
            if (!String.IsNullOrEmpty(searchString))
            { stationerys = strepo.GetByDescription(searchString).ToList(); }
            else
            {
                stationerys = strepo.GetAll().ToList();
                //Response.Write("<script> alert('Not found')</script>");
            }
            int pageSize = 15;
            int pageNumber = (page ?? 1);
            return View(stationerys.ToPagedList(pageNumber, pageSize));
        }

        // /Requisitions/AddToCart
        [HttpPost]
        public ActionResult AddToCart(string id, int qty)
        {
            Cart cart = new Cart(strepo.GetById(id), qty);
            (Session["MyCart"] as ShoppingCart).addToCart(cart);
            return RedirectToAction("Index");
            //return Json("ok");

        }
        //GET: MyRequisitions
        //public async Task<ActionResult> EmpReq(int EmpNum)
        //{
        //    return View(reqRepo.GetRequisitionByEmpNum(EmpNum));
        //}
        public ActionResult EmpReq(string currentFilter, int? page)
        {
            int id = erepo.GetCurrentUser().EmpNum;
            List<Requisition> reqlist = reqrepo.GetRequisitionByEmpNum(id).OrderByDescending(s => s.RequisitionDate).OrderByDescending(s => s.RequisitionId).ToList();
            int pageSize = 15;
            int pageNumber = (page ?? 1);
            return View(reqlist.ToPagedList(pageNumber, pageSize));
        }
        // GET: Requisitions/EmpReqDetail/5
        [HttpGet]
        public ActionResult EmpReqDetail(int id)
        {
            List<RequisitionDetail> requisitionDetail = reqRepo.GetRequisitionDetail(id).ToList<RequisitionDetail>();
            return View(requisitionDetail);
        }
        [HttpPost]
        public ActionResult SubmitReq()
        {
            var itemNum = (List<string>)Session["itemNub"];
            var itemQty = (List<int>)Session["itemQty"];
            int reqEmp = erepo.GetCurrentUser().EmpNum;
            string body = "Description".PadRight(30, ' ') + "\t\t" + "UOM".PadRight(30, ' ') + "\t\t" + "Quantity".PadRight(30, ' ') + "\n";
            DateTime reqDate = System.DateTime.Now.Date;
            string status = "pending";
            string remarks = Request["remarks"];
            if (itemNum != null)
            {
                Requisition requisition = new Requisition();
                requisition.RequestRemarks = remarks;
                requisition.RequisitionDate = reqDate;
                requisition.RequisitionEmpNum = reqEmp;
                requisition.Status = status;
                reqrepo.Add(requisition);
                for (int i = 0; i < itemNum.Count; i++)
                {
                    RequisitionDetail requisitionDetail = new RequisitionDetail();
                    requisitionDetail.RequisitionId = requisition.RequisitionId;
                    requisitionDetail.ItemNum = itemNum[i];
                    requisitionDetail.Quantity = itemQty[i];
                    reqrepo.AddRequisitionDetail(requisitionDetail);
                    body += strepo.GetById(requisitionDetail.ItemNum).Description.PadRight(30, ' ') + "\t\t" + strepo.GetById(requisitionDetail.ItemNum).UnitOfMeasure.PadRight(30, ' ') + "\t\t" + requisitionDetail.Quantity.ToString().PadRight(30, ' ') + "\n";
                }
                Session["itemNub"] = null;
                Session["itemQty"] = null;
                Session["MyCart"] = new ShoppingCart();
                //return View();
                //send email
                string destinationEmail = erepo.GetById(erepo.GetDepartmentByUser(erepo.GetCurrentUser()).DeptHeadNum.ToString().ToString()).EmailAddress;
                //string destinationEmail = "cuirunzesg@gmail.com";
                string subject = erepo.GetCurrentUser().FullName + " requested stationeries";
                EmailHelper emailHelper = new EmailHelper(destinationEmail, subject, body);
                return RedirectToAction("EmpReq");
            }
            else
            {
                return RedirectToAction("EmpCart");
            }
        }
        public ActionResult EmpCart()
        {
            ShoppingCart mycart = (ShoppingCart)Session["MyCart"];
            return View(mycart.GetAllCartItem());
        }
        [HttpPost]
        public ActionResult DeleteCartItem(string id, int qty)
        {

            ShoppingCart mycart = Session["MyCart"] as ShoppingCart;
            mycart.deleteCart(id);
            return RedirectToAction("EmpCart");
        }
        [HttpPost]
        public ActionResult UpdateCartItem(string id, int qty)
        {

            ShoppingCart mycart = Session["MyCart"] as ShoppingCart;
            foreach(Cart cart in mycart.shoppingCart)
            {
                if (cart.stationery.ItemNum==id)
                {
                    cart.quantity = qty;
                }
            }
            return RedirectToAction("EmpCart");
        }
        //Stock Clerk's page
        public ActionResult Consolidated()
        {

            return View(new RetrievalItemsWithDateDTO
            {
                retrievalItems = reqRepo.GetConsolidatedRequisition().ToList(),
                collectionDate = DateTime.Today.ToString("dd/MM/yyyy"),
                hasInprocessDisbursement = disRepo.hasInprocessDisbursements()
            });
        }

        //TODO: Add authorization - Stock Clerk only 
        //click on generate button - post with date selected
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Retrieve([Bind(Include = "collectionDate")] RetrievalItemsWithDateDTO listWithDate)
        {

            if (ModelState.IsValid)
            {
                DateTime selectedDate = DateTime.ParseExact(listWithDate.collectionDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                reqRepo.ArrangeRetrievalAndDisbursement(selectedDate);
                return RedirectToAction("RetrievalInProcess");
            }

            return View("Consolidated", new RetrievalItemsWithDateDTO
            {
                retrievalItems = reqRepo.GetConsolidatedRequisition().ToList(),
                collectionDate = DateTime.Today.ToString("dd/MM/yyyy"),
                hasInprocessDisbursement = disRepo.hasInprocessDisbursements()
            });
        }

        //TODO: A method to display in process Retrieval
        public ActionResult RetrievalInProcess()
        {
            return View(reqRepo.GetRetrievalInPorcess());
        }

        [HttpGet]
        public PartialViewResult _ApproveReq(int Id, String Status)
        {
            ReqApproveRejectDTO reqDTO = new ReqApproveRejectDTO
            {
                RequisitionId = Id,
                Status = Status
            };
            if (empRepo.GetCurrentUser().JobTitle == "head")
            {
                if (empRepo.CheckIfUserDepartmentHasDelegate())
                {
                    return PartialView("_hasDelegate");
                }
                else return PartialView("_ApproveReq", reqDTO);
            }
            else if (empRepo.CheckIfLoggedInUserIsDelegate())
            {
                return PartialView("_ApproveReq", reqDTO);
            }
            else return PartialView("_unauthoriseAccess");
        }

        [HttpPost]
        public PartialViewResult _ApproveReq([Bind(Include = "RequisitionId,ApprovalRemarks,Status")]ReqApproveRejectDTO RADTO)
        {
            if (ModelState.IsValid)
            {
                Requisition req = reqRepo.GetById(RADTO.RequisitionId);
                req.Status = RADTO.Status;
                req.ApprovalRemarks = RADTO.ApprovalRemarks;
                req.ApprovalEmpNum = empRepo.GetCurrentUser().EmpNum;
                req.ApprovalDate = DateTime.Today;
                reqRepo.Update(req);
                return PartialView();
            }
            return PartialView(RADTO);
        }



    }
}
