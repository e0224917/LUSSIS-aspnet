using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using LUSSIS.Models;
using LUSSIS.Models.WebAPI;
using LUSSIS.Repositories;

namespace LUSSIS.Controllers.WebAPI
{
    public class StockAdjustmentController : ApiController
    {
        private readonly StockAdjustmentRepository _repo = new StockAdjustmentRepository();
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new [] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
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

            await _repo.AddAsync(ad);

            return Ok(new {Message = "New adjusment sent."});
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}