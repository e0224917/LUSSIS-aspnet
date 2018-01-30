using LUSSIS.DAL;
using LUSSIS.Models;
using LUSSIS.Repositories;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using LUSSIS.Constants;

namespace LUSSIS.CustomAuthority
{
    //Authors: Koh Meng Guan, Ong Xin Ying
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly DelegateRepository _delegateRepo = new DelegateRepository();

        private readonly string[] allowedRoles;

        public CustomAuthorizeAttribute(params string[] roles)
        {
            this.allowedRoles = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {     
            var email = httpContext.User.Identity.Name;
            var isDelegate = _delegateRepo.FindCurrentByEmail(email) != null;

            if (httpContext.User.IsInRole(Role.DepartmentHead) || httpContext.User.IsInRole(Role.Staff) && isDelegate)
            {
                    return true;
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