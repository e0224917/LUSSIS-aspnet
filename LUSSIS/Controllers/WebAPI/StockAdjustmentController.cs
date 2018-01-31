using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using LUSSIS.Models;
using LUSSIS.Models.WebAPI;
using LUSSIS.Repositories;

namespace LUSSIS.Controllers.WebAPI
{
    //Authors: Ton That Minh Nhat
    public class StockAdjustmentController : ApiController
    {
        private readonly StockAdjustmentRepository _stockadjustmentRepo = new StockAdjustmentRepository();

        // POST api/StockAdjustment
        [HttpPost]
        [Route("api/StockAdjustment/")]
        public async Task<IHttpActionResult> Post([FromBody]AdjustmentDTO adjustment)
        {
            var ad = new AdjVoucher
            {
                ItemNum = adjustment.ItemNum,
                CreateDate = DateTime.Today,
                Quantity = adjustment.Quantity,
                Reason = adjustment.Reason,
                Status = "pending",
                RequestEmpNum = adjustment.RequestEmpNum
            };

            await _stockadjustmentRepo.AddAsync(ad);

            return Ok(new {Message = "New adjusment sent"});
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stockadjustmentRepo.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}