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
        }
    }
}
