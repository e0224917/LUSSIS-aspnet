using LUSSIS.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using System.Web.Http.Description;
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

                var emp = db.Employees.First(em => em.EmailAddress == model.Email);
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
    }
}
