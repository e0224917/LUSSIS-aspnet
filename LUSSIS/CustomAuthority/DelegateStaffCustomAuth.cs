using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using LUSSIS.Repositories;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace LUSSIS.CustomAuthority
{
    //Authors: Koh Meng Guan, Ong Xin Ying
    public class DelegateStaffCustomAuthAttribute : AuthorizeAttribute
    {
        private readonly DelegateRepository _delegateRepo = new DelegateRepository();

        private readonly string[] allowedRoles;

        public DelegateStaffCustomAuthAttribute(params string[] roles)
        {
            this.allowedRoles = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext.User.IsInRole(Role.Staff) || httpContext.User.IsInRole(Role.Representative))
            {
                var email = httpContext.User.Identity.Name;
                var isDelegate = _delegateRepo.FindCurrentByEmail(email) != null;
                return !isDelegate;
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
