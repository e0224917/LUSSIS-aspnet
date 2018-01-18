using LUSSIS.Models;
using LUSSIS.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace LUSSIS.Controllers
{
    public class RequisitionController : Controller
    {
        private LUSSISContext db = new LUSSISContext();
        private RequisitionRepository rr = new RequisitionRepository();
        // GET: Requisition
        public ActionResult PendingRequisition()
        {
            List<Requisition> pendingReq = rr.GetRequisitionsByStatus("pending").ToList();
            return View(pendingReq);
        }

        public ActionResult Detail(int reqId)
        {
            var req = rr.GetById(reqId);
            if (req != null)
            {
                return View(req);
            }
            return HttpNotFound();
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
        // GET: DeptEmpReqs
        public ActionResult Index(string searchString, string currentFilter, int? page)
        {
            List<Stationery> stationerys = new List<Stationery>();
            if (searchString != null)
            { page = 1; }
            else
            {
                searchString = currentFilter;
            }
            if (!String.IsNullOrEmpty(searchString))
            { stationerys = strepo.GetByDescription(searchString).ToList(); }
            else { stationerys = strepo.GetAll().ToList(); }
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(stationerys.ToPagedList(pageNumber, pageSize));
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
    }
}
