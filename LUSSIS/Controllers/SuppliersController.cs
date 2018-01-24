using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LUSSIS.Models;
using LUSSIS.Repositories;
using OfficeOpenXml;
using System.IO;

namespace LUSSIS.Controllers
{
    [Authorize(Roles = "clerk")]
    public class SuppliersController : Controller
    {
        private SupplierRepository repo = new SupplierRepository();
        private StationeryRepository srepo = new StationeryRepository();

        // GET: Suppliers
        public async Task<ActionResult> Index()
        {
            return View(await repo.GetAllAsync());
        }

        // GET: Suppliers/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Supplier supplier = await repo.GetByIdAsync((int)id);
            if (supplier == null)
            {
                return HttpNotFound();
            }
            return View(supplier);
        }

        // GET: Suppliers/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Suppliers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "SupplierName,ContactName,TelephoneNum,FaxNum,Address,GstRegistration")] Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                await repo.AddAsync(supplier);
                return RedirectToAction("Index");
            }

            return View(supplier);
        }

        // GET: Suppliers/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Supplier supplier = await repo.GetByIdAsync((int)id);
            if (supplier == null)
            {
                return HttpNotFound();
            }
            return View(supplier);
        }

        // POST: Suppliers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "SupplierId,SupplierName,ContactName,TelephoneNum,FaxNum,Address,GstRegistration")] Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                await repo.UpdateAsync(supplier);
                return RedirectToAction("Index");
            }
            return View(supplier);
        }

        // GET: Suppliers/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Supplier supplier = await repo.GetByIdAsync((int)id);
            if (supplier == null)
            {
                return HttpNotFound();
            }
            return View(supplier);
        }

        // POST: Suppliers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Supplier supplier = await repo.GetByIdAsync(id);
            try
            {
                await repo.DeleteAsync(supplier);
                return RedirectToAction("Index");
            }
            catch (System.Exception e)
            {
                ModelState.AddModelError("", "This supplier has existed stationeries.");
                return View(supplier);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                repo.Dispose();
            }
            base.Dispose(disposing);
        }


        #region Quotations
        //GET: Suppliers/Quotation
        [HttpGet]
        public ActionResult Quotation()
        {
            return View();
        }

        //POST: Suppliers/Quotation
        [HttpPost]
        public ActionResult Quotation(HttpPostedFileBase file1)
        {
            try
            {
                if (file1 == null)
                    throw new Exception("No file was uploaded");

                //convert excel to list
                List<StationerySupplier> list = StationerySupplierQuote.ConvertToList(file1.InputStream);

                //validate data
                StationerySupplierQuote.ValidateData(list);

                //upload data
              //  srepo.UpdateAllStationerySupplier(list);


                ViewBag.Success = "No error detected";
                return View();

            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message;
                return View();
            }
        }

        //GET: Suppliers/QuotationTemplate (will download Excel file directly)
       public ActionResult QuotationTemplate()
        {
            List<StationerySupplierQuote> slist =
                srepo.GetAllStationerySuppliers().Select(x => new StationerySupplierQuote
                {
                    ItemCode = x.ItemNum,
                    ItemName = x.Stationery.Description,
                    SupplierCode = x.SupplierId,
                    SupplierName = x.Supplier.SupplierName,
                    Rank = x.Rank ?? 0,
                    UnitPrice = x.Price ?? 0
                }).ToList();

            byte[] filecontent = StationerySupplierQuote.ConvertListToByte(slist);
            return File(filecontent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "quotations.xlsx");
        }


        //helper class to import/export from/to Excel
        public class StationerySupplierQuote
        {
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
            public double UnitPrice { get; set; }
            public int Rank { get; set; }
            public int SupplierCode { get; set; }
            public string SupplierName { get; set; }



            public static byte[] ConvertListToByte(IEnumerable<StationerySupplierQuote> slist)
            {
                byte[] filecontent = null;
                using (ExcelPackage pk = new ExcelPackage())
                {
                    ExcelWorksheet ws = pk.Workbook.Worksheets.Add("quotations");
                    //fill header
                    string[] headerString = new string[] { "Item Code", "Item Name", "Unit Price", "Rank", "Supplier Code", "Supplier Name" };
                    for (int i = 1; i < 6; i++)
                    {
                        ws.Cells[1, i].Value = headerString[i - 1];
                    }
                    //fill data
                    ws.Cells["A2"].LoadFromCollection(slist);
                    filecontent = pk.GetAsByteArray();
                }

                return filecontent;
            }
            public static List<StationerySupplier> ConvertToList(Stream stream)
            {
                List<StationerySupplier> list = new List<StationerySupplier>();
                try
                {
                    var excel = new ExcelPackage(stream);
                    var ws = excel.Workbook.Worksheets.First();
                    int currentRow = 2;
                    while (ws.Cells[currentRow, 1].Value != null)
                    {
                        StationerySupplier ss = new StationerySupplier();
                        ss.ItemNum = (string)ws.Cells[currentRow, 1].Value;
                        ss.Rank = Convert.ToInt32(ws.Cells[currentRow, 4].Value);
                        ss.Price = Convert.ToDouble(ws.Cells[currentRow, 3].Value);
                        ss.SupplierId = Convert.ToInt32(ws.Cells[currentRow, 5].Value);
                        list.Add(ss);
                        currentRow++;
                    }
                }
                catch (Exception e)
                { throw new Exception("Problems reading uploaded file, please follow template provided"); }
                return list;
            }

            public static void ValidateData(List<StationerySupplier> list)
            {
                StationeryRepository srepo = new StationeryRepository();
                SupplierRepository repo = new SupplierRepository();

               // IEnumerable<StationerySupplier> originalList = srepo.GetStationerySupplier();
                //check that all supplier id is valid
                if (list.Select(x => x.SupplierId).Distinct().Except(repo.GetAll().Select(x => x.SupplierId)).Any())
                    throw new Exception("Supplier Code is not valid");
                List<string> itemlist = srepo.GetAll().Select(x => x.ItemNum).ToList();
                if (list.Select(x => x.ItemNum).Distinct().Except(itemlist).Any() || itemlist.Except(list.Select(x => x.ItemNum).Distinct()).Any())
                    throw new Exception("Stationery in the file does not match database");
                List<string> itemlist2 = list.Where(x => x.Rank == 1).Select(x => x.ItemNum).Distinct().ToList();
                if (list.Where(x => x.Rank == 1).Select(x => x.ItemNum).Distinct().Count() != (srepo.GetAll().Select(x => x.ItemNum).Count()))
                    throw new Exception("Each stationery should have at least one primary supplier");
                if (list.Select(x => new { x.ItemNum, x.Rank }).Distinct().Count() > list.Count)
                    throw new Exception("Stationery with duplicated supplier/ranks detected");
            }
        }
        #endregion
    }
}
