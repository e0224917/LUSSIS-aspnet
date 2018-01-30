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
using LUSSIS.Models.WebDTO;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using System.Diagnostics;
using LUSSIS.Constants;

namespace LUSSIS.Controllers
{
    //Authors: May Zin Ko Authors: Douglas Lee Kiat Hui
    [Authorize(Roles = "manager, supervisor")]
    public class SupervisorDashboardController : Controller
    {
        protected PORepository _poRepo = new PORepository();
        protected DisbursementRepository _disbursementRepo = new DisbursementRepository();
        private StockAdjustmentRepository _stockAdjustmentRepo = new StockAdjustmentRepository();
        private StationeryRepository _stationeryRepo = new StationeryRepository();
        private EmployeeRepository _employeeRepo = new EmployeeRepository();
        private readonly SupplierRepository _supplierRepo = new SupplierRepository();
        private readonly CategoryRepository _categoryRepo = new CategoryRepository();
        private readonly DepartmentRepository _departmentRepo = new DepartmentRepository();

        public ActionResult Index()
        {
            ViewBag.Message = User.IsInRole(Role.Supervisor) ? Role.Supervisor : Role.Manager;
            var totalAddAdjustmentQty = _stockAdjustmentRepo.GetPendingAdjustmentByType("add").Count;
            var totalSubtractAdjustmentQty = _stockAdjustmentRepo.GetPendingAdjustmentByType("subtract").Count;
            List<String> fromList = new List<String>();
            fromList.Add(DateTime.Now.AddMonths(-3).ToString("dd/MM/yyyy"));
            fromList.Add(DateTime.Now.AddMonths(-2).ToString("dd/MM/yyyy"));
            fromList.Add(DateTime.Now.AddMonths(-1).ToString("dd/MM/yyyy"));
            var dash = new SupervisorDashboardDTO
            {
                PendingPOTotalAmount = _poRepo.GetPendingPOTotalAmount(),
                PendingPOCount = _poRepo.GetPendingPOCount(),
                POTotalAmount = _poRepo.GetPOTotalAmount(fromList),
                PendingStockAdjAddQty = totalAddAdjustmentQty,
                PendingStockAdjSubtractQty = totalSubtractAdjustmentQty,
                PendingStockAdjCount = _stockAdjustmentRepo.GetPendingAdjustmentList().Count,
                TotalDisbursementAmount = _disbursementRepo.GetDisbursementTotalAmount(fromList)
            };

            return View(dash);
        }

