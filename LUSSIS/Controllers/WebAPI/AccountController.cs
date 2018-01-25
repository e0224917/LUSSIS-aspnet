using LUSSIS.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using System.Web.Http.Description;
using LUSSIS.Emails;
using LUSSIS.Models.WebAPI;

namespace LUSSIS.Controllers.WebAPI
{
    public class AccountController : ApiController
    {
        private LUSSISContext db = new LUSSISContext();

        [HttpGet]
        [AllowAnonymous]
        [ResponseType(typeof(string))]
        public string TestAuth()
        {
            return "Ok";
        }

        [HttpPost]
        [AllowAnonymous]
        [ResponseType(typeof(EmployeeDTO))]
        public async Task<IHttpActionResult> Login(LoginViewModel model)
        {
            try
            {
                var manager = HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>();
                var result = await manager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe,
                    shouldLockout: false);

                if (result != SignInStatus.Success) return BadRequest("Wrong email or password. Please try again.");

                var emp = await db.Employees.FirstOrDefaultAsync(em => em.EmailAddress == model.Email);
                int num = emp.EmpNum;
                var delegateEmp = db.Delegates.AsEnumerable().LastOrDefault(d => d.EmpNum == num);

                bool isDelegated = false;
                if (delegateEmp != null)
                    isDelegated = DateTime.Today >= delegateEmp.StartDate && DateTime.Today <= delegateEmp.EndDate;

                var e = new EmployeeDTO
                {
                    EmpNum = emp.EmpNum,
                    Title = emp.Title,
                    FirstName = emp.FirstName,
                    LastName = emp.LastName,
                    EmailAddress = emp.EmailAddress,
                    JobTitle = emp.JobTitle,
                    DeptCode = emp.DeptCode,
                    DeptName = emp.Department.DeptName,
                    IsDelegated = isDelegated
                };
                return Ok(e);
            }
            catch (Exception e)
            {
                return Ok(e);
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userManager = HttpContext.Current.GetOwinContext().Get<ApplicationUserManager>();
                    var user = await userManager.FindByNameAsync(model.Email);
                    if (user == null)
                    {
                        return BadRequest("No email exists in the database.");
                    }

                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    string code = await userManager.GeneratePasswordResetTokenAsync(user.Id);
                    var callbackUrl = Url.Link("Default",
                        new { controller = "Account", action = "ResetPassword", userId = user.Id, code = code });
                    // await userManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    string subject = "Reset password for " + model.Email;
                    string body = "Please reset your password by clicking <a href=" + callbackUrl + ">here</a>";
                    string to = "minhnhattonthat@gmail.com";
                    EmailHelper.SendEmail(to, subject, body);
                }
                catch (Exception e)
                {
                    return Ok(e);
                }

                return Ok(new { Message = "Reset link sent to your email." });
            }

            return BadRequest("Something is wrong. Please try again.");
        }
    }
}
