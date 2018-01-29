using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LUSSIS.Repositories;

namespace LUSSIS.Controllers
{
    //Authors: Ton That Minh Nhat, Ong Xin Ying
    [RequireHttps]
    public class HomeController : Controller
    {
        private readonly DelegateRepository _delegateRepo = new DelegateRepository();

        public ActionResult Index()
        {
            if (User.IsInRole("staff"))
            {
                var empNum = Convert.ToInt32(Request.Cookies["Employee"]?["EmpNum"]);
                var isDelegate = _delegateRepo.FindCurrentByEmpNum(empNum) != null;
                return RedirectToAction("Index",
                    isDelegate ? "RepAndDelegate" : "Requisitions");
            }

            if (User.IsInRole("rep"))
            {
                return RedirectToAction("Index", "Collection");
            }

            if (User.IsInRole("head"))
            {
                return RedirectToAction("Index", "RepAndDelegate");
            }

            if (User.IsInRole("clerk"))
            {
                return RedirectToAction("Consolidated", "Requisitions");
            }

            if (User.IsInRole("supervisor"))
            {
                return RedirectToAction("Index", "SupervisorDashboard");
            }

            if (User.IsInRole("manager"))
            {
                return RedirectToAction("Index", "SupervisorDashboard");
            }

            return View();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _delegateRepo.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}