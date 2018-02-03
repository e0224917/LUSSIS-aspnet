using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LUSSIS.Constants;
using LUSSIS.Models;
using LUSSIS.Models.WebDTO;
using LUSSIS.Repositories;
using PagedList;

namespace LUSSIS.Controllers
{
    //Authors: Maddireddi Venkata Rajeswari
    [Authorize(Roles = Role.Clerk)]
    public class StationeriesController : Controller
    {
        private readonly StationeryRepository _stationeryRepo = new StationeryRepository();
        private readonly StockAdjustmentRepository _adjustmentRepo = new StockAdjustmentRepository();
        private readonly DisbursementRepository _disbursementRepo = new DisbursementRepository();
        private readonly PORepository _poRepo = new PORepository();
        private readonly SupplierRepository _supplierRepo = new SupplierRepository();
        private readonly CategoryRepository _categoryRepo = new CategoryRepository();
        private readonly StationerySupplierRepository _stationerySupplierRepo = new StationerySupplierRepository();

        // GET: Stationeries
        public ActionResult Index(string searchString, string currentFilter, int? page, string sortOrder)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.BinSortParm = string.IsNullOrEmpty(sortOrder) ? "bin_desc" : "";
            ViewBag.CatSortParm = string.IsNullOrEmpty(sortOrder) ? "cat_desc" : "";
            ViewBag.DesSortParm = string.IsNullOrEmpty(sortOrder) ? "des_desc" : "";
            ViewBag.QtySortParm = sortOrder == "qty" ? "qty_desc" : "qty";
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var result = !string.IsNullOrEmpty(searchString)
                ? _stationeryRepo.GetByDescription(searchString).ToList()
                : _stationeryRepo.GetAll().ToList();
            switch (sortOrder)
            {
                case "bin_desc":
                    result = result.OrderByDescending(s => s.BinNum).ToList();
                    break;
                case "cat_desc":
                    result = result.OrderByDescending(s => s.CategoryId).ToList();
                    break;
                case "des_desc":
                    result = result.OrderByDescending(s => s.Description).ToList();
                    break;
                case "qty_desc":
                    result = result.OrderByDescending(s => s.CurrentQty).ToList();
                    break;
                default:
                    result = result.ToList();
                    break;
            }

            var stationeryAll = result.ToPagedList(pageNumber: page ?? 1, pageSize: 15);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_Index", stationeryAll);
            }

