using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using LUSSIS.Constants;
using LUSSIS.Models.WebAPI;
using LUSSIS.Repositories;

namespace LUSSIS.Controllers.WebAPI
{
    //Authors: Ton That Minh Nhat
    public class DisbursementController : ApiController
    {
        private readonly DisbursementRepository _disbursementRepo = new DisbursementRepository();
        private readonly StationeryRepository _stationeryRepo = new StationeryRepository();
        private readonly EmployeeRepository _employeeRepo = new EmployeeRepository();

        [HttpGet]
        [Route("api/Disbursement/")]
        public IHttpActionResult Get()
        {
            var disbursements = _disbursementRepo.GetDisbursementByStatus(DisbursementStatus.InProcess);
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
        [Route("api/Disbursement/Acknowledge/")]
        public IHttpActionResult Acknowledge(int id, int empnum)
        {
            var disbursement = _disbursementRepo.GetById(id);
            var employee = _employeeRepo.GetById(empnum);

            if (employee.DeptCode != disbursement.DeptCode)
            {
                return BadRequest("Wrong department.");
            }

            if (disbursement.Status != DisbursementStatus.InProcess)
            {
                return BadRequest("This disbursement has already been acknowledged");
            }

            _disbursementRepo.Acknowledge(disbursement);
            //update current quantity of stationery
            foreach (var disbursementDetail in disbursement.DisbursementDetails)
            {
                var stationery = _stationeryRepo.GetById(disbursementDetail.ItemNum);
                stationery.CurrentQty -= disbursementDetail.ActualQty;
                _stationeryRepo.Update(stationery);
            }

            return Ok(new {Message = "Disbursement acknowledged"});
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _disbursementRepo.Dispose();
                _stationeryRepo.Dispose();
                _employeeRepo.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}