using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LUSSIS.Repositories;
using LUSSIS.Models;

namespace LUSSIS.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        EmployeeRepository employeeRepo = new EmployeeRepository();

        public ActionResult Index()
        {
            var user = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            ViewBag.Message = user.GetRoles(System.Web.HttpContext.Current.User.Identity.GetUserId()).First().ToString();

            switch (ViewBag.Message)
            {
                case "head":
                    return RedirectToAction("Index", "RepAndDelegate");
                case "rep":
                    return RedirectToAction("Index", "Collection");
                case "staff":
                    return RedirectToAction("Index", "Requisitions");
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