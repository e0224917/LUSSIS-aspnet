using LUSSIS.Models;
using System;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using System.Web.Http.Description;
using LUSSIS.Emails;
using LUSSIS.Models.WebAPI;
using LUSSIS.Repositories;

namespace LUSSIS.Controllers.WebAPI
{
    //Authors: Ton That Minh Nhat
    public class AccountController : ApiController
    {
        private readonly EmployeeRepository _employeeRepo = new EmployeeRepository();
        private readonly DelegateRepository _delegateRepo = new DelegateRepository();

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

                var emp = _employeeRepo.GetEmployeeByEmail(model.Email);

                var isDelegated = false;

                if (emp.JobTitle.Equals("staff"))
                {
                    isDelegated = _delegateRepo.FindCurrentByEmpNum(emp.EmpNum) != null;
                }

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
                return BadRequest(e.Message);
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
                        new { controller = "Account", action = "ResetPassword", userId = user.Id, code });
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _employeeRepo.Dispose();
                _delegateRepo.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
