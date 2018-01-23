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

namespace LUSSIS.Controllers
{
    public class SupervisorDashboardController : Controller
    {
        private PORepository pr = new PORepository();
        private DisbursementRepository disRepo = new DisbursementRepository();
        private StockAdjustmentRepository stockRepo = new StockAdjustmentRepository();
        private StationeryRepository sr = new StationeryRepository();
        private EmployeeRepository er = new EmployeeRepository();
        private SupplierRepository sur = new SupplierRepository();



        [Authorize(Roles = "manager,supervisor")]
        public async Task<ActionResult> SupervisorDashboard()
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
        public JsonResult GetPiechartJSON()
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

    }
}
