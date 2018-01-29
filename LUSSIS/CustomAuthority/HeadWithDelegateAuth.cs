using LUSSIS.Repositories;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace LUSSIS.CustomAuthority
{
    //Authors: Koh Meng Guan, Ong Xin Ying
    public class HeadWithDelegateAuthAttribute : AuthorizeAttribute
    {
        private readonly DelegateRepository _delegateRepo = new DelegateRepository();

        private readonly string[] allowedRoles;

        public HeadWithDelegateAuthAttribute(params string[] roles)
        {
            allowedRoles = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var email = httpContext.User.Identity.Name;
            var deptCode = httpContext.Request.Cookies["Employee"]?["DeptCode"];
            var isDelegate = _delegateRepo.FindCurrentByEmail(email) != null;
            var hasDelegate = _delegateRepo.FindCurrentByDeptCode(deptCode) != null;

            if (httpContext.User.IsInRole("head") && !hasDelegate
                || httpContext.User.IsInRole("staff") && isDelegate)
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
                    new RouteValueDictionary(new {controller = "Account", action = "Login"})
                );
            }
            //User is logged in but has no access
            else
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new {controller = "Account", action = "NotAuthorized"})
                );
            }
        }
    }
}