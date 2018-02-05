using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LUSSIS.Models;
using LUSSIS.Repositories;
using OfficeOpenXml;
using System.IO;
using LUSSIS.Constants;

namespace LUSSIS.Controllers
{
    //Authors: Ton That Minh Nhat, Douglas Lee Kiat Hui
    [Authorize(Roles = Role.Clerk)]
    public class SuppliersController : Controller
    {
        private readonly SupplierRepository _supplierRepo = new SupplierRepository();
        private readonly StationeryRepository _stationeryRepo = new StationeryRepository();
        private readonly StationerySupplierRepository _stationerySupplierRepo = new StationerySupplierRepository();

        // GET: Suppliers
        public async Task<ActionResult> Index()
        {
            return View(await _supplierRepo.GetAllAsync());
        }

        // GET: Suppliers/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var supplier = await _supplierRepo.GetByIdAsync((int) id);
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
        public async Task<ActionResult> Create(
            [Bind(Include = "SupplierName,ContactName,TelephoneNum,FaxNum,Address,GstRegistration")]
            Supplier supplier)
        {
            if (!ModelState.IsValid) return View(supplier);

            await _supplierRepo.AddAsync(supplier);
            return RedirectToAction("Index");
        }

        // GET: Suppliers/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var supplier = await _supplierRepo.GetByIdAsync((int) id);
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
        public async Task<ActionResult> Edit(
            [Bind(Include = "SupplierId,SupplierName,ContactName,TelephoneNum,FaxNum,Address,GstRegistration")]
            Supplier supplier)
        {
            if (!ModelState.IsValid) return View(supplier);

            await _supplierRepo.UpdateAsync(supplier);
            return RedirectToAction("Index");
        }

        // GET: Suppliers/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var supplier = await _supplierRepo.GetByIdAsync((int) id);
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
            var supplier = await _supplierRepo.GetByIdAsync(id);
            try
            {
                await _supplierRepo.DeleteAsync(supplier);
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", @"This supplier has existing stationeries.");
                return View(supplier);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _supplierRepo.Dispose();
                _stationeryRepo.Dispose();
                _stationerySupplierRepo.Dispose();
            }

            base.Dispose(disposing);
        }

        //Authors: Douglas Lee Kiat Hui

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
                _stationerySupplierRepo.UpdateAll(list);


                ViewBag.Success = "Successfully uploaded";
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
            //get data to put into excel
            List<StationerySupplierQuote> slist =
                _stationerySupplierRepo.GetAll().Select(x => new StationerySupplierQuote
                {
                    ItemCode = x.ItemNum,
                    ItemName = x.Stationery.Description,
                    SupplierCode = x.SupplierId,
                    SupplierName = x.Supplier.SupplierName,
                    Rank = x.Rank,
                    UnitPrice = x.Price
                }).ToList();

            byte[] filecontent = StationerySupplierQuote.ConvertListToByte(slist);
            return File(filecontent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "quotations.xlsx");
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

            //method to convert c# list into format for excel parsing
            public static byte[] ConvertListToByte(IEnumerable<StationerySupplierQuote> slist)
            {
                byte[] filecontent;
                using (ExcelPackage pk = new ExcelPackage())
                {
                    ExcelWorksheet ws = pk.Workbook.Worksheets.Add("quotations");
                    //fill header
                    string[] headerString =
                        {"Item Code", "Item Name", "Unit Price", "Rank", "Supplier Code", "Supplier Name"};
                    for (int i = 1; i < 7; i++)
                    {
                        ws.Cells[1, i].Value = headerString[i - 1];
                    }

                    //fill data
                    ws.Cells["A2"].LoadFromCollection(slist);
                    ws.Cells["A1:F100"].AutoFitColumns();
                    filecontent = pk.GetAsByteArray();
                }

                return filecontent;
            }

            //method to convert the excel format back to c# list class
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
                        var stationerySupplier = new StationerySupplier
                        {
                            ItemNum = (string) ws.Cells[currentRow, 1].Value,
                            Rank = Convert.ToInt32(ws.Cells[currentRow, 4].Value),
                            Price = Convert.ToDouble(ws.Cells[currentRow, 3].Value),
                            SupplierId = Convert.ToInt32(ws.Cells[currentRow, 5].Value)
                        };
                        list.Add(stationerySupplier);
                        currentRow++;
                    }
                }
                catch (Exception)
                {
                    throw new Exception("Problems reading uploaded file, please follow template provided");
                }

                return list;
            }

            public static void ValidateData(List<StationerySupplier> list)
            {
                StationeryRepository srepo = new StationeryRepository();
                SupplierRepository repo = new SupplierRepository();

                // IEnumerable<StationerySupplier> originalList = _stationeryRepo.GetStationerySupplier();
                //check that all supplier id is valid
                if (list.Select(x => x.SupplierId).Distinct().Except(repo.GetAll().Select(x => x.SupplierId)).Any())
                    throw new Exception("Supplier Code is not valid");
                //check that stationery exists as in database, and there is no missing stationery
                List<string> itemlist = srepo.GetAll().Select(x => x.ItemNum).ToList();
                if (list.Select(x => x.ItemNum).Distinct().Except(itemlist).Any() ||
                    itemlist.Except(list.Select(x => x.ItemNum).Distinct()).Any())
                    throw new Exception("Stationery in the file does not match database");
                //List<string> itemlist2 = list.Where(x => x.Rank == 1).Select(x => x.ItemNum).Distinct().ToList();
                if (list.Where(x => x.Rank == 1).Select(x => x.ItemNum).Distinct().Count() !=
                    (srepo.GetAll().Select(x => x.ItemNum).Count()))
                    throw new Exception("Each stationery should have at least one primary supplier");
                //check that composite PK is ok (stationery-rank is distinct) 
                if (list.Select(x => new {x.ItemNum, x.Rank}).Distinct().Count() > list.Count)
                    throw new Exception("Stationery with duplicated supplier/ranks detected");
            }
        }

        #endregion
    }
}