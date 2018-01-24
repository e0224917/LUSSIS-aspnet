﻿using System;
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
            var user = System.Web.HttpContext.Current.User.Identity.GetUserId();
            var userManager = httpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var roles = userManager.GetRoles(user);
            if (roles.Contains("staff"))
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
