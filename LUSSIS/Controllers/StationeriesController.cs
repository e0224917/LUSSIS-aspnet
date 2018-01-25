using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LUSSIS.Models;
using LUSSIS.Models.WebDTO;
using LUSSIS.Repositories;
using PagedList;
namespace LUSSIS.Controllers
{
    public class StationeriesController : Controller
    {
        private LUSSISContext db = new LUSSISContext();
        private StationeryRepository strepo = new StationeryRepository();
        private StockAdjustmentRepository adrepo = new StockAdjustmentRepository();
        private DisbursementRepository disrepo = new DisbursementRepository();
        private PORepository porepo = new PORepository();
        private SupplierRepository srepo = new SupplierRepository();


        // GET: Stationeries
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
        
        // GET: Stationeries/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Stationery s = strepo.GetById(id);
            if (s == null)
                return HttpNotFound();
            ViewBag.Stationery = s;
            ViewBag.Supplier1 = s.PrimarySupplier().SupplierName;
            ViewBag.Supplier2 = s.StationerySuppliers.Where(x => x.Rank == 2).First().Supplier.SupplierName;
            ViewBag.Supplier3 = s.StationerySuppliers.Where(x => x.Rank == 3).First().Supplier.SupplierName;
            List<StationeryTransactionDTO> receiveList = porepo.GetReceiveTransDetailByItem(id).Select(
                x => new StationeryTransactionDTO
                {
                    Date = x.ReceiveTran.ReceiveDate,
                    Qty = x.Quantity,
                    Transtype = "received",
                    Remarks = x.ReceiveTran.PurchaseOrder.Supplier.SupplierName
                }).ToList();
            List<StationeryTransactionDTO> disburseList = disrepo.GetAllDisbursementDetailByItem(id).Select(
                x => new StationeryTransactionDTO
                {
                    Date = x.Disbursement.CollectionDate,
                    Qty = -x.ActualQty,
                    Transtype = "disburse",
                    Remarks = x.Disbursement.DeptCode
                }).ToList();
            List<StationeryTransactionDTO> adjustList = adrepo.GetApprovedAdjVoucherByItem(id).Select(
                x => new StationeryTransactionDTO
                {
                    Date = x.CreateDate,
                    Qty = x.Quantity,
                    Transtype = "adjust",
                    Remarks = x.Reason
                }).ToList();
            receiveList.AddRange(disburseList);
            receiveList.AddRange(adjustList);
            var p = receiveList.Sum(x => x.Qty);
            ViewBag.InitBal = s.CurrentQty - p;
            ViewBag.StationeryTxList =receiveList.OrderBy(x=>x.Date);

