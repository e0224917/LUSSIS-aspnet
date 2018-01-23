using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LUSSIS.Repositories;
using LUSSIS.Models;
using LUSSIS.Controllers;

namespace LUSSIS.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {

        EmployeeRepository employeeRepo = new EmployeeRepository();

        public ActionResult Index()
        {
            //var user = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            //ViewBag.Message = user.GetRoles(System.Web.HttpContext.Current.User.Identity.GetUserId()).First().ToString();
            string justARole = employeeRepo.GetCurrentUser().JobTitle;
            switch (justARole)
            {
                case "rep":
                    return RedirectToAction("Index", "Collection");
                case "head":
                    return RedirectToAction("Index", "RepAndDelegate");   
                case "staff":
                    if (employeeRepo.GetDelegateByDate(employeeRepo.GetCurrentUser().Department, DateTime.Now.Date) != null)
                    {
                        if (employeeRepo.GetCurrentUser().EmpNum == employeeRepo.GetDelegateByDate(employeeRepo.GetCurrentUser().Department, DateTime.Now.Date).EmpNum)
                        {
                            return RedirectToAction("Index", "RepAndDelegate");
                        }
                        else
                        {
                            return RedirectToAction("Index", "Requisitions");
                        }
                    }
                    else
                    {
                        return RedirectToAction("Index", "Requisitions");
                    }
                case "manager":
                    return RedirectToAction("SupervisorDashboard", "PurchaseOrders");
                case "supervisor":
                    return RedirectToAction("SupervisorDashboard", "PurchaseOrders");
                case "clerk":
                    return RedirectToAction("Consolidated", "Requisitions");
            }

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}