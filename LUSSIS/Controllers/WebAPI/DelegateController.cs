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
        private readonly DepartmentRepository _departmentRepo = new DepartmentRepository();
        private readonly EmployeeRepository _employeeRepo = new EmployeeRepository();

        // GET api/Delegate/COMM
        [HttpGet]
        [Route("api/Delegate/{dept}")]
        [ResponseType(typeof(DelegateDTO))]
        public IHttpActionResult Get([FromUri] string dept)
        {
            var d = _delegateRepo.FindExistingByDeptCode(dept);

            if (d == null) return BadRequest("No delegate available.");

            var result = new DelegateDTO()
            {
                DelegateId = d.DelegateId,
                StartDate = d.StartDate,
                EndDate = d.EndDate,
                Employee = new EmployeeDTO()
                {
                    DeptCode = d.Employee.DeptCode,
                    DeptName = d.Employee.Department.DeptName,
                    EmailAddress = d.Employee.EmailAddress,
                    EmpNum = d.EmpNum,
                    FirstName = d.Employee.FirstName,
                    LastName = d.Employee.LastName,
                    IsDelegated = true,
                    JobTitle = d.Employee.JobTitle,
                    Title = d.Employee.Title
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
            var thread = new Thread(delegate() { EmailHelper.SendEmail(email); });
            thread.Start();

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
            var thread = new Thread(delegate() { EmailHelper.SendEmail(email); });
            thread.Start();

            return Ok(new {Message = "Delegate has been revoked"});
        }

        [HttpGet]
        [Route("api/Delegate/Employee/{dept}")]
        public IEnumerable<EmployeeDTO> GetEmployeeList(string dept)
        {
            var list = _departmentRepo.GetById(dept).Employees;

            return list.Select(item => new EmployeeDTO()
            {
                DeptCode = item.DeptCode,
                DeptName = item.Department.DeptName,
                EmailAddress = item.EmailAddress,
                EmpNum = item.EmpNum,
                FirstName = item.FirstName,
                LastName = item.LastName,
                IsDelegated = false,
                JobTitle = item.JobTitle,
                Title = item.Title
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _departmentRepo.Dispose();
                _departmentRepo.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}