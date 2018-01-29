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
using System.Diagnostics;

namespace LUSSIS.Controllers
{
    [Authorize(Roles = "manager,supervisor")]
    public class SupervisorDashboardController : Controller
    {
        private readonly PORepository _poRepo = new PORepository();
        private readonly DisbursementRepository _disbursementRepo = new DisbursementRepository();
        private readonly StockAdjustmentRepository _stockAdjustmentRepo = new StockAdjustmentRepository();
        private StationeryRepository _stationeryRepo = new StationeryRepository();
        private EmployeeRepository _employeeRepo = new EmployeeRepository();
        private readonly SupplierRepository _supplierRepo = new SupplierRepository();
        private readonly CategoryRepository _categoryRepo = new CategoryRepository();
        private readonly DepartmentRepository _departmentRepo = new DepartmentRepository();

        public ActionResult Index()
        {
            ViewBag.Message = User.IsInRole("supervisor") ? "supervisor" : "manager";
            var totalAddAdjustmentQty = _stockAdjustmentRepo.GetPendingAdjustmentByType("add").Count;
            var totalSubtractAdjustmentQty = _stockAdjustmentRepo.GetPendingAdjustmentByType("subtract").Count;

            var dash = new SupervisorDashboardDTO
            {
                PendingPOTotalAmount = _poRepo.GetPendingPOTotalAmount(),
                PendingPOCount = _poRepo.GetPendingPOCount(),
                POTotalAmount = _poRepo.GetPOTotalAmount(),
                PendingStockAdjAddQty = totalAddAdjustmentQty,
                PendingStockAdjSubtractQty = totalSubtractAdjustmentQty,
                PendingStockAdjCount = _stockAdjustmentRepo.GetPendingAdjustmentList().Count,
                TotalDisbursementAmount = _disbursementRepo.GetDisbursementTotalAmount()
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
            /*List<String> pileName = _categoryRepo.GetAllCategoryName().ToList();
            List<double> pileValue = _poRepo.GetPOByCategory();
            return Json(new { ListOne = pileName, ListTwo = pileValue }, JsonRequestBehavior.AllowGet);*/

            List<String> depList = _departmentRepo.GetAllDepartmentCode();
            List<int> catList = _categoryRepo.GetAllCategoryIds();

            List<ReportTransferDTO> Listone = new List<ReportTransferDTO>();
           
            List<String> fromList = new List<String>();
            List<String> toList = new List<String>();

           
            List<String> datevalue = new List<String>();

            List<double> xvalue = new List<double>();
            List<String> titlevalue = new List<String>();
          
                double temp = 0;
                ReportTransferDTO rto = new ReportTransferDTO();
                    datevalue.Add("2017 November");
                    datevalue.Add("2017 December");
                    datevalue.Add("2018 January");

                    fromList.Add("2017-11-01");
                    fromList.Add("2017-12-01");
                    fromList.Add("2018-01-01");

                    toList.Add("2017-11-30");
                    toList.Add("2017-12-31");
                    toList.Add("2018-01-31");

                    titlevalue = depList;

                    for (int j = 0; j < datevalue.Count; j++)
                    {
                        rto = new ReportTransferDTO();
                        xvalue = new List<double>();
                      
                        rto.timeValue = datevalue[j];
                        
                        for (int i = 0; i < depList.Count; i++)
                        {
                             Debug.WriteLine(catList[i]);
                              Debug.WriteLine(depList[i]);
                           
                            temp = _disbursementRepo.GetDisAmountByDate(depList[i],catList,fromList[j],toList[j]);
                            xvalue.Add(temp);
                           
                         }
                         rto.xvalue = xvalue;
                         Listone.Add(rto);
                    }
                       
                

                return Json(new { Title = titlevalue, ListOne = Listone }, JsonRequestBehavior.AllowGet);

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

    }

}
