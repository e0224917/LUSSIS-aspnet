using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using LUSSIS.Models.WebAPI;
using LUSSIS.Repositories;
using static LUSSIS.Constants.DisbursementStatus;

namespace LUSSIS.Controllers.WebAPI
{
    public class DisbursementController : ApiController
    {
        private readonly DisbursementRepository _repo = new DisbursementRepository();

        [HttpGet]
        [Route("api/Disbursement/")]
        public IHttpActionResult Get()
        {
            var list = _repo.GetDisbursementByStatus("inprocess");
            var result = list.Select(item => item.ToApiDTO());

            return Ok(result);
        }

        [HttpGet]
        [Route("api/Disbursement/{id}")]
        public async Task<DisbursementDTO> Get(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            return item.ToApiDTO();
        } 

        [HttpGet]
        [Route("api/Disbursement/Upcoming/{dept}")]
        [ResponseType(typeof(DisbursementDTO))]
        public IHttpActionResult Upcoming([FromUri] string dept)
        {
            var d = _repo.GetUpcomingDisbursement(dept);
            if (d == null) return NotFound();

            return Ok(d.ToApiDTO());
        }

        // POST api/<controller>
        [Route("api/Disbursement/Acknowledge/{id}")]
        public IHttpActionResult Acknowledge(int id, [FromBody] int empnum)
        {
            var disbursement = _repo.GetById(id);

            var isFulfilled = disbursement.DisbursementDetails.All(item => item.ActualQty == item.RequestedQty);

            disbursement.Status = isFulfilled ? Fulfilled : Unfulfilled;

            disbursement.AcknowledgeEmpNum = empnum;

            return Ok(new {Message = "Acknowledged"});
        }

    }
}