using System;
using System.Threading.Tasks;
using System.Web.Http;
using LUSSIS.Constants;
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
        public async Task<IHttpActionResult> Post([FromBody] AdjustmentDTO adjustment)
        {
            var adjVoucher = new AdjVoucher
            {
                ItemNum = adjustment.ItemNum,
                CreateDate = DateTime.Today,
                Quantity = adjustment.Quantity,
                Reason = adjustment.Reason,
                Status = AdjustmentVoucherStatus.Pending,
                RequestEmpNum = adjustment.RequestEmpNum
            };

            await _stockadjustmentRepo.AddAsync(adjVoucher);

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