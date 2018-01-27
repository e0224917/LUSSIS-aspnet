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

        public JsonResult GetPiechartJSON(String List, String date, String e)
        {
            List<String> pileName = sr.GetAllCategoryName().ToList();
            List<double> pileValue = pr.GetPOByCategory();

            return Json(new { ListOne = pileName, ListTwo = pileValue }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetBarchartJSON()
        {
            List<String> Name = er.GetDepartmentNames();
            List<double> Value = er.GetDepartmentValue();

            return Json(new { firstList = Name, secondList = Value }, JsonRequestBehavior.AllowGet);
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
                ViewBag.category = sr.GetCategoryBySupplier(supplier);
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
        public JsonResult GetStationeryReportJSON(String supplier,String category, String from,String to)
        {
            List<String> xvalue = new List<String>();
            List<double> yvalue = new List<double>();
            String[] suppArray;
            String[] catArray;
           
            if (supplier != null && category != null)
            {
                catArray = category.Split(',');
                suppArray = supplier.Split(',');

                List<String> supplierList = suppArray.ToList();
                List<String> categoryList = catArray.ToList();
                if (supplier.Length == 1)
                {
                    xvalue = sr.GetCategoryNamebyId(categoryList);
                    yvalue = pr.GetAmountByCategoryList(categoryList, supplier, from, to);

                }
                else if (category.Length == 1)
                {
                    xvalue = sur.GetSupplierNamebyId(supplierList);

                    yvalue = pr.GetAmountBySupplierList(supplierList, category, from, to);


                }

                return Json(new { ListOne = xvalue, ListTwo = yvalue }, JsonRequestBehavior.AllowGet);

            }


            return Json(new { ListOne = xvalue, ListTwo = yvalue }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetDepartmentReportJSON(String depart, String cat, String to, String from)
        {
            List<String> catList = new List<String>();
            List<String> depList = new List<String>();
            List<String> xvalue = new List<String>();
            List<double> yvalue = new List<double>();
            String[] departArray;
            String[] catArray;
            
            if (depart != null && cat != null)
            {
                departArray = depart.Split(',');
                catArray = cat.Split(',');
                 catList = catArray.ToList();
                 depList = departArray.ToList();
               
                if (depart.Equals("0"))
                {
                    depList = er.GetAllDepartmentCode();
                }
                if (cat=="0")
                {
                    List<String> tmpList = new List<String>();
                    foreach (int i in sr.GetAllCategoryIds())
                    {
                        tmpList.Add(Convert.ToString(i));
                    }
                    catList = tmpList;

                }
                if (depList.Capacity == 1 && catList.Capacity > 1)
                {
                    xvalue = sr.GetCategoryNamebyId(catList);

                    yvalue = disRepo.GetAmountByDepAndCatList(depart, catList, from, to);
                }
                else if (depList.Capacity > 1 && catList.Capacity == 1)
                {
                    xvalue = depList;

                    yvalue = disRepo.GetAmoutByCatAndDepList(cat, xvalue, from, to);
                }
                else if (depList.Capacity > 1 && catList.Capacity > 1)
                {
                    xvalue = depList;

                    yvalue = disRepo.GetMaxCategoryAmountByDep(catList, depList, from, to);
                }


                return Json(new { ListOne = xvalue, ListTwo = yvalue }, JsonRequestBehavior.AllowGet);

            }

            return Json(new { ListOne = xvalue, ListTwo = yvalue }, JsonRequestBehavior.AllowGet);
        }
       /* public JsonResult GetDepartmentStackJSON(String depart, String cat, String from, String to, String stack)
        {
            List<ReportTransferDTO> Listone = new List<ReportTransferDTO>();

            List<String> fromList = new List<String>();
            List<String> toList = new List<String>();
            List<String> dateList = new List<String>();
            List<String> datevalue = new List<String>();
            List<double> xvalue = new List<double>();
            List<String> titlevalue = new List<String>();
            String[] departArray;
            String[] catArray;
            String fromDate = "2017-10-02";
            String toDate = "2018-10-02";
            String[] fromArray = fromDate.Split('-');
            String[] toArray = toDate.Split('-');
            int[] start = new int[3];
            int[] end = new int[3];
            for (int i = 0; i < start.Length; i++)
            {
                start[i] = Convert.ToInt32(fromArray[i]);
                end[i] = Convert.ToInt32(toArray[i]);
            }
            if (start[0] - end[0] > 1)
            {
                for (int i = start[0]; i <end[0]; i++)
                {
                    dateList.Add("" + start[0]);
                    fromList.Add(String.Format("{0}-{1}-{2}", start[i], start[1], start[2]));

                }
            }
            else if (start[1] - end[1] > 1)
            {
                for (int i = start[1]; i <end[1]; i++)
                {
                    dateList.Add("" + start[1]);
                    fromList.Add(String.Format("{0}-{1}-{2}", start[0], start[i], start[2]));
                }
            }
            else
            {
                for (int i = start[2]; i <end[2]; i++)
                {
                    dateList.Add("" + start[2]);
                    fromList.Add(String.Format("{0}-{1}-{2}", start[0], start[1], start[i]));
                }
            }

            if (depart != null && cat != null)
            {
                departArray = depart.Split(',');
                catArray = cat.Split(',');
                List<String> catList = catArray.ToList();
                List<String> depList = departArray.ToList();


                if (depart.Equals("0"))
                {
                    depList = er.GetAllDepartmentCode();
                }
                if (cat.Equals("0"))
                {
                    List<String> tmpList = new List<String>();
                    foreach (int i in sr.GetAllCategoryIds())
                    {
                        tmpList.Add(Convert.ToString(i));
                    }
                    catList = tmpList;
                }


                ReportTransferDTO rto = new ReportTransferDTO();
                if (depList.Capacity == 1 && catList.Capacity > 1)
                {
                    titlevalue = sr.GetCategoryNamebyId(catList);
                    for (int j = 0; j < dateList.Capacity; j++)
                    {
                        rto = new ReportTransferDTO();
                        
                        rto.timeValue = dateList[j];
                        for (int i = 0; i < fromList.Capacity - 1; i++)
                        {

                            List<double> temp = disRepo.GetAmountByDepAndCatList(depart, catList, fromList[i], fromList[i + 1]);
                            rto.xvalue = temp;
                        }
                        Listone.Add(rto);

                    }

                }
                else if (depList.Capacity > 1 && catList.Capacity == 1)
                {
                    datevalue = dateList;
                    titlevalue = depList;

                    for (int j = 0; j < dateList.Capacity; j++)
                    {
                        rto = new ReportTransferDTO();
                        rto.timeValue = dateList[j];
                        for (int i = 0; i < dateList.Capacity - 1; i++)
                        {

                            List<double> temp = disRepo.GetAmoutByCatAndDepList(cat, depList, fromList[i], toList[i + 1]);
                            rto.xvalue = temp;
                        }
                        Listone.Add(rto);
                    }

                }
                else if (depList.Capacity > 1 && catList.Capacity > 1)
                {
                    datevalue = dateList;
                    titlevalue = depList;

                    for (int j = 0; j < dateList.Capacity; j++)
                    {
                        rto = new ReportTransferDTO();
                       
                        rto.timeValue = dateList[j];
                        for (int i = 0; i < fromList.Capacity - 1; i++)
                        {

                            List<double> temp = disRepo.GetMaxCategoryAmountByDep(catList, depList, fromList[i], toList[i + 1]);
                            rto.xvalue = temp;
                        }
                        Listone.Add(rto);
                    }

                }


                return Json(new { Title=titlevalue,ListOne = Listone }, JsonRequestBehavior.AllowGet);

            }

            return Json(new { Title=titlevalue,ListOne = Listone }, JsonRequestBehavior.AllowGet);
        }*/
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