        public JsonResult GetPiechartJson(string list, string date, string e)
        {
            var pileName = _categoryRepo.GetAllCategoryName().ToList();
            var pileValue = _poRepo.GetPOByCategory();

            return Json(new { ListOne = pileName, ListTwo = pileValue }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBarchartJson()
        {
            var deptNames = _departmentRepo.GetAll().Select(item => item.DeptName).ToList();
            var deptCodes = _departmentRepo.GetAll().Select(item => item.DeptCode).ToList();

            var deptValues = new List<double>();
            foreach (var deptCode in deptCodes)
            {
                deptValues.Add(_disbursementRepo.GetDisbursementTotalAmountOfDept(deptCode));
            }

            return Json(new { firstList = deptNames, secondList = deptValues },
                JsonRequestBehavior.AllowGet);
        }



        public JsonResult GetReportJSON(String supplier_values, String category_values, String date)
        {
            List<String> pileName = _categoryRepo.GetAllCategoryName().ToList();
            List<double> pileValue = _poRepo.GetPOByCategory();

            return Json(new { ListOne = pileName, ListTwo = pileValue }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetStackJSON()
        {
            List<String> depList = _departmentRepo.GetAllDepartmentCode();
            List<int> catList = _categoryRepo.GetAllCategoryIds();

            List<String> fromList = new List<String>();
            List<String> toList = new List<String>();

            List<String> datevalue = new List<String>();

            List<double> xvalue = new List<double>();
            List<String> titlevalue = new List<String>();

            fromList.Add(DateTime.Now.AddMonths(-3).ToString("dd/MM/yyyy"));
            fromList.Add(DateTime.Now.AddMonths(-2).ToString("dd/MM/yyyy"));
            fromList.Add(DateTime.Now.AddMonths(-1).ToString("dd/MM/yyyy"));

            datevalue.Add(DateTime.Now.AddMonths(-3).ToString("MMM/yyyy"));
            datevalue.Add(DateTime.Now.AddMonths(-2).ToString("MMM/yyyy"));
            datevalue.Add(DateTime.Now.AddMonths(-1).ToString("MMM/yyyy"));
            titlevalue = depList;
            ReportTransferDTO rto;
            List<ReportTransferDTO> Listone = new List<ReportTransferDTO>();
            for (int j = 0; j< fromList.Count; j++)
            {
                Debug.WriteLine(j);
                rto = new ReportTransferDTO();
                double temp=0;
                rto.timeValue = datevalue[j];
                xvalue = new List<double>();
                for (int i = 0; i <depList.Count; i++)
                {

                    temp = _disbursementRepo.GetDisAmountByDate(depList[i], catList, fromList[j]);
                    xvalue.Add(temp);
                }
                rto.xvalue = xvalue;
                Listone.Add(rto);
            }
            return Json(new { title = titlevalue, ListOne = Listone }, JsonRequestBehavior.AllowGet);

        }

    protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _poRepo.Dispose();
                _stockAdjustmentRepo.Dispose();
                _stationeryRepo.Dispose();
                _employeeRepo.Dispose();
                _departmentRepo.Dispose();
                _categoryRepo.Dispose();
                _supplierRepo.Dispose();
                _disbursementRepo.Dispose();
            }

            base.Dispose(disposing);
        }

        [Authorize(Roles = "manager,supervisor")]
        public ActionResult TrendAnalysis()
        {
            ViewBag.Departments = _departmentRepo.GetAll().Where(x => x.DeptCode != "STNR");
            ViewBag.Categories = _categoryRepo.GetAll();
            ViewBag.Suppliers = _supplierRepo.GetAll();

            return View();
        }
        public ActionResult ReturnJsonData()
        {
            //get post results from ajax
            Request.InputStream.Position = 0;
            var input = new StreamReader(Request.InputStream).ReadToEnd();
            AjaxInputFilter filter = new AjaxInputFilter(input);

            //get Dictionary of category id and names
            Dictionary<string, string> categoryDic = _categoryRepo.GetAll().ToDictionary(x => x.CategoryId.ToString(), x => x.CategoryName);
            Dictionary<string, string> groupDic;
            if (filter.IsDisburse)
                groupDic = _departmentRepo.GetAll().ToDictionary(x => x.DeptCode, x => x.DeptName);
            else
                groupDic = _supplierRepo.GetAll().ToDictionary(x => x.SupplierId.ToString(), x => x.SupplierName);

            //get data
            IEnumerable<Detail> allList;
            if (filter.IsDisburse)
                allList = _disbursementRepo.GetDisbursementDetailsByStatus("fulfilled")
                    .Select(x => new Detail
                    {
                        Date = x.Disbursement.CollectionDate,
                        Qty = x.ActualQty,
                        Cost = x.UnitPrice * x.ActualQty,
                        CategoryId = x.Stationery.CategoryId,
                        Group = x.Disbursement.DeptCode
                    }).Where(x => x.Date >= filter.FromDate && x.Date <= filter.ToDate);
            else
                allList = _poRepo.GetPurchaseOrderDetailsByStatus("fulfilled")
                    .Select(x => new Detail
                    {
                        Date = x.PurchaseOrder.CreateDate,
                        Qty = x.OrderQty,
                        Cost = x.UnitPrice * x.OrderQty,
                        CategoryId = x.Stationery.CategoryId,
                        Group = x.PurchaseOrder.SupplierId.ToString()
                    }).Where(x => x.Date >= filter.FromDate && x.Date <= filter.ToDate);
            var allList1 = allList;
            var allListTotal = allList;

            int noOfMonths = (filter.ToDate.Year - filter.FromDate.Year) * 12 + filter.ToDate.Month - filter.FromDate.Month + 1;
            string[] categories = filter.Categories.Split(',');
            if (filter.Categories == "")
                categories = categoryDic.Keys.ToArray();
            categories = allList.Select(x => x.CategoryId.ToString()).Distinct().Intersect(categories).ToArray();
            string[] groups = filter.Group.Split(',');
            if (filter.Group == "")
                groups = groupDic.Keys.ToArray();
            groups = allList.Select(x => x.Group).Distinct().Intersect(groups).ToArray();

            //get label
            object[] arr = new object[noOfMonths + 1];
            object[] arr1 = new object[noOfMonths + 1];
            object[] arrTotal = new object[noOfMonths + 1];
            string[] monthLabel = new string[groups.Length + 1];
            monthLabel[0] = "Month";
            for (int i = 0; i < groups.Length; i++)
                monthLabel[i + 1] = groupDic[groups[i]];
            arr[0] = monthLabel;
            string[] monthLabel1 = new string[categories.Length + 1];
            monthLabel1[0] = "Month";
            for (int i = 0; i < categories.Length; i++)
                monthLabel1[i + 1] = categoryDic[categories[i]];
            arr1[0] = monthLabel1;
            string[] monthLabelTotal = new string[2];
            monthLabelTotal[0] = "Month";
            monthLabelTotal[1] = filter.IsQty ? "Qty" : "Cost";
            arrTotal[0] = monthLabelTotal;

            allList = Detail.FilterByCategory(allList, categories).ToList();
            allList1 = Detail.FilterByGroup(allList, groups).ToList();
            for (int i = 0; i < noOfMonths; i++)
            {
                object[] rowArr = new object[groups.Length + 1];
                object[] rowArr1 = new object[categories.Length + 1];
                object[] rowArrTotal = new object[2];
                rowArr[0] = filter.FromDate.AddMonths(i).ToString("yyyyMM");
                rowArr1[0] = filter.FromDate.AddMonths(i).ToString("yyyyMM");
                rowArrTotal[0] = filter.FromDate.AddMonths(i).ToString("yyyyMM");
                for (int j = 0; j < groups.Length; j++)
                {
                    rowArr[j + 1] = Detail.FilterByYyymm(allList, rowArr[0] as string)
                        .Where(x => x.Group == groups[j]).Sum(x => filter.IsQty ? x.Qty : x.Cost);
                }
                for (int j = 0; j < categories.Length; j++)
                {
                    rowArr1[j + 1] = Detail.FilterByYyymm(allList1, rowArr1[0] as string)
                           .Where(x => x.CategoryId.ToString() == categories[j]).Sum(x => filter.IsQty ? x.Qty : x.Cost);
                }
                rowArrTotal[1] =
                    Detail.FilterByYyymm(
                    Detail.FilterByCategory(
                        Detail.FilterByGroup(allListTotal, groups), categories), rowArrTotal[0] as string)
                        .Sum(x => filter.IsQty ? x.Qty : x.Cost);
                arr[i + 1] = rowArr;
                arr1[i + 1] = rowArr1;
                arrTotal[i + 1] = rowArrTotal;
            }
            return Json(new { arr, arr1, arrTotal }, JsonRequestBehavior.AllowGet);
        }


        public class AjaxInputFilter
        {
            public AjaxInputFilter(string input)
            {
                string fromdate = new Regex(@"fromdate=([^ ]+), ").Match(input).Groups[0].Value;
                this.FromDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(Convert.ToInt64(fromdate.Substring(9, fromdate.Length - 11))).ToLocalTime();
                string todate = new Regex(@"todate=([^ ]+), ").Match(input).Groups[0].Value;
                this.ToDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(Convert.ToInt64(todate.Substring(7, todate.Length - 9))).ToLocalTime();
                string categories = new Regex(@"categories=([^ ]+), ").Match(input).Groups[0].Value;
                if (categories == "")
                    this.Categories = "";
                else
                    this.Categories = categories.Substring(11, categories.Length - 13);
                this.IsDisburse = new Regex(@"data_type=([^ ]+), ").Match(input).Groups[0].Value == "data_type=disburse, ";
                if (IsDisburse)
                {
                    string departments = new Regex(@"departments=([^ ]+) ").Match(input).Groups[0].Value;
                    if (departments == "departments=, ")
                        this.Group = "";
                    else
                        this.Group = departments.Substring(12, departments.Length - 14);
                }
                else
                {
                    string suppliers = new Regex(@"suppliers=([^ ]+) ").Match(input).Groups[0].Value;
                    if (suppliers == "suppliers=, ")
                        this.Group = "";
                    else
                        this.Group = suppliers.Substring(10, suppliers.Length - 12);
                }
                this.IsQty = new Regex(@"unit_type=([^ ]+)}").Match(input).Groups[0].Value == "unit_type=qty}";
            }
            public string Categories { get; set; }
            public string Group { get; set; }
            public bool IsDisburse { get; private set; }
            public bool IsQty { get; private set; }
            public DateTime FromDate { get; private set; }
            public DateTime ToDate { get; private set; }
        }

        public class Detail
        {
            public DateTime Date { get; set; }
            public int Qty { get; set; }
            public double Cost { get; set; }
            public string Group { get; set; }
            public int CategoryId { get; set; }
            public static IEnumerable<Detail> FilterByCategory(IEnumerable<Detail> list, string[] categories)
            {
                List<Detail> value = new List<Detail>();
                foreach (string catId in categories)
                {
                    value.AddRange(list.Where(x => x.CategoryId == Convert.ToInt32(catId)));
                }
                return value;
            }
            public static IEnumerable<Detail> FilterByGroup(IEnumerable<Detail> list, string[] groups)
            {
                List<Detail> value = new List<Detail>();
                foreach (string groupId in groups)
                {
                    value.AddRange(list.Where(x => x.Group == groupId));
                }
                return value;
            }
            public static IEnumerable<Detail> FilterByYyymm(IEnumerable<Detail> list, string yyyymm)
            {
                return list.Where(x => x.Date.ToString("yyyyMM") == yyyymm);
            }
        }

    }

}
