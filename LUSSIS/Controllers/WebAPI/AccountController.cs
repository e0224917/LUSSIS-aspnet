using LUSSIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using System.Web.Http.Description;
using LUSSIS.Models.WebAPI;

namespace LUSSIS.Controllers.WebAPI
{
    public class AccountController : ApiController
    {
        private LUSSISContext db = new LUSSISContext();

        [HttpPost]
        [ResponseType(typeof(EmployeeDTO))]
        public async Task<IHttpActionResult> Login(LoginViewModel model)
        {
            string email = model.Email;
            string pass = model.Password;
            var manager = HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>();
            var result = await manager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            if(result == SignInStatus.Success)
            {
                var emp = db.Employees.Where(em => em.EmailAddress == email).First<Employee>();
                var e = new EmployeeDTO
                {
                    EmpNum = emp.EmpNum,
                    Title = emp.Title,
                    FirstName = emp.FirstName,
                    LastName = emp.LastName,
                    EmailAddress = emp.EmailAddress,
                    JobTitle = emp.JobTitle,
                    DeptCode = emp.DeptCode
                };
                return Ok(e);
            }
            return BadRequest();
        }
    }
}
