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
            StationerynDTO stationerynDTO = new StationerynDTO
            {
                CategoryList = strepo.GetCategories(),
                SupplierList = srepo.GetSupplierList()
            };
            return View(stationerynDTO);
        }

        [HttpPost]
        public ActionResult Create(StationerynDTO stationerynDTO)  //Create in MVC architecture
        {

            if (ModelState.IsValid)
            {
                //This is to check if supplier are unique
                List<string> theList = new List<string>
                {
                    stationerynDTO.SupplierName1,
                    stationerynDTO.SupplierName2,
                    stationerynDTO.SupplierName3
                };
                bool isUnique = theList.Distinct().Count() == theList.Count();
                if (isUnique == false)
                {
                    ViewBag.DistinctError = "Please select different suppliers";
                    stationerynDTO.CategoryList = strepo.GetCategories();
                    stationerynDTO.SupplierList = srepo.GetSupplierList();
                    return View(stationerynDTO);
                }
                else
                {
                    string initial = strepo.GetCategoryInitial(stationerynDTO.CategoryId);
                    string number = strepo.GetLastRunningPlusOne(initial).ToString();
                    string generatedItemNum = initial + number.PadLeft(3, '0');
                    Stationery st = new Stationery
                    {
                        ItemNum = generatedItemNum,
                        CategoryId = Int32.Parse(stationerynDTO.CategoryId),
                        Description = stationerynDTO.Description,
                        ReorderLevel = stationerynDTO.ReorderLevel,
                        ReorderQty = stationerynDTO.ReorderQty,
                        AverageCost = 0,
                        UnitOfMeasure = stationerynDTO.UnitOfMeasure,
                        CurrentQty = 0,
                        BinNum = stationerynDTO.BinNum,
                        AvailableQty = 0
                    };
                    strepo.Add(st);

                    StationerySupplier sp1 = new StationerySupplier
                    {
                        ItemNum = generatedItemNum,
                        SupplierId = Int32.Parse(stationerynDTO.SupplierName1),
                        Price = stationerynDTO.Price1,
                        Rank = 1
                    };
                    strepo.AddSS(sp1);


                    StationerySupplier sp2 = new StationerySupplier
                    {
                        ItemNum = generatedItemNum,
                        SupplierId = Int32.Parse(stationerynDTO.SupplierName2),
                        Price = stationerynDTO.Price2,
                        Rank = 2
                    };
                    strepo.AddSS(sp2);

                    StationerySupplier sp3 = new StationerySupplier
                    {
                        ItemNum = generatedItemNum,
                        SupplierId = Int32.Parse(stationerynDTO.SupplierName3),
                        Price = stationerynDTO.Price3,
                        Rank = 3
                    };
                    strepo.AddSS(sp3);
                    return RedirectToAction("Index");
                }

            }
            stationerynDTO.CategoryList = strepo.GetCategories();
            stationerynDTO.SupplierList = srepo.GetSupplierList();
            return View(stationerynDTO);
        }

        [HttpGet]
        public ActionResult Edit(string id) //Edit in MVC architecture
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                Stationery st = strepo.GetById(id);
                StationerynDTO nDTO = new StationerynDTO
                {
                    SupplierList = srepo.GetSupplierList(),
                    CategoryList = strepo.GetCategories(),
                    ItemNum = id,
                    BinNum = st.BinNum,
                    CategoryId = st.CategoryId.ToString(),
                    Description = st.Description,
                    ReorderLevel = st.ReorderLevel,
                    ReorderQty = st.ReorderQty,
                    UnitOfMeasure = st.UnitOfMeasure
                };
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
        public ActionResult Edit(StationerynDTO stationerynDTO) //Edit in MVC architecture
        {
            if (ModelState.IsValid)
            {
                List<string> theList = new List<string>
                {
                    stationerynDTO.SupplierName1,
                    stationerynDTO.SupplierName2,
                    stationerynDTO.SupplierName3
                };
                bool isUnique = theList.Distinct().Count() == theList.Count();
                if (isUnique == false)
                {//Check Supplier is different
                    ViewBag.DistinctError = "Please select different suppliers";
                    stationerynDTO.CategoryList = strepo.GetCategories();
                    stationerynDTO.SupplierList = srepo.GetSupplierList();
                    return View(stationerynDTO);
                }
                else
                {
                    Stationery st = strepo.GetById(stationerynDTO.ItemNum);
                    st.CategoryId = Int32.Parse(stationerynDTO.CategoryId);
                    st.Description = stationerynDTO.Description;
                    st.ReorderLevel = stationerynDTO.ReorderLevel;
                    st.ReorderQty = stationerynDTO.ReorderQty;
                    st.UnitOfMeasure = stationerynDTO.UnitOfMeasure;
                    st.BinNum = stationerynDTO.BinNum;
                    strepo.Update(st);
                    strepo.DeleteStationerySUpplier(stationerynDTO.ItemNum);

                    StationerySupplier sp1 = new StationerySupplier
                    {
                        ItemNum = stationerynDTO.ItemNum,
                        SupplierId = Int32.Parse(stationerynDTO.SupplierName1),
                        Price = stationerynDTO.Price1,
                        Rank = 1
                    };
                    strepo.AddSS(sp1);

                    StationerySupplier sp2 = new StationerySupplier
                    {
                        ItemNum = stationerynDTO.ItemNum,
                        SupplierId = Int32.Parse(stationerynDTO.SupplierName2),
                        Price = stationerynDTO.Price2,
                        Rank = 2
                    };
                    strepo.AddSS(sp2);

                    StationerySupplier sp3 = new StationerySupplier
                    {
                        ItemNum = stationerynDTO.ItemNum,
                        SupplierId = Int32.Parse(stationerynDTO.SupplierName3),
                        Price = stationerynDTO.Price3,
                        Rank = 3
                    };
                    strepo.AddSS(sp3);

                    return RedirectToAction("Index");
                }

            }
            stationerynDTO.CategoryList = strepo.GetCategories();
            stationerynDTO.SupplierList = srepo.GetSupplierList();
            return View(stationerynDTO);
        }

        // GET: Stationeries/Delete/5
        //public ActionResult Delete(string id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Stationery stationery = db.Stationeries.Find(id);
        //    if (stationery == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(stationery);
        //}

        //// POST: Stationeries/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(string id)
        //{
        //    Stationery stationery = db.Stationeries.Find(id);
        //    db.Stationeries.Remove(stationery);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

    }
}