            return View(stationeryAll);
        }

        // GET: Stationeries/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            //get stationery data
            var stationery = _stationeryRepo.GetById(id);
            if (stationery == null)
                return HttpNotFound();

            //put stationery and the 3 suppliers into the view
            ViewBag.Stationery = stationery;
            ViewBag.Supplier1 = stationery.PrimarySupplier().SupplierName;
            ViewBag.Supplier2 = stationery.StationerySuppliers.First(x => x.Rank == 2).Supplier.SupplierName;
            ViewBag.Supplier3 = stationery.StationerySuppliers.First(x => x.Rank == 3).Supplier.SupplierName;

            //get full list of receive+disburse+adjust transactions for the stationery and put into view
            var receiveList = _poRepo.GetReceiveTransDetailByItem(id).Select(
                x => new StationeryTransactionDTO
                {
                    Date = x.ReceiveTran.ReceiveDate,
                    Qty = x.Quantity,
                    Transtype = "received",
                    Remarks = x.ReceiveTran.PurchaseOrder.Supplier.SupplierName
                }).ToList();
            var disburseList = _disbursementRepo.GetAllDisbursementDetailByItem(id).Select(
                x => new StationeryTransactionDTO
                {
                    Date = x.Disbursement.CollectionDate,
                    Qty = -x.ActualQty,
                    Transtype = "disburse",
                    Remarks = x.Disbursement.DeptCode
                }).ToList();
            var adjustList = _adjustmentRepo.GetApprovedAdjVoucherByItem(id).Select(
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
            ViewBag.InitBal = stationery.CurrentQty - p;
            ViewBag.StationeryTxList = receiveList.OrderBy(x => x.Date);

            return View();
        }

        [HttpGet]
        public ActionResult Create()
        {
            var stationeryDto = new StationeryDTO
            {
                CategoryList = _categoryRepo.GetCategories(),
                SupplierList = _supplierRepo.GetSupplierList()
            };
            return View(stationeryDto);
        }

        //POST: Stationeries/Create/
        [HttpPost]
        public ActionResult Create(StationeryDTO stationeryDto) //Create in MVC architecture
        {
            if (ModelState.IsValid)
            {
                //This is to check if supplier are unique
                var theList = new List<string>
                {
                    stationeryDto.SupplierName1,
                    stationeryDto.SupplierName2,
                    stationeryDto.SupplierName3
                };
                var isUnique = theList.Distinct().Count() == theList.Count();
                if (isUnique == false)
                {
                    ViewBag.DistinctError = "Please select different suppliers";
                    stationeryDto.CategoryList = _categoryRepo.GetCategories();
                    stationeryDto.SupplierList = _supplierRepo.GetSupplierList();
                    return View(stationeryDto);
                }

                var selectedCategory = _categoryRepo.GetById(stationeryDto.CategoryId);
                var initial = selectedCategory.CategoryName.Substring(0, 1);
                var number = _stationeryRepo.GetLastRunningPlusOne(initial).ToString();
                var generatedItemNum = initial + number.PadLeft(3, '0');
                var st = new Stationery
                {
                    ItemNum = generatedItemNum,
                    CategoryId = stationeryDto.CategoryId,
                    Description = stationeryDto.Description,
                    ReorderLevel = stationeryDto.ReorderLevel,
                    ReorderQty = stationeryDto.ReorderQty,
                    AverageCost = 0,
                    UnitOfMeasure = stationeryDto.UnitOfMeasure,
                    CurrentQty = 0,
                    BinNum = stationeryDto.BinNum,
                    AvailableQty = 0
                };
                _stationeryRepo.Add(st);

                var sp1 = new StationerySupplier
                {
                    ItemNum = generatedItemNum,
                    SupplierId = int.Parse(stationeryDto.SupplierName1),
                    Price = stationeryDto.Price1,
                    Rank = 1
                };
                _stationerySupplierRepo.Add(sp1);


                var sp2 = new StationerySupplier
                {
                    ItemNum = generatedItemNum,
                    SupplierId = int.Parse(stationeryDto.SupplierName2),
                    Price = stationeryDto.Price2,
                    Rank = 2
                };
                _stationerySupplierRepo.Add(sp2);

                var sp3 = new StationerySupplier
                {
                    ItemNum = generatedItemNum,
                    SupplierId = int.Parse(stationeryDto.SupplierName3),
                    Price = stationeryDto.Price3,
                    Rank = 3
                };
                _stationerySupplierRepo.Add(sp3);
                return RedirectToAction("Index");
            }

            stationeryDto.CategoryList = _categoryRepo.GetCategories();
            stationeryDto.SupplierList = _supplierRepo.GetSupplierList();
            return View(stationeryDto);
        }

        // POST: Stationeries/Edit/
        [HttpGet]
        public ActionResult Edit(string id) //Edit in MVC architecture
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var st = _stationeryRepo.GetById(id);
            if (st == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var nDto = new StationeryDTO
            {
                SupplierList = _supplierRepo.GetSupplierList(),
                CategoryList = _categoryRepo.GetCategories(),
                ItemNum = id,
                BinNum = st.BinNum,
                CategoryId = st.CategoryId,
                Description = st.Description,
                ReorderLevel = st.ReorderLevel,
                ReorderQty = st.ReorderQty,
                UnitOfMeasure = st.UnitOfMeasure
            };
            var ss1 = _stationerySupplierRepo.GetSSByIdRank(id, 1);
            nDto.SupplierName1 = ss1.SupplierId.ToString();
            nDto.Price1 = ss1.Price;
            var ss2 = _stationerySupplierRepo.GetSSByIdRank(id, 2);
            nDto.SupplierName2 = ss2.SupplierId.ToString();
            nDto.Price2 = ss2.Price;
            var ss3 = _stationerySupplierRepo.GetSSByIdRank(id, 3);
            nDto.SupplierName3 = ss3.SupplierId.ToString();
            nDto.Price3 = ss3.Price;

            return View(nDto);
        }

        [HttpPost]
        public ActionResult Edit(StationeryDTO stationeryDto) //Edit in MVC architecture
        {
            if (ModelState.IsValid)
            {
                var theList = new List<string>
                {
                    stationeryDto.SupplierName1,
                    stationeryDto.SupplierName2,
                    stationeryDto.SupplierName3
                };
                var isUnique = theList.Distinct().Count() == theList.Count();
                if (isUnique == false)
                {
                    //Check Supplier is different
                    ViewBag.DistinctError = "Please select different suppliers";
                    stationeryDto.CategoryList = _categoryRepo.GetCategories();
                    stationeryDto.SupplierList = _supplierRepo.GetSupplierList();
                    return View(stationeryDto);
                }

                var stationery = _stationeryRepo.GetById(stationeryDto.ItemNum);
                stationery.CategoryId = stationeryDto.CategoryId;
                stationery.Description = stationeryDto.Description;
                stationery.ReorderLevel = stationeryDto.ReorderLevel;
                stationery.ReorderQty = stationeryDto.ReorderQty;
                stationery.UnitOfMeasure = stationeryDto.UnitOfMeasure;
                stationery.BinNum = stationeryDto.BinNum;
                _stationeryRepo.Update(stationery);
                _stationerySupplierRepo.DeleteStationerySupplier(stationeryDto.ItemNum);

                var sp1 = new StationerySupplier
                {
                    ItemNum = stationeryDto.ItemNum,
                    SupplierId = Int32.Parse(stationeryDto.SupplierName1),
                    Price = stationeryDto.Price1,
                    Rank = 1
                };
                _stationerySupplierRepo.Add(sp1);

                var sp2 = new StationerySupplier
                {
                    ItemNum = stationeryDto.ItemNum,
                    SupplierId = int.Parse(stationeryDto.SupplierName2),
                    Price = stationeryDto.Price2,
                    Rank = 2
                };
                _stationerySupplierRepo.Add(sp2);

                var sp3 = new StationerySupplier
                {
                    ItemNum = stationeryDto.ItemNum,
                    SupplierId = Int32.Parse(stationeryDto.SupplierName3),
                    Price = stationeryDto.Price3,
                    Rank = 3
                };
                _stationerySupplierRepo.Add(sp3);

                return RedirectToAction("Index");
            }

            stationeryDto.CategoryList = _categoryRepo.GetCategories();
            stationeryDto.SupplierList = _supplierRepo.GetSupplierList();
            return View(stationeryDto);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stationeryRepo.Dispose();
                _adjustmentRepo.Dispose();
                _supplierRepo.Dispose();
                _poRepo.Dispose();
                _disbursementRepo.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}