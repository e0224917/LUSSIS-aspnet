using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using LUSSIS.Models.WebAPI;
using LUSSIS.Repositories;
using static LUSSIS.Constants.DisbursementStatus;

namespace LUSSIS.Controllers.WebAPI
{
    //Authors: Ton That Minh Nhat
    public class DisbursementController : ApiController
    {
        private readonly DisbursementRepository _disbursementRepo = new DisbursementRepository();

        [HttpGet]
        [Route("api/Disbursement/")]
        public IHttpActionResult Get()
        {
            var list = _disbursementRepo.GetDisbursementByStatus(InProcess);
            var result = list.Select(item => item.ToApiDTO());

            return Ok(result);
        }

        [HttpGet]
        [Route("api/Disbursement/{id}")]
        public async Task<DisbursementDTO> Get(int id)
        {
            var item = await _disbursementRepo.GetByIdAsync(id);
            return item.ToApiDTO();
        } 

        [HttpGet]
        [Route("api/Disbursement/Upcoming/{dept}")]
        [ResponseType(typeof(DisbursementDTO))]
        public IHttpActionResult Upcoming([FromUri] string dept)
        {
            var d = _disbursementRepo.GetUpcomingDisbursement(dept);
            if (d == null) return NotFound();

            return Ok(d.ToApiDTO());
        }

        // POST api/<controller>
        [Route("api/Disbursement/Acknowledge/{id}")]
        public IHttpActionResult Acknowledge(int id, [FromBody] EmployeeDTO employee)
        {
            var disbursement = _disbursementRepo.GetById(id);

            if (employee.DeptCode != disbursement.DeptCode)
            {
                return BadRequest("Wrong department.");
            }

            var isFulfilled = disbursement.DisbursementDetails.All(item => item.ActualQty == item.RequestedQty);

            disbursement.Status = isFulfilled ? Fulfilled : Unfulfilled;

            disbursement.AcknowledgeEmpNum = employee.EmpNum;

            return Ok(new {Message = "Acknowledged"});
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _disbursementRepo.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}