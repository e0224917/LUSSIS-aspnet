using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using LUSSIS.Models.WebAPI;
using LUSSIS.Repositories;

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
            var result = list.Select(item => new DisbursementDTO()
            {
                CollectionDate = item.CollectionDate,
                CollectionPoint = item.CollectionPoint.CollectionName,
                CollectionPointId = (int) item.CollectionPointId,
                CollectionTime = item.CollectionPoint.Time,
                DepartmentName = item.Department.DeptName,
                DisbursementId = item.DisbursementId,
                DisbursementDetails = item.DisbursementDetails.Select(detail => new RequisitionDetailDTO()
                {
                    Description = detail.Stationery.Description,
                    Quantity = detail.ActualQty,
                    UnitOfMeasure = detail.Stationery.UnitOfMeasure
                })
            });

            return Ok(result);
        }

        [HttpGet]
        [Route("api/Disbursement/{id}")]
        public async Task<DisbursementDTO> Get(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            return new DisbursementDTO()
            {
                CollectionDate = item.CollectionDate,
                CollectionPoint = item.CollectionPoint.CollectionName,
                CollectionPointId = (int) item.CollectionPointId,
                CollectionTime = item.CollectionPoint.Time,
                DepartmentName = item.Department.DeptName,
                DisbursementId = item.DisbursementId,
                DisbursementDetails = item.DisbursementDetails.Select(detail => new RequisitionDetailDTO()
                {
                    Description = detail.Stationery.Description,
                    Quantity = detail.ActualQty,
                    UnitOfMeasure = detail.Stationery.UnitOfMeasure
                })
            };
        } 

        [HttpGet]
        [Route("api/Disbursement/Upcoming/{dept}")]
        [ResponseType(typeof(DisbursementDTO))]
        public IHttpActionResult Upcoming([FromUri] string dept)
        {
            var d = _repo.GetUpcomingDisbursement(dept);
            if (d == null) return NotFound();

            var result = new DisbursementDTO()
            {
                DisbursementId = d.DisbursementId,
                CollectionDate = d.CollectionDate,
                CollectionPoint = d.CollectionPoint.CollectionName,
                CollectionPointId = (int) d.CollectionPointId,
                CollectionTime = d.CollectionPoint.Time,
                DepartmentName = d.Department.DeptName,
                DisbursementDetails = d.DisbursementDetails.Select(details => new RequisitionDetailDTO()
                {
                    Description = details.Stationery.Description,
                    Quantity = details.ActualQty,
                    UnitOfMeasure = details.Stationery.UnitOfMeasure
                })
            };
            return Ok(result);
        }

        // POST api/<controller>
        [Route("api/Disbursement/Acknowledge/{id}")]
        public IHttpActionResult Acknowledge(int id, [FromBody] int empnum)
        {
            var disbursement = _repo.GetById(id);

            var isFulfilled = disbursement.DisbursementDetails.All(item => item.ActualQty == item.RequestedQty);

            disbursement.Status = isFulfilled ? "fulfilled" : "unfulfilled";

            disbursement.AcknowledgeEmpNum = empnum;

            return Ok(new {Message = "Acknowledged"});
        }

        
    }
}