using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Http;
using LUSSIS.Models.WebDTO;

namespace LUSSIS
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterApiFilters(GlobalConfiguration.Configuration.Filters);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
            //GlobalConfiguration.Configuration.Formatters.JsonFormatter.S‌​erializerSettings.Re‌​ferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        }
        void Session_Start(object sender, EventArgs e)
        {
            ShoppingCart shoppingCart = new ShoppingCart();
            Session["MyCart"] = shoppingCart;
            Session["Name"] = "";
            Session["Roles"] = new List<string>();
        }
    }
}
