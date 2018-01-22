using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        [Route("api/Disbursement/{dept}")]
        [ResponseType(typeof(DisbursementDTO))]
        public IHttpActionResult Upcoming([FromUri] string dept)
        {
            var d = _repo.GetUpcomingDisbursement(dept);
            if (d == null) return NotFound();

            var result = new DisbursementDTO()
            {
                DisbursementId = d.DisbursementId,
                CollectionDate = (DateTime) d.CollectionDate,
                CollectionPoint = d.CollectionPoint.CollectionName,
                CollectionPointId = (int) d.CollectionPointId,
                DisbursementDetails = d.DisbursementDetails.Select(details => new RequisitionDetailDTO()
                {
                    Description = details.Stationery.Description,
                    Quantity = (int) details.ActualQty,
                    UnitOfMeasure = details.Stationery.UnitOfMeasure
                })
            };
            return Ok(result);
        }

        // POST api/<controller>
        [Route("api/Disbursement/{id}")]
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