using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using LUSSIS.Controllers.WebAPI;

namespace LUSSIS
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "AuthApi",
                routeTemplate: "api/auth/{action}",
                defaults: new
                {
                    controller = "Account",
                    action = RouteParameter.Optional
                }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new
                {
                    controller="Stationeries",
                    id = RouteParameter.Optional
                }
            );
            
        }
    }
}
