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
                List<SelectListItem> datePeriod = new List<SelectListItem>() {
                    new SelectListItem() { Value="Last Month",Text="Last Month",Selected=true},
                    new SelectListItem() { Value="Past 3 Months",Text="Past 3 Months"},
                    new SelectListItem() { Value="Past 6 Months",Text="Past 6 Months"},
                    new SelectListItem() { Value="Past 12 Months",Text="Past 12 Months"},
                };
                ViewBag.DatePeriod = new List<string>() { "Last Month", "Past 3 Months", "Past 6 Months", "Past 12 Months" };
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
            string suppliers= new Regex(@"suppliers=([^ ]+), ").Match(input).Groups[0].Value;
            if (suppliers == "suppliers=null, ")
            {
                suppliers = "";
            } else
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
            bool isdisburse = new Regex(@"isdisburse=([^ ]+), ").Match(input).Groups[0].Value=="isdisburse=True, ";
            bool iscost = new Regex(@"isdisburse=([^ ]+), ").Match(input).Groups[0].Value == "iscost=True}";


            var disburse = disRepo.GetDisbursementDetailsByStatus("fulfilled")
                .GroupBy(x => x.Disbursement.DeptCode)
                .Select(x => new {DeptCode=x.Key, Qty=x.Sum(a=>a.ActualQty) })
                .OrderByDescending(x=>x.Qty);


            int[] dataArr = disburse.Select(x => x.Qty).ToArray();
            string[] labelArr = disburse.Select(x=>x.DeptCode).ToArray();

            return Json(new {Data = dataArr, Label = labelArr }, JsonRequestBehavior.AllowGet);
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
