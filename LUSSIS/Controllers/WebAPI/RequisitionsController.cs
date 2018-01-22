﻿using System;
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

        //GET: api/Requisitions/
        [Route("api/Requisitions/Pending/{dept}")]
        public IEnumerable<RequisitionDTO> GetPending(string dept)
        {
            var list = _repo.GetPendingListForHead(dept).ToList();

            var result = list.Select(item => new RequisitionDTO()
            {
                RequisitionId = item.RequisitionId,
                RequisitionEmp = item.RequisitionEmployee.FirstName + " " + item.RequisitionEmployee.LastName,
                RequisitionDate = (DateTime) item.RequisitionDate,
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
                return Ok(new { Message = "Updated"});
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}