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
        public ActionResult Create1()
        {
            StationerynDTO stationerynDTO = new StationerynDTO();
            stationerynDTO.CategoryList = strepo.GetCategories();
            stationerynDTO.SupplierList = srepo.GetSupplierList();
            return View(stationerynDTO);
        }

        [HttpPost]
        public ActionResult Create1(StationerynDTO stationerynDTO)  //Create in MVC architecture
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
                        BinNum = initial + stationerynDTO.BinNum.ToString(),
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
        public ActionResult Edit1(string id) //Edit in MVC architecture
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
                    BinNum = Int32.Parse(st.BinNum.ToString().Substring(1)),
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
        public ActionResult Edit1(StationerynDTO stationerynDTO) //Edit in MVC architecture
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
                {
                    ViewBag.DistinctError = "Please select different suppliers";
                    stationerynDTO.CategoryList = strepo.GetCategories();
                    stationerynDTO.SupplierList = srepo.GetSupplierList();
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
            stationerynDTO.CategoryList = strepo.GetCategories();
            stationerynDTO.SupplierList = srepo.GetSupplierList();
            return View(stationerynDTO);
        }




            // GET: Stationeries/Create
            public ActionResult Create()
        {

            var categoryList = (from val in db.Categories
                                select new { CategoryId = val.CategoryId, CategoryName = val.CategoryName.Trim().Replace("\r", "").Replace("\n", "") }).ToList();

            ViewBag.Category = categoryList;

            //var supplierlist1 = (from val in db.Suppliers
            //                     join val1 in db.StationerySuppliers
            //                     on val.SupplierId equals val1.SupplierId
            //                     where val1.Rank == 1
            //                     select new { val.SupplierId, val.SupplierName }).Distinct().ToList();

            var supplierlist1 = (from val in db.Suppliers
                                 select new { val.SupplierId, val.SupplierName }).Distinct().ToList();

            ViewBag.Supplier1 = supplierlist1;

            ViewBag.Supplier2 = supplierlist1;
          
            ViewBag.Supplier3 = supplierlist1;

            return View();
        }

        // POST: Stationeries/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // public ActionResult Create([Bind(Include = "ItemNum,CategoryId,Description,ReorderLevel,ReorderQty,AverageCost,UnitOfMeasure,CurrentQty,BinNum,AvailableQty")] Stationery stationery)
        public ActionResult Create(StationeryDTO stationeryDT)
        {

            Stationery stationery = new Stationery();
            Int32 nextItemNum = 0;
            if (ModelState.IsValid)
            {
                double[] supplierPrice = { Convert.ToDouble(stationeryDT.Price1), Convert.ToDouble(stationeryDT.Price2), Convert.ToDouble(stationeryDT.Price3) };

                double averageItemprice1 = supplierPrice.Average();
                double averageItemprice = Math.Round(averageItemprice1, 2);

                stationery.CategoryId = stationeryDT.stationery.CategoryId;

                var maxItemNum = (from values1 in db.Stationeries
                                  where values1.CategoryId == stationeryDT.stationery.CategoryId
                                  select values1.ItemNum.Substring(1, 3)).ToList().Max();

                nextItemNum = (maxItemNum == null) ? 1 : Convert.ToInt32(maxItemNum) + 1;

                var intialChar = (from values1 in db.Stationeries
                                  where values1.CategoryId == stationeryDT.stationery.CategoryId
                                  select values1.ItemNum.Substring(0, 1)).ToList().FirstOrDefault();

                var itemNum = string.Concat(intialChar, nextItemNum.ToString().PadLeft(4 - nextItemNum.ToString().Length, '0'));
                stationery.Description = stationeryDT.stationery.Description;
                stationery.ReorderLevel = stationeryDT.stationery.ReorderLevel;
                stationery.ReorderQty = stationeryDT.stationery.ReorderQty;
                stationery.BinNum = stationeryDT.stationery.BinNum;
                stationery.UnitOfMeasure = stationeryDT.stationery.UnitOfMeasure;
                stationery.ItemNum = itemNum;
                stationery.CurrentQty = 0;
                stationery.AvailableQty = 0;
                stationery.AverageCost = averageItemprice;


                db.Stationeries.Add(stationery);
                db.SaveChanges();

                for (int i = 0; i < 3; i++)
                {
                    StationerySupplier stationerysupplier = new StationerySupplier();
                    if (i == 0)
                    {
                        stationerysupplier.ItemNum = itemNum;
                        stationerysupplier.SupplierId = stationeryDT.SupplierId1;
                        stationerysupplier.Price = stationeryDT.Price1;
                        stationerysupplier.Rank = 1;

                    }
                    else if (i == 1)
                    {
                        stationerysupplier.ItemNum = itemNum;
                        stationerysupplier.SupplierId = stationeryDT.SupplierId2;
                        stationerysupplier.Price = stationeryDT.Price2;
                        stationerysupplier.Rank = 2;
                    }
                    else
                    {
                        stationerysupplier.ItemNum = itemNum;
                        stationerysupplier.SupplierId = stationeryDT.SupplierId3;
                        stationerysupplier.Price = stationeryDT.Price3;
                        stationerysupplier.Rank = 3;
                    }
                    db.StationerySuppliers.Add(stationerysupplier);
                    db.SaveChanges();

                }

                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CategoryName",stationeryDT.stationery.CategoryId);
           
            return View(stationeryDT);
        }







        public ActionResult Edit(string id)
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
            var stationarySupplier = (from S1 in db.StationerySuppliers
                                      join S2 in db.Suppliers
                                      on S1.SupplierId equals S2.SupplierId
                                      where S1.ItemNum == id
                                      select new { S1.ItemNum, S1.SupplierId, S1.Price, S1.Rank, S2.SupplierName }).ToList();

            var supplierlist1 = (from val in db.Suppliers
                                 select new { val.SupplierId, val.SupplierName }).Distinct().ToList();

            ViewBag.Supplier1 = supplierlist1;
     
            ViewBag.Supplier2 = supplierlist1;
     
            ViewBag.Supplier3 = supplierlist1;


            if (stationarySupplier == null)
            {
                return HttpNotFound();
            }
            StationeryDTO stationeryDTO = new StationeryDTO();
            stationeryDTO.stationery = stationery;

            int recordCount = 0;
            foreach (var ssuplier in stationarySupplier)
            {
                recordCount++;
                if (recordCount == 1)
                {
                    stationeryDTO.SupplierName1 = ssuplier.SupplierName;
                    stationeryDTO.SupplierId1 = ssuplier.SupplierId;
                    stationeryDTO.Price1 = ssuplier.Price;
                }
                else if (recordCount == 2)
                {
                    stationeryDTO.SupplierName2 = ssuplier.SupplierName;
                    stationeryDTO.SupplierId2 = ssuplier.SupplierId;
                    stationeryDTO.Price2 = ssuplier.Price;
                }
                else
                {
                    stationeryDTO.SupplierName3 = ssuplier.SupplierName;
                    stationeryDTO.SupplierId3 = ssuplier.SupplierId;
                    stationeryDTO.Price3 = ssuplier.Price;
                }

            }

            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CategoryName", stationery.CategoryId);
            return View(stationeryDTO);
        }


        // POST: Stationeries/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StationeryDTO stationeryDTO)
        {
            Stationery stationery1 = new Stationery();
            int rel = Convert.ToInt32(stationeryDTO.stationery.ReorderLevel);
            if (ModelState.IsValid)
            {
                stationery1 = db.Stationeries.Find(stationeryDTO.stationery.ItemNum);

                stationery1.Description = stationeryDTO.stationery.Description;
                stationery1.ReorderLevel = stationeryDTO.stationery.ReorderLevel;
                stationery1.ReorderQty = stationeryDTO.stationery.ReorderQty;
                stationery1.BinNum = stationeryDTO.stationery.BinNum;
                stationery1.UnitOfMeasure = stationeryDTO.stationery.UnitOfMeasure;

                db.Entry(stationery1).State = EntityState.Modified;
                db.SaveChanges();

                double[] supplierPrice = { Convert.ToDouble(stationeryDTO.Price1), Convert.ToDouble(stationeryDTO.Price2), Convert.ToDouble(stationeryDTO.Price3) };
                double averageItemprice1 = supplierPrice.Average();
                double averageItemprice = Math.Round(averageItemprice1, 2);

                StationerySupplier stationerysupplier = new StationerySupplier(); ;
                for (int i = 0; i < 3; i++)
                {
                    stationerysupplier = new StationerySupplier();
                    if (i == 0)
                    {
                        stationerysupplier.ItemNum = stationeryDTO.stationery.ItemNum;
                        stationerysupplier.SupplierId = stationeryDTO.SupplierId1;
                        stationerysupplier.Price = stationeryDTO.Price1;
                        stationerysupplier.Rank = 1;

                    }
                    else if (i == 1)
                    {
                        stationerysupplier.ItemNum = stationeryDTO.stationery.ItemNum;
                        stationerysupplier.SupplierId = stationeryDTO.SupplierId2;
                        stationerysupplier.Price = stationeryDTO.Price2;
                        stationerysupplier.Rank = 2;
                    }
                    else
                    {
                        stationerysupplier.ItemNum = stationeryDTO.stationery.ItemNum;
                        stationerysupplier.SupplierId = stationeryDTO.SupplierId3;
                        stationerysupplier.Price = stationeryDTO.Price3;
                        stationerysupplier.Rank = 3;
                    }
                }
                db.Entry(stationerysupplier).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CategoryName", stationeryDTO.stationery.CategoryId);
            return View(stationeryDTO);
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
