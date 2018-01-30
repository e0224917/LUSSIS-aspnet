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
        private readonly StationeryRepository _stationeryRepo = new StationeryRepository();

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

            //check if disbursement is fulfilled or not and update status
            var isFulfilled = disbursement.DisbursementDetails.All(item => item.ActualQty == item.RequestedQty);
            disbursement.Status = isFulfilled ? Fulfilled : Unfulfilled;
            disbursement.AcknowledgeEmpNum = employee.EmpNum;
            _disbursementRepo.Update(disbursement);

            //update current quantity of stationery
            foreach (var disbursementDetail in disbursement.DisbursementDetails)
            {
                var stationery = _stationeryRepo.GetById(disbursementDetail.ItemNum);
                stationery.CurrentQty -= disbursementDetail.ActualQty;
                _stationeryRepo.Update(stationery);
            }

            return Ok(new {Message = "Acknowledged"});
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _disbursementRepo.Dispose();
                _stationeryRepo.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}