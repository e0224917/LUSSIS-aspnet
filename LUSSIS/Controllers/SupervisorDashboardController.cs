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
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System.IO;
using System.Text.RegularExpressions;

namespace LUSSIS.Controllers
{
    [Authorize(Roles = "manager,supervisor")]
    public class SupervisorDashboardController : Controller
    {
        private PORepository pr = new PORepository();
        private DisbursementRepository disRepo = new DisbursementRepository();
        private StockAdjustmentRepository stockRepo = new StockAdjustmentRepository();
        private StationeryRepository sr = new StationeryRepository();
        private EmployeeRepository er = new EmployeeRepository();
        private SupplierRepository sur = new SupplierRepository();




        public async Task<ActionResult> Index()
        {
            var user = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            ViewBag.Message = user.GetRoles(System.Web.HttpContext.Current.User.Identity.GetUserId()).First().ToString();
            SupervisorDashboardDTO dash = new SupervisorDashboardDTO();

            dash.PendingPOTotalAmount = pr.GetPendingPOTotalAmount();
            dash.PendingPOCount = pr.GetPendingPOCount();
            dash.POTotalAmount = pr.GetPOTotalAmount();
            dash.PendingStockAdjAddQty = stockRepo.GetPendingStockAddQty();
            dash.PendingStockAdjSubtractQty = stockRepo.GetPendingStockSubtractQty();
            dash.PendingStockAdjCount = stockRepo.GetPendingStockCount();
            dash.TotalDisbursementAmount = disRepo.GetDisbursementTotalAmount();

            return View(dash);
        }
        /*  public ActionResult CharterColumn()
          {
              ArrayList xValue = new ArrayList();
              ArrayList yValue = new ArrayList();

              List<double> list = new List<double>();
                  List<Department> depList = er.GetDepartmentAll();

                  foreach (Department e in depList)
                  {
                  xValue.Add(e.DeptCode);
                  yValue.Add(disRepo.GetDisbursementByDepCode(e.DeptCode));

                  }
              new System.Web.Helpers.Chart(width: 600, height: 330, theme: ChartTheme.Blue)
                  .AddTitle("Chart for ")
                  .AddSeries("Default", chartType: "Column", xValue: xValue, yValues: yValue)
                  .Write("bmp");

              return null;
          }
          public ActionResult PieChartColumn()
          {
              ArrayList xValue = new ArrayList();
              ArrayList yValue = new ArrayList();

              List<double> list = new List<double>();
             // List<Category> cateList = sr.GetCategoryList();

             /* foreach (Category e in cateList)
              {
                  xValue.Add(e.CategoryName);
                  yValue.Add(pr.GetPOAmountByCategory(e.CategoryId));

              }
              new System.Web.Helpers.Chart(width: 600, height: 600, theme: ChartTheme.Blue)
                  .AddTitle("Chart for ")
                  .AddSeries("Default", chartType: "Pie", xValue: xValue, yValues: yValue)
                  .Write("bmp");

              return null;
          }*/
        public JsonResult GetPiechartJSON(String List, String date, String e)
        {
            List<String> pileName = sr.GetAllCategory().ToList();
            List<double> pileValue = pr.GetPOByCategory();

            return Json(new { ListOne = pileName, ListTwo = pileValue }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetBarchartJSON()
        {
            List<String> Name = er.GetDepartmentNames();
            List<double> Value = er.GetDepartmentValue();

            return Json(new { firstList = Name, secondList = Value }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult TrendAnalysis(bool? disburse)
        {
            ViewBag.disburse = disburse;
            if (disburse == null)
            {
                ViewBag.ChartType = new string[] { "bar chart","stacked bar chart","stack bar chart 2"};
                ViewBag.Suppliers = sur.GetAll().Select(x => new SelectListItem { Value = x.SupplierId.ToString(), Text = x.SupplierName }).ToList();
                ViewBag.Categories = sr.GetAllCategories().Select(x => new SelectListItem { Value = x.CategoryId.ToString(), Text = x.CategoryName }).ToList();
                ViewBag.Departments = er.GetDepartmentAll().Select(x => new SelectListItem { Value = x.DeptCode, Text = x.DeptName }).ToList();
            }
            return View();
        }

        [HttpPost]
        public ActionResult TrendAnalysisJSON()
        {
            //get filters
            Request.InputStream.Position = 0;
            var input = new StreamReader(Request.InputStream).ReadToEnd();
            string startdate = new Regex(@"startdate=([^ ]+), ").Match(input).Groups[0].Value;
            DateTime startDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(Convert.ToInt64(startdate.Substring(10, startdate.Length - 12))).ToLocalTime();
            string enddate = new Regex(@"enddate=([^ ]+), ").Match(input).Groups[0].Value;
            DateTime endDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(Convert.ToInt64(enddate.Substring(8, enddate.Length - 10))).ToLocalTime();
            string suppliers = new Regex(@"suppliers=([^ ]+), ").Match(input).Groups[0].Value;
            if (suppliers == "suppliers=null, ")
            {
                suppliers = "";
            }
            else
            {
                suppliers = suppliers.Substring(10, suppliers.Length - 12);
            }
            string departments = new Regex(@"departments=([^ ]+), ").Match(input).Groups[0].Value;
            if (departments == "departments=null, ")
            {
                departments = "";
            }
            else
            {
                departments = departments.Substring(12, departments.Length - 14);
            }
            string categories = new Regex(@"categories=([^ ]+), ").Match(input).Groups[0].Value;
            if (categories == "categories=null, ")
            {
                categories = "";
            }
            else
            {
                categories = categories.Substring(11, categories.Length - 13);
            }
            bool isdisburse = new Regex(@"isdisburse=([^ ]+), ").Match(input).Groups[0].Value == "isdisburse=True, ";
            bool iscost = new Regex(@"iscost=([^ ]+), ").Match(input).Groups[0].Value == "iscost=True, ";
            string charttype = new Regex(@"charttype=([^ ]+), ").Match(input).Groups[0].Value;

            //initialize data
            string[] labels;
            double[] data;
            string[] backgroundcolor;


            //get data
            double[] dataArr;
            string[] labelArr;
            
            if (isdisburse)
            {
                IEnumerable<DisbursementDetail> disburseData = disRepo.GetDisbursementDetailsByStatus("fulfilled")
                    .Where(x=>x.Disbursement.CollectionDate>=startDate && x.Disbursement.CollectionDate<=endDate);
                disburseData = FilterDisbursementDetailByDepartment(disburseData, departments);
                disburseData = FilterDisbursementDetailByCategory(disburseData, categories);
                var disburse = disburseData.GroupBy(x => x.Disbursement.CollectionDate.ToString("yyyy-MM"));
                if (iscost) {
                    var dataSetArr = disburse.Select(x => new { Key = x.Key, Cost = x.Sum(a => a.UnitPrice*a.ActualQty) }).OrderBy(x => x.Key);
                    dataArr = dataSetArr.Select(x => x.Cost).ToArray();
                    labelArr = dataSetArr.Select(x => x.Key).ToArray();
                }
                else
                {
                var dataSetArr=disburse.Select(x => new { Key = x.Key, Qty = x.Sum(a => a.ActualQty) }).OrderBy(x => x.Key);
                    dataArr = dataSetArr.Select(x => Convert.ToDouble(x.Qty)).ToArray();
                    labelArr = dataSetArr.Select(x => x.Key).ToArray();
                }                
            }
            else
            {
                IEnumerable<PurchaseOrderDetail> poData = pr.GetPurchaseOrderDetailsByStatus("fulfilled")
                    .Where(x => x.PurchaseOrder.CreateDate >= startDate && x.PurchaseOrder.CreateDate <= endDate);
                poData = FilterPurchaseOrderDetailBySupplier(poData, suppliers);
                poData = FilterPurchaseOrderDetailByCategory(poData, categories);
                var purchase = poData.GroupBy(x => x.PurchaseOrder.CreateDate.ToString("yyyy-MM"));
                if (iscost)
                {
                    var dataSetArr = purchase.Select(x => new { Key = x.Key, Cost = x.Sum(a => a.UnitPrice * a.OrderQty) }).OrderBy(x => x.Key);
                    dataArr = dataSetArr.Select(x => x.Cost).ToArray();
                    labelArr = dataSetArr.Select(x => x.Key).ToArray();
                }
                else
                {
                    var dataSetArr = purchase.Select(x => new { Key = x.Key, Qty = x.Sum(a => a.OrderQty) }).OrderBy(x => x.Key);
                    dataArr = dataSetArr.Select(x => Convert.ToDouble(x.Qty)).ToArray();
                    labelArr = dataSetArr.Select(x => x.Key).ToArray();
                }
            }
            return Json(new { Data = dataArr, Label = labelArr }, JsonRequestBehavior.AllowGet);
        }


        public List<DisbursementDetail> FilterDisbursementDetailByDepartment(IEnumerable<DisbursementDetail> data, string departments)
        {
            IEnumerable<DisbursementDetail> result = data.ToList();
            if (departments != "")
            {
                string[] deptArr = departments.Split(',');
                List<DisbursementDetail> newResult = new List<DisbursementDetail>();
                foreach (string deptCode in deptArr)
                {
                    newResult.AddRange(result.Where(x => x.Disbursement.DeptCode == deptCode));
                }
                return newResult;
            }
            return result.ToList();
        }

        public List<DisbursementDetail> FilterDisbursementDetailByCategory(IEnumerable<DisbursementDetail> data, string categories)
        {
            IEnumerable<DisbursementDetail> result = data.ToList();
            if (categories != "")
            {
                string[] catArr = categories.Split(',');
                List<DisbursementDetail> newResult = new List<DisbursementDetail>();
                foreach (string catId in catArr)
                {
                    newResult.AddRange(result.Where(x => x.Stationery.CategoryId == Convert.ToInt32(catId)));
                }
                return newResult;
            }
            return result.ToList();
        }
        public List<PurchaseOrderDetail> FilterPurchaseOrderDetailBySupplier(IEnumerable<PurchaseOrderDetail> data, string departments)
        {
            IEnumerable<PurchaseOrderDetail> result = data.ToList();
            if (departments != "")
            {
                string[] deptArr = departments.Split(',');
                List<PurchaseOrderDetail> newResult = new List<PurchaseOrderDetail>();
                foreach (string supplierId in deptArr)
                {
                    newResult.AddRange(result.Where(x => x.PurchaseOrder.SupplierId == Convert.ToInt32(supplierId)));
                }
                return newResult;
            }
            return result.ToList();
        }

        public List<PurchaseOrderDetail> FilterPurchaseOrderDetailByCategory(IEnumerable<PurchaseOrderDetail> data, string categories)
        {
            IEnumerable<PurchaseOrderDetail> result = data.ToList();
            if (categories != "")
            {
                string[] catArr = categories.Split(',');
                List<PurchaseOrderDetail> newResult = new List<PurchaseOrderDetail>();
                foreach (string catId in catArr)
                {
                    newResult.AddRange(result.Where(x => x.Stationery.CategoryId == Convert.ToInt32(catId)));
                }
                return newResult;
            }
            return result.ToList();
        }

    }
    public class TrendAnalysisDTO
    {
        public List<string> Suppliers { get; set; }
        public List<string> Categories { get; set; }
        public List<string> Departments { get; set; }
        public string DatePeriod { get; set; }
        public bool IsDisburse { get; set; }
        public bool IsCost { get; set; }
    }
}
