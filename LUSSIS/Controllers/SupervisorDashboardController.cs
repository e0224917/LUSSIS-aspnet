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
        private readonly DepartmentRepository _departmentRepo = new DepartmentRepository();


        
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
     
        public JsonResult GetPiechartJSON(String List,String date,String e)
        {
            List<String> pileName = sr.GetAllCategoryName().ToList();
            List<double> pileValue = pr.GetPOByCategory();


            return Json(new { ListOne = pileName, ListTwo = pileValue }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBarchartJson()
        {
            var deptNames = _departmentRepo.GetAll().Select(item => item.DeptName).ToList();
            var deptCodes = _departmentRepo.GetAll().Select(item => item.DeptCode).ToList();
            
            var deptValues = new List<double>();
            foreach (var deptCode in deptCodes)
            {
                deptValues.Add(disRepo.GetDisbursementTotalAmountOfDept(deptCode));
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
                List<Supplier> supplierList = sur.GetAll().ToList<Supplier>();
                List<Category> categoryList = sr.GetAllCategoryList().ToList<Category>();
                model.Suppliers = supplierList;
                model.Categories = categoryList;


            }
            else if (supplier != "a")
            {
                ViewBag.second = "redirect";
                List<Supplier> supplierList2 = sur.GetAll().ToList<Supplier>();
                List<Category> categoryList2 = sr.GetCategoryBySupplier(supplier);
                model.Suppliers = supplierList2;
                model.Categories = categoryList2;

            }
            else if(category!="a")
            {
                ViewBag.second = "redirect";
                List<Supplier> supplierList3 = sr.GetSupplierByCategory(category);
                model.Suppliers = supplierList3;
                model.Categories = sr.GetAllCategoryList();
            }
            return View(model);


        }


        public JsonResult GetReportJSON(String supplier_values, String category_values, String date)
        {
            List<String> pileName = sr.GetAllCategoryName().ToList();
            List<double> pileValue = pr.GetPOByCategory();

            return Json(new { ListOne = pileName, ListTwo = pileValue }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GenerateStationeryTrendAnalysis(String supplier, String category, String flag)
        {
            SupervisorReportDTO model = new SupervisorReportDTO();
                ViewBag.flag = flag;
           
                List<Supplier> supplierList = sur.GetAll().ToList<Supplier>();
                List<Category> categoryList = sr.GetAllCategoryList().ToList<Category>();
                model.Suppliers = supplierList;
                model.Categories = categoryList;


    
             if (supplier != "a")
            {
                ViewBag.selected = supplier;
                ViewBag.category= sr.GetCategoryBySupplier(supplier);
                ViewBag.supplier = null;

            }
            else if (category != "a")
            {
                ViewBag.selected = category;
                ViewBag.supplier = sr.GetSupplierByCategory(category);
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
                    xvalue = sr.GetCategoryNamebyId(categoryList);
                   yvalue=pr.GetAmountByCategoryList(categoryList,supplier_values, fromDate, toDate);
                    
                }
                else if(category_values.Length==1)
                {
                    xvalue =sur.GetSupplierNamebyId(supplierList);
                   
                    yvalue=pr.GetAmountBySupplierList(supplierList,category_values,fromDate,toDate);
                   
                    
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
                    depList=er.GetAllDepartmentCode();
                    }
                    if (cat.Equals("0"))
                    {
                    foreach (int i in sr.GetAllCategoryIds())
                    {
                        catList.Add(Convert.ToString(i));
                    }

                    }
  
                
                if (depList.Capacity==1 && catList.Capacity>1)
                {
                    xvalue = sr.GetCategoryNamebyId(catList);

                    yvalue = disRepo.GetAmountByDepAndCatList(depart, catList, fromDate, toDate);
                }
                else if (depList.Capacity>1 && catList.Capacity==1)
                {
                    xvalue =depList;
                    
                   yvalue=disRepo.GetAmoutByCatAndDepList(cat,xvalue, fromDate,toDate);
                }
                else if(depList.Capacity>1 && catList.Capacity>1)
                {
                    xvalue = depList;
                
                    yvalue = disRepo.GetMaxCategoryAmountByDep(catList, depList, fromDate, toDate);
                }
                

                return Json(new { ListOne = xvalue, ListTwo = yvalue }, JsonRequestBehavior.AllowGet);

            }

            return Json(new { ListOne = xvalue, ListTwo = yvalue }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> GenerateDepartmentTrendAnalysis(String supplier, String category, String flag)
        {
            SupervisorReportDTO model = new SupervisorReportDTO();
            List<Department> departList = er.GetAllDepartment();
            List<Category> categoryList = sr.GetAllCategoryList().ToList<Category>();
            model.Department = departList;
            model.Categories = categoryList;
            return View(model);




        }
      


    }

}
