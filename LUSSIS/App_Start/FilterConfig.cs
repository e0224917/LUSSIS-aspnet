using System.Web;
using System.Web.Http.Filters;
using System.Web.Mvc;
using LUSSIS.Filters;

namespace LUSSIS
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new AuthorizeAttribute());
        }

        public static void RegisterApiFilters(HttpFilterCollection filters)
        {
            //filters.Add(new ValidateModelAttribute());
        }
    }
}
