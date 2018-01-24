using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using LUSSIS.Repositories;

namespace LUSSIS.CustomAuthority
{
    public class DelegateStaffCustomAuthAttribute : AuthorizeAttribute
    {
        
        EmployeeRepository empRepo = new EmployeeRepository();

        private readonly string[] allowedRoles;

        public DelegateStaffCustomAuthAttribute(params string[] roles)
        {
            this.allowedRoles = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var user = httpContext.User;

            if (user.IsInRole("staff"))
            {
                if(!empRepo.CheckIfLoggedInUserIsDelegate())
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
           
            return false;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(new { controller = "Account", action = "Login" })
                );
            }
            //User is logged in but has no access
            else
            {
                filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(new { controller = "Account", action = "NotAuthorized" })
                );
            }
        }
    }
}
