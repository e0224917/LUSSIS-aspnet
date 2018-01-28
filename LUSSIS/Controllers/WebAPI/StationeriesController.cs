using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using LUSSIS.Models.WebAPI;
using LUSSIS.Repositories;

namespace LUSSIS.Controllers.WebAPI
{
    public class StationeriesController : ApiController
    {
        private readonly StationeryRepository _stationeryRepo = new StationeryRepository();

        // GET: api/Stationeries
        public IEnumerable<StationeryDTO> GetStationeries()
        {
            return _stationeryRepo.GetAll().Select(item => new StationeryDTO()
                {
                    ItemNum = item.ItemNum,
                    Category = item.Category.CategoryName,
                    Description = item.Description,
                    ReorderLevel = item.ReorderLevel,
                    ReorderQty = item.ReorderQty,
                    AvailableQty = item.AvailableQty,
                    UnitOfMeasure = item.UnitOfMeasure,
                    BinNum = item.BinNum
                })
                .ToList();
        }

        // GET: api/Stationeries/C001
        [ResponseType(typeof(StationeryDTO))]
        public async Task<IHttpActionResult> GetStationery(string id)
        {
            var stationery = await _stationeryRepo.GetByIdAsync(id);
            if (stationery == null)
            {
                return NotFound();
            }

            var dto = new StationeryDTO()
            {
                Description = stationery.Description,
                BinNum = stationery.BinNum,
                AvailableQty = stationery.AvailableQty,
                Category = stationery.Category.CategoryName,
                ItemNum = stationery.ItemNum,
                ReorderLevel = stationery.ReorderLevel,
                ReorderQty = stationery.ReorderQty,
                UnitOfMeasure = stationery.UnitOfMeasure
            };

            return Ok(dto);
        }

        // PUT: api/Stationeries/C001
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutStationery(string id, StationeryDTO stationery)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != stationery.ItemNum)
            {
                return BadRequest();
            }

            var s = _stationeryRepo.GetById(id);

            s.Description = stationery.Description;
            s.ReorderLevel = stationery.ReorderLevel;
            s.ReorderQty = stationery.ReorderQty;
            if (s.AvailableQty != stationery.AvailableQty)
            {
                //TODO: create adjustment voucher
                s.CurrentQty = stationery.AvailableQty;
            }

            s.BinNum = stationery.BinNum;

            await _stationeryRepo.UpdateAsync(s);

            return StatusCode(HttpStatusCode.NoContent);
        }

        //// POST: api/Stationeries
        //[ResponseType(typeof(Stationery))]
        //public async Task<IHttpActionResult> PostStationery(Stationery stationery)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    db.Stationeries.Add(stationery);

        //    try
        //    {
        //        await db.SaveChangesAsync();
        //    }
        //    catch (DbUpdateException)
        //    {
        //        if (StationeryExists(stationery.ItemNum))
        //        {
        //            return Conflict();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return CreatedAtRoute("DefaultApi", new { id = stationery.ItemNum }, stationery);
        //}

        //// DELETE: api/Stationeries/5
        //[ResponseType(typeof(Stationery))]
        //public async Task<IHttpActionResult> DeleteStationery(string id)
        //{
        //    Stationery stationery = await db.Stationeries.FindAsync(id);
        //    if (stationery == null)
        //    {
        //        return NotFound();
        //    }

        //    db.Stationeries.Remove(stationery);
        //    await db.SaveChangesAsync();

        //    return Ok(stationery);
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stationeryRepo.Dispose();
            }
            base.Dispose(disposing);
        }

        //private bool StationeryExists(string id)
        //{
        //    return db.Stationeries.Count(e => e.ItemNum == id) > 0;
        //}
    }
}