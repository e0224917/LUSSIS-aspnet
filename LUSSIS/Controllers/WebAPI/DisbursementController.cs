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
            var disbursements = _disbursementRepo.GetDisbursementByStatus("inprocess");
            var result = disbursements.Select(item => new DisbursementDTO(item));

            return Ok(result);
        }

        [HttpGet]
        [Route("api/Disbursement/{id}")]
        public async Task<DisbursementDTO> Get(int id)
        {
            var disbursement = await _disbursementRepo.GetByIdAsync(id);
            return new DisbursementDTO(disbursement);
        } 

        [HttpGet]
        [Route("api/Disbursement/Upcoming/{dept}")]
        [ResponseType(typeof(DisbursementDTO))]
        public IHttpActionResult Upcoming([FromUri] string dept)
        {
            var disbursement = _disbursementRepo.GetUpcomingDisbursement(dept);
            if (disbursement == null) return NotFound();

            return Ok(new DisbursementDTO(disbursement));
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