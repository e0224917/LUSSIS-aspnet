using System;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using System.Web.Http.Description;
using LUSSIS.Constants;
using LUSSIS.Emails;
using LUSSIS.Models.Account;
using LUSSIS.Models.WebAPI;
using LUSSIS.Repositories;

namespace LUSSIS.Controllers.WebAPI
{
    //Authors: Ton That Minh Nhat
    public class AccountController : ApiController
    {
        private readonly EmployeeRepository _employeeRepo = new EmployeeRepository();
        private readonly DelegateRepository _delegateRepo = new DelegateRepository();

        [HttpPost]
        [AllowAnonymous]
        [Route("api/auth/Login")]
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

                if (emp.JobTitle.Equals(Role.Staff))
                {
                    isDelegated = _delegateRepo.FindCurrentByEmpNum(emp.EmpNum) != null;
                }

                var e = new EmployeeDTO(emp)
                {
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
        [Route("api/auth/ForgotPassword")]
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

                    // Send an email with the reset password link
                    var code = await userManager.GeneratePasswordResetTokenAsync(user.Id);
                    var callbackUrl = Url.Link("Default",
                        new {controller = "Account", action = "ResetPassword", userId = user.Id, code});
                    var fullName = _employeeRepo.GetEmployeeByEmail(model.Email).FullName;
                    var email = new LUSSISEmail.Builder().From("sa45team7@gmail.com").To(model.Email)
                        .ForResetPassword(fullName, callbackUrl).Build();
                    new System.Threading.Thread(delegate() { EmailHelper.SendEmail(email); }).Start();
                }
                catch (Exception e)
                {
                    return Ok(e);
                }

                return Ok(new {Message = "Reset link sent to your email."});
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