            return View();

        }

        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.Category = strepo.GetAllCategories();
            ViewBag.Suppliers = srepo.GetAll();
            return View();
        }

        [HttpPost]
        public ActionResult Create(StationerynDTO stationerynDTO)
        {

            if (ModelState.IsValid)
            {
                List<string> theList = new List<string>();
                theList.Add(stationerynDTO.SupplierName1);
                theList.Add(stationerynDTO.SupplierName2);
                theList.Add(stationerynDTO.SupplierName3);
                bool isUnique = theList.Distinct().Count() == theList.Count();
                if (isUnique == false)
                {
                    ViewBag.DistinctError = "Please select different suppliers";
                    ViewBag.Category = strepo.GetAllCategories();
                    ViewBag.Suppliers = srepo.GetAll();
                    return View(stationerynDTO);
                }
                else
                {

                    Stationery st = new Stationery();
                    string initial = strepo.GetCategoryInitial(stationerynDTO.CategoryId);
                    string number = strepo.GetLastRunningPlusOne(initial).ToString();
                    string generatedItemNum = initial + number.PadLeft(3, '0');
                    st.ItemNum = generatedItemNum;
                    st.CategoryId = Int32.Parse(stationerynDTO.CategoryId);
                    st.Description = stationerynDTO.Description;
                    st.ReorderLevel = stationerynDTO.ReorderLevel;
                    st.ReorderQty = stationerynDTO.ReorderQty;
                    st.AverageCost = 0;
                    st.UnitOfMeasure = stationerynDTO.UnitOfMeasure;
                    st.CurrentQty = 0;
                    st.BinNum = initial + stationerynDTO.BinNum.ToString();
                    st.AvailableQty = 0;
                    strepo.Add(st);

                    StationerySupplier sp1 = new StationerySupplier();
                    sp1.ItemNum = generatedItemNum;
                    sp1.SupplierId = Int32.Parse(stationerynDTO.SupplierName1);
                    sp1.Price = stationerynDTO.Price1;
                    sp1.Rank = 1;
                    strepo.AddSS(sp1);


                    StationerySupplier sp2 = new StationerySupplier();
                    sp2.ItemNum = generatedItemNum;
                    sp2.SupplierId = Int32.Parse(stationerynDTO.SupplierName2);
                    sp2.Price = stationerynDTO.Price2;
                    sp2.Rank = 2;
                    strepo.AddSS(sp2);

                    StationerySupplier sp3 = new StationerySupplier();
                    sp3.ItemNum = generatedItemNum;
                    sp3.SupplierId = Int32.Parse(stationerynDTO.SupplierName3);
                    sp3.Price = stationerynDTO.Price3;
                    sp3.Rank = 3;
                    strepo.AddSS(sp3);
                    return RedirectToAction("Index");
                }

            }
            ViewBag.Category = strepo.GetAllCategories();
            ViewBag.Suppliers = srepo.GetAll();
            return View(stationerynDTO);
        }

        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                ViewBag.Category = strepo.GetAllCategories();
                ViewBag.Suppliers = srepo.GetAll();
                Stationery st = strepo.GetById(id);
                StationerynDTO nDTO = new StationerynDTO();
                nDTO.ItemNum = id;
                nDTO.BinNum = Int32.Parse(st.BinNum.ToString().Substring(1));
                nDTO.CategoryId = st.CategoryId.ToString();
                nDTO.Description = st.Description;
                nDTO.ReorderLevel = st.ReorderLevel;
                nDTO.ReorderQty = st.ReorderQty;
                nDTO.UnitOfMeasure = st.UnitOfMeasure;
                StationerySupplier ss1 = strepo.GetSSByIdRank(id,1);
                nDTO.SupplierName1 = ss1.SupplierId.ToString();
                nDTO.Price1 = ss1.Price;
                StationerySupplier ss2 = strepo.GetSSByIdRank(id,2);
                nDTO.SupplierName2 = ss2.SupplierId.ToString();
                nDTO.Price2 = ss2.Price;
                StationerySupplier ss3 = strepo.GetSSByIdRank(id, 3);
                nDTO.SupplierName3 = ss3.SupplierId.ToString();
                nDTO.Price3 = ss3.Price;

                return View(nDTO);
            }
        }

        [HttpPost]
        public ActionResult Edit(StationerynDTO stationerynDTO)
        {
            if (ModelState.IsValid)
            {
                List<string> theList = new List<string>();
                theList.Add(stationerynDTO.SupplierName1);
                theList.Add(stationerynDTO.SupplierName2);
                theList.Add(stationerynDTO.SupplierName3);
                bool isUnique = theList.Distinct().Count() == theList.Count();
                if (isUnique == false)
                {
                    ViewBag.DistinctError = "Please select different suppliers";
                    ViewBag.Category = strepo.GetAllCategories();
                    ViewBag.Suppliers = srepo.GetAll();
                    return View(stationerynDTO);
                }
                else
                {

                    Stationery st = strepo.GetById(stationerynDTO.ItemNum);
                    string initial = strepo.GetCategoryInitial(stationerynDTO.CategoryId);
                    st.CategoryId = Int32.Parse(stationerynDTO.CategoryId);
                    st.Description = stationerynDTO.Description;
                    st.ReorderLevel = stationerynDTO.ReorderLevel;
                    st.ReorderQty = stationerynDTO.ReorderQty;
                    st.UnitOfMeasure = stationerynDTO.UnitOfMeasure;
                    st.BinNum = initial + stationerynDTO.BinNum.ToString();
                    strepo.Update(st);
                    strepo.DeleteStationerySUpplier(stationerynDTO.ItemNum);

                    StationerySupplier sp1 = new StationerySupplier();
                    sp1.ItemNum = stationerynDTO.ItemNum;
                    sp1.SupplierId = Int32.Parse(stationerynDTO.SupplierName1);
                    sp1.Price = stationerynDTO.Price1;
                    sp1.Rank = 1;
                    strepo.AddSS(sp1);


                    StationerySupplier sp2 = new StationerySupplier();
                    sp2.ItemNum = stationerynDTO.ItemNum;
                    sp2.SupplierId = Int32.Parse(stationerynDTO.SupplierName2);
                    sp2.Price = stationerynDTO.Price2;
                    sp2.Rank = 2;
                    strepo.AddSS(sp2);

                    StationerySupplier sp3 = new StationerySupplier();
                    sp3.ItemNum = stationerynDTO.ItemNum;
                    sp3.SupplierId = Int32.Parse(stationerynDTO.SupplierName3);
                    sp3.Price = stationerynDTO.Price3;
                    sp3.Rank = 3;
                    strepo.AddSS(sp3);

                    return RedirectToAction("Index");
                }

            }
            ViewBag.Category = strepo.GetAllCategories();
            ViewBag.Suppliers = srepo.GetAll();
            return View(stationerynDTO);
        }

        // GET: Stationeries/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Stationery stationery = db.Stationeries.Find(id);
            if (stationery == null)
            {
                return HttpNotFound();
            }
            return View(stationery);
        }

        // POST: Stationeries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Stationery stationery = db.Stationeries.Find(id);
            db.Stationeries.Remove(stationery);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
