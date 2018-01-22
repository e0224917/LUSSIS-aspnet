using System;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json.Converters;


namespace LUSSIS
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            IsoDateTimeConverter converter = new IsoDateTimeConverter
            {
                DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'",
                Culture = CultureInfo.InvariantCulture
            };
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(converter);

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
