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
            Department meDept = empRepo.GetCurrentUser().Department;
            Models.Delegate meDeptDelegate = empRepo.GetDelegateByDate(meDept, DateTime.Today);
            if (meDeptDelegate != null)
            {
                ViewBag.Message = "Delegate";
            }
            else
            {
                ViewBag.Message = "NoDelegate";
            }
            return View(req);
        }

        //TODO: Add authroization - DepartmentHead or Delegate only
        [HttpGet]
        public async Task<ActionResult> Detail(int reqId)
        {
            Department meDept = empRepo.GetCurrentUser().Department;
            Models.Delegate meDeptDelegate = empRepo.GetDelegateByDate(meDept, DateTime.Today);
            if (meDeptDelegate != null)
            {
                ViewBag.Message = "Delegate";
            }
            else
            {
                ViewBag.Message = "NoDelegate";
            }
            var req = await reqRepo.GetByIdAsync(reqId);
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
            { stationeries = statRepo.GetByDescription(searchString).ToList(); }
            else { stationeries = statRepo.GetAll().ToList(); }
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(stationeries.ToPagedList(pageNumber, pageSize));
        }

        //GET: MyRequisitions
        //public async Task<ActionResult> EmpReq(int EmpNum)
        //{
        //    return View(reqRepo.GetRequisitionByEmpNum(EmpNum));
        //}
        public ActionResult EmpReq()
        {
            return View(reqRepo.GetAll());
        }
        // GET: Requisitions/Details/
        [HttpGet]
        public ActionResult EmpReqDetail(int id)
        {
            List<RequisitionDetail> requisitionDetail = reqRepo.GetRequisitionDetail(id).ToList<RequisitionDetail>();
            return View(requisitionDetail);
        }


        //TODO: Add authorization - Stock Clerk only
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
