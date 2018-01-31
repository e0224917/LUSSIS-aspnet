using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using LUSSIS.Emails;
using LUSSIS.Models.WebAPI;
using LUSSIS.Repositories;
using Delegate = LUSSIS.Models.Delegate;

namespace LUSSIS.Controllers.WebAPI
{
    //Authors: Ton That Minh Nhat
    public class DelegateController : ApiController
    {
        private readonly DelegateRepository _delegateRepo = new DelegateRepository();
        private readonly EmployeeRepository _employeeRepo = new EmployeeRepository();

        // GET api/Delegate/COMM
        [HttpGet]
        [Route("api/Delegate/{dept}")]
        [ResponseType(typeof(DelegateDTO))]
        public IHttpActionResult Get([FromUri] string dept)
        {
            var @delegate = _delegateRepo.FindExistingByDeptCode(dept);

            if (@delegate == null) return BadRequest("No delegate available.");

            var result = new DelegateDTO()
            {
                DelegateId = @delegate.DelegateId,
                StartDate = @delegate.StartDate,
                EndDate = @delegate.EndDate,
                Employee = new EmployeeDTO(@delegate.Employee)
                {
                    IsDelegated = true
                }
            };
            return Ok(result);
        }

        [HttpPost]
        [Route("api/Delegate/")]
        // POST api/Delegate
        public IHttpActionResult Post([FromBody] DelegateDTO delegateDto)
        {
            var d = new Delegate()
            {
                StartDate = delegateDto.StartDate,
                EndDate = delegateDto.EndDate,
                EmpNum = delegateDto.Employee.EmpNum
            };

            _delegateRepo.Add(d);

            //Send email on new thread
            var headEmail = _employeeRepo.GetDepartmentHead(delegateDto.Employee.DeptCode);
            var email = new LUSSISEmail.Builder().From(headEmail).To(delegateDto.Employee.EmailAddress)
                .ForNewDelegate().Build();
            new Thread(delegate() { EmailHelper.SendEmail(email); }).Start();

            var id = _delegateRepo.FindExistingByDeptCode(delegateDto.Employee.DeptCode).DelegateId;
            delegateDto.DelegateId = id;

            return Ok(delegateDto);
        }

        [HttpDelete]
        [Route("api/Delegate/")]
        // DELETE api/Delegate/
        public IHttpActionResult Delete([FromBody] DelegateDTO delegateDto)
        {
            _delegateRepo.DeleteByDeptCode(delegateDto.Employee.DeptCode);

            //Send email
            var headEmail = _employeeRepo.GetDepartmentHead(delegateDto.Employee.DeptCode);
            var email = new LUSSISEmail.Builder().From(headEmail).To(delegateDto.Employee.EmailAddress)
                .ForOldDelegate().Build();
            new Thread(delegate() { EmailHelper.SendEmail(email); }).Start();

            return Ok(new {Message = "Delegate has been revoked"});
        }

        [HttpGet]
        [Route("api/Delegate/Employee/{dept}")]
        public IEnumerable<EmployeeDTO> GetEmployeeList(string dept)
        {
            var list = _employeeRepo.GetStaffRepByDeptCode(dept);

            return list.Select(item => new EmployeeDTO(item));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _delegateRepo.Dispose();
                _employeeRepo.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}