﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using LUSSIS.Models;
using LUSSIS.Models.WebAPI;
using LUSSIS.Repositories;

namespace LUSSIS.Controllers.WebAPI
{
    //Authors: Ton That Minh Nhat
    public class RequisitionsController : ApiController
    {
        private readonly RequisitionRepository _requistionRepo = new RequisitionRepository();
        private readonly DisbursementRepository _disbursementRepo = new DisbursementRepository();
        private readonly DelegateRepository _delegateRepo = new DelegateRepository();

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
                    ItemNum = detail.ItemNum,
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
            var req = _requistionRepo.GetRequisitionsByEmpNum(empnum);
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
                    ItemNum = detail.Stationery.ItemNum,
                    Description = detail.Stationery.Description,
                    Quantity = detail.Quantity,
                    UnitOfMeasure = detail.Stationery.UnitOfMeasure
                })
            });

            return Ok(result);
        }

        [HttpGet]
        [Route("api/Requisitions/Retrieval")]
        public IEnumerable<RetrievalItemDTO> GetRetrievalList()
        {
            return _disbursementRepo.GetRetrievalInProcess().Select(x => new RetrievalItemDTO
            {
                ItemNum = x.ItemNum,
                AvailableQty = x.AvailableQty,
                BinNum = x.BinNum,
                RequestedQty = x.RequestedQty,
                Description = x.Description,
                UnitOfMeasure = x.UnitOfMeasure
            });
        }

        [HttpPost]
        [Route("api/Requisitions/Create")]
        public IHttpActionResult Create([FromBody] RequisitionDTO requisitionDto)
        {
            var empNum = requisitionDto.RequisitionEmp.EmpNum;
            var isDelegated = _delegateRepo.FindCurrentByEmpNum(empNum) != null;

            if (isDelegated) return BadRequest("Delegated staff cannot make request");

            var detail = requisitionDto.RequisitionDetails.First();

            var requisitionDetail = new RequisitionDetail()
            {
                ItemNum = detail.ItemNum,
                Quantity = detail.Quantity
            };

            var requisition = new Requisition()
            {
                RequisitionEmpNum = requisitionDto.RequisitionEmp.EmpNum,
                DeptCode = requisitionDto.RequisitionEmp.DeptCode,
                RequestRemarks = requisitionDto.RequestRemarks,
                RequisitionDate = DateTime.Today,
                Status = "inprogress",
                RequisitionDetails = new List<RequisitionDetail>() {requisitionDetail}
            };

            _requistionRepo.Add(requisition);
            return Ok(new {Message = "Requisition created"});
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _requistionRepo.Dispose();
                _disbursementRepo.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}