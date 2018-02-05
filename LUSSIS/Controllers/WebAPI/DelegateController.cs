using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        [Route("api/Delegate/Get/{dept}")]
        [ResponseType(typeof(DelegateDTO))]
        public IHttpActionResult GetDelegate([FromUri] string dept)
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
        [Route("api/Delegate/Create/{empnum}")]
        // POST api/Delegate/Create
        public IHttpActionResult CreateDelegate(int empnum, [FromBody] DelegateDTO delegateDto)
        {
            var d = new Delegate()
            {
                StartDate = delegateDto.StartDate,
                EndDate = delegateDto.EndDate,
                EmpNum = empnum
            };

            _delegateRepo.Add(d);

            //Send email on new thread
            var employee = _employeeRepo.GetById(empnum);
            var headEmail = _employeeRepo.GetDepartmentHead(employee.DeptCode).EmailAddress;
            var email = new LUSSISEmail.Builder().From(headEmail).To(employee.EmailAddress)
                .ForNewDelegate().Build();
            new Thread(delegate() { EmailHelper.SendEmail(email); }).Start();

            //return delegate with id included
            var id = _delegateRepo.FindExistingByDeptCode(employee.DeptCode).DelegateId;
            delegateDto.DelegateId = id;

            return Ok(delegateDto);
        }

        [HttpPost]
        [Route("api/Delegate/Delete")]
        // POST api/Delegate/Delete
        public IHttpActionResult DeleteDelegate([FromBody] DelegateDTO delegateDto)
        {
            var del = _delegateRepo.GetById(delegateDto.DelegateId);
            var toEmail = _employeeRepo.GetById(del.EmpNum).EmailAddress;
            var deptCode = _employeeRepo.GetById(del.EmpNum).DeptCode;
            _delegateRepo.Delete(del);

            //Send email
            var headEmail = _employeeRepo.GetDepartmentHead(deptCode).EmailAddress;
            var email = new LUSSISEmail.Builder().From(headEmail).To(toEmail)
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