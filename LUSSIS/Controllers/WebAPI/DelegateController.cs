using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Http.Description;
using LUSSIS.Models.WebAPI;
using LUSSIS.Repositories;
using Delegate = LUSSIS.Models.Delegate;

namespace LUSSIS.Controllers.WebAPI
{
    public class DelegateController : ApiController
    {
        private readonly DelegateRepository _repo = new DelegateRepository();

        // GET api/Delegate/COMM
        [HttpGet]
        [Route("api/Delegate/{dept}")]
        [ResponseType(typeof(DelegateDTO))]
        public IHttpActionResult Get([FromUri] string dept)
        {
            var d = _repo.GetAll().LastOrDefault(de => de.Employee.DeptCode == dept);

            if (d == null) return Ok(new { });

            var result = new DelegateDTO()
            {
                DelegateId = d.DelegateId,
                StartDate = (DateTime) d.StartDate,
                EndDate = (DateTime) d.EndDate,
                Employee = new EmployeeDTO()
                {
                    DeptCode = d.Employee.DeptCode,
                    DeptName = d.Employee.Department.DeptName,
                    EmailAddress = d.Employee.EmailAddress,
                    EmpNum = (int) d.EmpNum,
                    FirstName = d.Employee.FirstName,
                    LastName = d.Employee.LastName,
                    IsDelegated = true,
                    JobTitle = d.Employee.JobTitle,
                    Title = d.Employee.Title
                }
            };
            return Ok(result);

        }

        // POST api/Delegate
        public async Task<IHttpActionResult> Post([FromBody] DelegateDTO del)
        {
            var d = new Delegate()
            {
                StartDate = del.StartDate,
                EndDate = del.EndDate,
                EmpNum = del.Employee.EmpNum
            };

            await _repo.AddAsync(d);

            return Ok(new {Message = "Added delegate"});
        }

        [HttpPut]
        [Route("api/Delegate/{dept}")]
        public IHttpActionResult Put(int id, [FromBody] DelegateDTO del)
        {
            var d = new Delegate()
            {
                StartDate = del.StartDate,
                EndDate = del.EndDate,
                EmpNum = del.Employee.EmpNum,
                DelegateId = id
            };
            _repo.Update(d);

            return Ok(new {Message = "Editted delegate"});
        }

        [HttpDelete]
        [Route("api/Delegate/{dept}")]
        // DELETE api/Delegate/5
        public void Delete(string dept)
        {
        }
    }
}