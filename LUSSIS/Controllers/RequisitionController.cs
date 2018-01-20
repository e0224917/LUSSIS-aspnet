using LUSSIS.Models;
using LUSSIS.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PagedList;
using LUSSIS.Models.WebDTO;

namespace LUSSIS.Controllers
{
    public class RequisitionController : Controller
    {
        private LUSSISContext db = new LUSSISContext();
        private RequisitionRepository rr = new RequisitionRepository();
        private EmployeeRepository er = new EmployeeRepository();

        // GET: Requisition
        public ActionResult Pending()
        {
            return View(rr.GetRequisitionsByStatus("pending"));
        }


        [HttpGet]
        public async Task<ActionResult> Detail(int reqId)
        {
            var req = await rr.GetByIdAsync(reqId);
            if (req != null)
            {
                return View(req);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest); ;
        }
        [HttpPost]
        public async Task<ActionResult> Detail([Bind(Include = "RequisitionId,RequisitionEmpNum,RequisitionDate,RequestRemarks,ApprovalRemarks")] Requisition requisition, string SubmitButton)
        {
            if (ModelState.IsValid)
            {
                requisition.Status = "approved";
                requisition.ApprovalEmpNum = er.GetCurrentUser().EmpNum;
                requisition.ApprovalDate = DateTime.Today;
                if (SubmitButton == "Approve")
                {

                    await rr.UpdateAsync(requisition);
                    return RedirectToAction("index");
                }

                if (SubmitButton == "Reject")
                {

                    await rr.UpdateAsync(requisition);
                    return RedirectToAction("index");
                }
            }
            return RedirectToAction("index");
        }


        // GET: Requisition/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Requisition/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Requisition/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Requisition/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Requisition/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

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
            else { stationerys = strepo.GetAll().ToList(); }
            int pageSize = 15;
            int pageNumber = (page ?? 1);
            return View(stationerys.ToPagedList(pageNumber, pageSize));
        }

        // /Requisition/AddToCart
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
        //    return View(reqrepo.GetRequisitionByEmpNum(EmpNum));
        //}
        public ActionResult EmpReq(string currentFilter, int? page)
        {           
            int id=erepo.GetCurrentUser().EmpNum;
            List<Requisition> reqlist = reqrepo.GetRequisitionByEmpNum(id).OrderByDescending(s=>s.RequisitionDate).ToList();
            int pageSize = 15;
            int pageNumber = (page ?? 1);
            return View(reqlist.ToPagedList(pageNumber,pageSize));
        }
        // GET: Requisitions/Details/
        [HttpGet]
        public ActionResult EmpReqDetail(int id)
        {
            List<RequisitionDetail> requisitionDetail = reqrepo.GetRequisitionDetail(id).ToList<RequisitionDetail>();
            return View(requisitionDetail);
        }
        [HttpPost]
        public ActionResult SubmitReq()
        {
            var itemNum = (List<string> )Session["itemNub"];
            var itemQty = (List<int>)Session["itemQty"];
            int reqEmp = erepo.GetCurrentUser().EmpNum;
            DateTime reqDate = System.DateTime.Now.Date;
            string status = "pending";
            string remarks = Request["remarks"];
            Requisition requisition = new Requisition();
            requisition.RequestRemarks = remarks;
            requisition.RequisitionDate = reqDate;
            requisition.RequisitionEmpNum = reqEmp;
            requisition.Status = status;
            reqrepo.Add(requisition);
            for (int i=0;i<itemNum.Count;i++)
            {
                RequisitionDetail requisitionDetail = new RequisitionDetail();
                requisitionDetail.RequisitionId = requisition.RequisitionId;
                requisitionDetail.ItemNum =itemNum[i] ;
                requisitionDetail.Quantity = itemQty[i];
                reqrepo.AddRequisitionDetail(requisitionDetail);
            }
            Session["itemNub"] = null;
            Session["itemQty"] = null;
            Session["MyCart"]=new ShoppingCart();           
            //return View();
            return RedirectToAction("EmpReq");
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
        //Stock Clerk's page
        public ActionResult Consolidated()
        {
            return View(rr.GetConsolidatedRequisition());
        }
        //Stock Clerk click on generate button - post with date selected
        [HttpPost]
        public ActionResult Retrieve(DateTime? collectionDate)
        {
            //TODO: pass the selected DateTime object to controller
            return View(rr.ArrangeRetrievalAndDisbursement(new DateTime()));
        }

        [HttpGet]
        public ActionResult ApproveReq(int Id, String Status)
        {
            ReqApproveRejectDTO reqDTO = new ReqApproveRejectDTO();
            reqDTO.RequisitionId = Id;
            reqDTO.Status = Status;
            return PartialView("ApproveReq", reqDTO);
        }

        [HttpPost]
        public ActionResult ApproveReq([Bind(Include = "RequisitionId,ApprovalRemarks,Status")]ReqApproveRejectDTO RADTO)
        {
            if (ModelState.IsValid)
            {
                Requisition req = rr.GetById(RADTO.RequisitionId);
                req.Status = RADTO.Status;
                req.ApprovalRemarks = RADTO.ApprovalRemarks;
                req.ApprovalEmpNum = er.GetCurrentUser().EmpNum;
                req.ApprovalDate = DateTime.Today;
                rr.Update(req);
                return PartialView();
            }
            return PartialView(RADTO);
        }


            
    }
}
