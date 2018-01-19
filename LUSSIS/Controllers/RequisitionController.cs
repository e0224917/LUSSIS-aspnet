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

namespace LUSSIS.Controllers
{
    public class RequisitionController : Controller
    {
       
        private RequisitionRepository rr = new RequisitionRepository();
        private EmployeeRepository er = new EmployeeRepository();


        //TODO: Add authroization - DepartmentHead or Delegate only
        // GET: Requisition
        public ActionResult Pending()
        {
            return View(rr.GetRequisitionsByStatus("pending"));
        }

        //TODO: Add authroization - DepartmentHead or Delegate only
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

        //TODO: Add authroization - DepartmentHead or Delegate only
        [HttpPost]
        public async Task<ActionResult> Detail([Bind(Include = "RequisitionId,RequisitionEmpNum,RequisitionDate,RequestRemarks,ApprovalRemarks")] Requisition requisition, string SubmitButton)
        {
            if (ModelState.IsValid)
            {

                requisition.ApprovalEmpNum = er.GetCurrentUser().EmpNum;
                requisition.ApprovalDate = DateTime.Today;
                if (SubmitButton == "Approve")
                {
                    requisition.Status = "approved";
                    await rr.UpdateAsync(requisition);
                    return RedirectToAction("index");
                }

                if (SubmitButton == "Reject")
                {
                    requisition.Status = "reject";
                    await rr.UpdateAsync(requisition);
                    return RedirectToAction("index");
                }
            }
            return RedirectToAction("index");
        }

        //TODO: View requisition details (without approve and reject button)
        // [employee page] GET: Requisition/Details/5
        public ActionResult Details(int id)
        {
            return View();
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

        private RequisitionRepository reqrepo = new RequisitionRepository();
        private StationeryRepository strepo = new StationeryRepository();
        // GET: DeptEmpReqs
        public ActionResult Index(string searchString, string currentFilter, int? page)
        {
            List<Stationery> stationeries = new List<Stationery>();
            if (searchString != null)
            { page = 1; }
            else
            {
                searchString = currentFilter;
            }
            if (!String.IsNullOrEmpty(searchString))
            { stationeries = strepo.GetByDescription(searchString).ToList(); }
            else { stationeries = strepo.GetAll().ToList(); }
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(stationeries.ToPagedList(pageNumber, pageSize));
        }

        //GET: MyRequisitions
        //public async Task<ActionResult> EmpReq(int EmpNum)
        //{
        //    return View(reqrepo.GetRequisitionByEmpNum(EmpNum));
        //}
        public ActionResult EmpReq()
        {
            return View(reqrepo.GetAll());
        }
        // GET: Requisitions/Details/
        [HttpGet]
        public ActionResult EmpReqDetail(int id)
        {
            List<RequisitionDetail> requisitionDetail = reqrepo.GetRequisitionDetail(id).ToList<RequisitionDetail>();
            return View(requisitionDetail);
        }
        
        
        //TODO: Add authorization - Stock Clerk only
        
        public ActionResult Consolidated()
        {
            return View(rr.GetConsolidatedRequisition());
        }

        //TODO: Add authorization - Stock Clerk only 
        //click on generate button - post with date selected
        [HttpPost]
        public ActionResult Retrieve(DateTime? collectionDate)
        {
            return View(rr.ArrangeRetrievalAndDisbursement(new DateTime()));
        }


    }
}
