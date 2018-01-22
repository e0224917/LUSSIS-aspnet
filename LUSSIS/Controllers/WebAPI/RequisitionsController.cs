using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using LUSSIS.Models;
using LUSSIS.Models.WebAPI;
using LUSSIS.Repositories;

namespace LUSSIS.Controllers.WebAPI
{
    public class RequisitionsController : ApiController
    {
        private readonly RequisitionRepository _repo = new RequisitionRepository();

        // GET: api/Requisitions/COMM/?status=pending&empnum=77
        //        [ResponseType(typeof(RequisitionDTO))]
        //        public async Task<IHttpActionResult> GetRequisition(string dept, [FromUri]string status, [FromUri]int empnum)
        //        {
        //            var reqList = repo.GetRequisitionsByReferences(dept, status, empnum);
        //            var list = repo.GetRequisitionsByStatus(status);
        //            
        //        }

        //GET: api/Requisitions/
        [Route("api/Requisitions/Pending/{dept}")]
        public IEnumerable<RequisitionDTO> GetPending(string dept)
        {
            var list = _repo.GetPendingListForHead(dept).ToList();

            var result = list.Select(item => new RequisitionDTO()
            {
                RequisitionId = item.RequisitionId,
                RequisitionEmp = item.RequisitionEmployee.FirstName + " " + item.RequisitionEmployee.LastName,
                RequisitionDate = (DateTime)item.RequisitionDate,
                ApprovalEmp = item.ApprovalEmpNum != null ? item.ApprovalEmployee.FirstName + " " + item.ApprovalEmployee.LastName : "",
                ApprovalRemarks = item.ApprovalRemarks != null ? item.ApprovalRemarks : "",
                RequestRemarks = item.RequestRemarks != null ? item.RequestRemarks : "",
                RequisitionDetails = item.RequisitionDetails.Select(detail => new RequisitionDetailDTO()
                {
                    Description = detail.Stationery.Description,
                    UnitOfMeasure = detail.Stationery.UnitOfMeasure,
                    Quantity = (int)detail.Quantity
                })
            });

            return result;
        }

        [HttpGet]
        [Route("api/Requisitions/Process")]
        public IHttpActionResult Test(int empnum, string status)
        {
            return Ok(empnum + status);
        }

        [HttpPost]
        [Route("api/Requisitions/Process")]
        public async Task<IHttpActionResult> Process(int empnum, string status, [FromBody]RequisitionDTO requisition)
        {
            try
            {
                var req = await _repo.GetByIdAsync(requisition.RequisitionId);
                req.ApprovalEmpNum = empnum;
                req.ApprovalRemarks = requisition.ApprovalRemarks;
                req.ApprovalDate = DateTime.Today;
                req.Status = status;

                await _repo.UpdateAsync(req);
                return Ok(new { Message = "Updated" });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        public List<RetrievalItemDTO> GetConsolidatedRequisition()
        {
            var list = _repo.GetConsolidatedRequisition().Select(x => new RetrievalItemDTO
            {
                ItemNum = x.ItemNum,
                AvailableQty = x.AvailableQty,
                BinNum = x.BinNum,
                RequestedQty = x.RequestedQty,
                Description = x.Description,
                UnitOfMeasure = x.UnitOfMeasure
            });
            return list.ToList();
        }

    }
}