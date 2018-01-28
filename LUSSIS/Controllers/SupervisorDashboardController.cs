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
        private PORepository _poRepo = new PORepository();
        private DisbursementRepository _disbursementRepo = new DisbursementRepository();
        private StockAdjustmentRepository _stockadjustmentRepo = new StockAdjustmentRepository();
        private StationeryRepository _stationeryRepo = new StationeryRepository();
        private EmployeeRepository _employeeRepo = new EmployeeRepository();
        private SupplierRepository _supplierRepo = new SupplierRepository();
        private readonly DepartmentRepository _departmentRepo = new DepartmentRepository();


        
        public async Task<ActionResult> Index()
        {
            var user = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            ViewBag.Message = user.GetRoles(System.Web.HttpContext.Current.User.Identity.GetUserId()).First().ToString();
            SupervisorDashboardDTO dash = new SupervisorDashboardDTO();

            dash.PendingPOTotalAmount = _poRepo.GetPendingPOTotalAmount();
            dash.PendingPOCount = _poRepo.GetPendingPOCount();
            dash.POTotalAmount = _poRepo.GetPOTotalAmount();
            dash.PendingStockAdjAddQty = _stockadjustmentRepo.GetPendingStockAddQty();
            dash.PendingStockAdjSubtractQty = _stockadjustmentRepo.GetPendingStockSubtractQty();
            dash.PendingStockAdjCount = _stockadjustmentRepo.GetPendingStockCount();
            dash.TotalDisbursementAmount = _disbursementRepo.GetDisbursementTotalAmount();

            return View(dash);
        }
     
        public JsonResult GetPiechartJSON(String List,String date,String e)
        {
            List<String> pileName = _stationeryRepo.GetAllCategoryName().ToList();
            List<double> pileValue = _poRepo.GetPOByCategory();


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


        [HttpGet]
        public async Task<ActionResult> GenerateReport(String supplier, String category, String flag)
        {
            SupervisorReportDTO model = new SupervisorReportDTO();
            ViewBag.flag = flag;
            if (supplier == "a" && category == "a")
            {
                List<Supplier> supplierList = _supplierRepo.GetAll().ToList<Supplier>();
                List<Category> categoryList = _stationeryRepo.GetAllCategoryList().ToList<Category>();
                model.Suppliers = supplierList;
                model.Categories = categoryList;


            }
            else if (supplier != "a")
            {
                ViewBag.second = "redirect";
                List<Supplier> supplierList2 = _supplierRepo.GetAll().ToList<Supplier>();
                List<Category> categoryList2 = _stationeryRepo.GetCategoryBySupplier(supplier);
                model.Suppliers = supplierList2;
                model.Categories = categoryList2;

            }
            else if(category!="a")
            {
                ViewBag.second = "redirect";
                List<Supplier> supplierList3 = _stationeryRepo.GetSupplierByCategory(category);
                model.Suppliers = supplierList3;
                model.Categories = _stationeryRepo.GetAllCategoryList();
            }
            return View(model);


        }


        public JsonResult GetReportJSON(String supplier_values, String category_values, String date)
        {
            List<String> pileName = _stationeryRepo.GetAllCategoryName().ToList();
            List<double> pileValue = _poRepo.GetPOByCategory();

            return Json(new { ListOne = pileName, ListTwo = pileValue }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GenerateStationeryTrendAnalysis(String supplier, String category, String flag)
        {
            SupervisorReportDTO model = new SupervisorReportDTO();
                ViewBag.flag = flag;
           
                List<Supplier> supplierList = _supplierRepo.GetAll().ToList<Supplier>();
                List<Category> categoryList = _stationeryRepo.GetAllCategoryList().ToList<Category>();
                model.Suppliers = supplierList;
                model.Categories = categoryList;


    
             if (supplier != "a")
            {
                ViewBag.selected = supplier;
                ViewBag.category= _stationeryRepo.GetCategoryBySupplier(supplier);
                ViewBag.supplier = null;

            }
            else if (category != "a")
            {
                ViewBag.selected = category;
                ViewBag.supplier = _stationeryRepo.GetSupplierByCategory(category);
                ViewBag.category = null;
            }
            return View(model);
        }
        public JsonResult GetStationeryReportJSON(String supplier_values, String category_values, String date)
        {
            List<String> xvalue = new List<String>();
            List<double> yvalue = new List<double>();
            String[] suppArray;
            String[] catArray;
            String fromDate = "2017-10-02";
            String toDate = "2018-10-02";
            if (supplier_values!=null && category_values!=null)
            {
                catArray = category_values.Split(',');
                suppArray = supplier_values.Split(',');
               
                List<String> supplierList = suppArray.ToList();
                List<String> categoryList = catArray.ToList();
                if (supplier_values.Length==1)
                {
                    xvalue = _stationeryRepo.GetCategoryNamebyId(categoryList);
                   yvalue=_poRepo.GetAmountByCategoryList(categoryList,supplier_values, fromDate, toDate);
                    
                }
                else if(category_values.Length==1)
                {
                    xvalue =_supplierRepo.GetSupplierNamebyId(supplierList);
                   
                    yvalue=_poRepo.GetAmountBySupplierList(supplierList,category_values,fromDate,toDate);
                   
                    
                }
               
                return Json(new { ListOne = xvalue, ListTwo = yvalue }, JsonRequestBehavior.AllowGet);

            }
            
          
            return Json(new { ListOne = xvalue, ListTwo = yvalue }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetDepartmentReportJSON(String depart, String cat, String date)
        {
            List<String> xvalue = new List<String>();
            List<double> yvalue = new List<double>();
            String[] departArray;
            String[] catArray;
            String fromDate = "2017-10-02";
            String toDate = "2018-10-02";
            if (depart != null && cat != null)
            {
                departArray = depart.Split(',');
                catArray= cat.Split(',');
                List<String> catList=catArray.ToList();
                List<String> depList = departArray.ToList();


                    if(depart.Equals("0"))
                    {
                    depList=_employeeRepo.GetAllDepartmentCode();
                    }
                    if (cat.Equals("0"))
                    {
                    foreach (int i in _stationeryRepo.GetAllCategoryIds())
                    {
                        catList.Add(Convert.ToString(i));
                    }

                    }
  
                
                if (depList.Capacity==1 && catList.Capacity>1)
                {
                    xvalue = _stationeryRepo.GetCategoryNamebyId(catList);

                    yvalue = _disbursementRepo.GetAmountByDepAndCatList(depart, catList, fromDate, toDate);
                }
                else if (depList.Capacity>1 && catList.Capacity==1)
                {
                    xvalue =depList;
                    
                   yvalue=_disbursementRepo.GetAmoutByCatAndDepList(cat,xvalue, fromDate,toDate);
                }
                else if(depList.Capacity>1 && catList.Capacity>1)
                {
                    xvalue = depList;
                
                    yvalue = _disbursementRepo.GetMaxCategoryAmountByDep(catList, depList, fromDate, toDate);
                }
                

                return Json(new { ListOne = xvalue, ListTwo = yvalue }, JsonRequestBehavior.AllowGet);

            }

            return Json(new { ListOne = xvalue, ListTwo = yvalue }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> GenerateDepartmentTrendAnalysis(String supplier, String category, String flag)
        {
            SupervisorReportDTO model = new SupervisorReportDTO();
            List<Department> departList = _employeeRepo.GetAllDepartment();
            List<Category> categoryList = _stationeryRepo.GetAllCategoryList().ToList<Category>();
            model.Department = departList;
            model.Categories = categoryList;
            return View(model);




        }
      


    }

}
