using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using LUSSIS.Models.WebAPI;
using LUSSIS.Repositories;
using Delegate = LUSSIS.Models.Delegate;

namespace LUSSIS.Controllers.WebAPI
{
    public class DelegateController : ApiController
    {
        private readonly DelegateRepository _delegateRepo = new DelegateRepository();
        private readonly DepartmentRepository _departmentRepo = new DepartmentRepository();

        // GET api/Delegate/COMM
        [HttpGet]
        [Route("api/Delegate/{dept}")]
        [ResponseType(typeof(DelegateDTO))]
        public IHttpActionResult Get([FromUri] string dept)
        {
            var d = _delegateRepo.GetAll().LastOrDefault(de => de.Employee.DeptCode == dept);

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
        [Route("api/Delegate/{dept}")]
        // POST api/Delegate
        public async Task<IHttpActionResult> Post(string dept, [FromBody] DelegateDTO del)
        {
            var d = new Delegate()
            {
                StartDate = del.StartDate,
                EndDate = del.EndDate,
                EmpNum = del.Employee.EmpNum
            };

            await _delegateRepo.AddAsync(d);

            var id = _delegateRepo.GetByDeptCode(dept).DelegateId;
            del.DelegateId = id;

            return Ok(del);
        }

        [HttpPut]
        [Route("api/Delegate/{dept}")]
        public IHttpActionResult Put(string dept, [FromBody] DelegateDTO del)
        {
            var d = _delegateRepo.GetByDeptCode(dept);
            d.EmpNum = del.Employee.EmpNum;
            d.StartDate = del.StartDate;
            d.EndDate = del.EndDate;

            _delegateRepo.Update(d);

            return Ok(new {Message = "Editted delegate"});
        }

        [HttpDelete]
        [Route("api/Delegate/{dept}")]
        // DELETE api/Delegate/COMM
        public IHttpActionResult Delete(string dept)
        {
            _delegateRepo.DeleteByDeptCode(dept);

            return Ok(new {Message = "Revoked delegate"});
        }

        [HttpGet]
        [Route("api/Delegate/Employee/{dept}")]
        public IEnumerable<EmployeeDTO> GetEmployeeList(string deptCode)
        {
            var list = _departmentRepo.GetById(deptCode).Employees;

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
    }
}