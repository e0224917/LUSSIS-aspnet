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
        private readonly RequisitionRepository _requistionRepo = new RequisitionRepository();

        //GET: api/Requisitions/
        [Route("api/Requisitions/Pending/{dept}")]
        public IEnumerable<RequisitionDTO> GetPending(string dept)
        {
            var list = _requistionRepo.GetPendingListForHead(dept).ToList();

            var result = list.Select(item => new RequisitionDTO()
            {
                RequisitionId = item.RequisitionId,
                RequisitionEmp = item.RequisitionEmployee.ToApiDTO(),
                RequisitionDate = item.RequisitionDate,
                ApprovalEmp = null,
                ApprovalRemarks = item.ApprovalRemarks ?? "",
                RequestRemarks = item.RequestRemarks ?? "",
                RequisitionDetails = item.RequisitionDetails.Select(detail => new RequisitionDetailDTO()
                {
                    Description = detail.Stationery.Description,
                    UnitOfMeasure = detail.Stationery.UnitOfMeasure,
                    Quantity = detail.Quantity
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
        public async Task<IHttpActionResult> Process([FromBody] RequisitionDTO requisition)
        {
            try
            {
                var req = await _requistionRepo.GetByIdAsync(requisition.RequisitionId);
                req.ApprovalEmpNum = requisition.ApprovalEmp.EmpNum;
                req.ApprovalRemarks = requisition.ApprovalRemarks;
                req.ApprovalDate = DateTime.Today;
                req.Status = requisition.Status;

                await _requistionRepo.UpdateAsync(req);
                return Ok(new {Message = "Updated"});
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("api/Requisitions/MyReq/{empnum}")]
        public IHttpActionResult MyRequisitions(int empnum)
        {
            var req = _requistionRepo.GetRequisitionByEmpNum(empnum);
            var result = req.Select(item => new RequisitionDTO()
            {
                ApprovalEmp = item.ApprovalEmployee.ToApiDTO(),
                ApprovalRemarks = item.ApprovalRemarks,
                RequestRemarks = item.RequestRemarks,
                RequisitionDate = item.RequisitionDate,
                RequisitionEmp = item.RequisitionEmployee.ToApiDTO(),
                RequisitionId = item.RequisitionId,
                Status = item.Status,
                RequisitionDetails = item.RequisitionDetails.Select(detail => new RequisitionDetailDTO()
                {
                    Description = detail.Stationery.Description,
                    Quantity = detail.Quantity,
                    UnitOfMeasure = detail.Stationery.UnitOfMeasure
                })
            });

            return Ok(result);
        }

        [HttpGet]
        [Route("api/Requisitions/Consolidated")]
        public List<RetrievalItemDTO> GetConsolidatedRequisition()
        {
            var list = _requistionRepo.GetConsolidatedRequisition().Select(x => new RetrievalItemDTO
            {
                ItemNum = x.ItemNum,
                AvailableQty = (int) x.AvailableQty,
                BinNum = x.BinNum,
                RequestedQty = (int) x.RequestedQty,
                Description = x.Description,
                UnitOfMeasure = x.UnitOfMeasure
            });
            return list.ToList();
        }

        [HttpGet]
        [Route("api/Requisitions/Retrieval")]
        public IEnumerable<RetrievalItemDTO> GetRetrievalList()
        {
            return _requistionRepo.GetRetrievalInPorcess().Select(x => new RetrievalItemDTO
            {
                ItemNum = x.ItemNum,
                AvailableQty = (int) x.AvailableQty,
                BinNum = x.BinNum,
                RequestedQty = (int) x.RequestedQty,
                Description = x.Description,
                UnitOfMeasure = x.UnitOfMeasure
            });
        }
    }
}