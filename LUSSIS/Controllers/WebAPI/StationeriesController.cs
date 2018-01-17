using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using LUSSIS.Models;
using LUSSIS.Models.WebAPI;

namespace LUSSIS.Controllers.WebAPI
{
    public class StationeriesController : ApiController
    {
        private LUSSISContext db = new LUSSISContext();

        // GET: api/Stationeries
        public IQueryable<Stationery> GetStationeries()
        {
            return db.Stationeries;
        }

        // GET: api/Stationeries/C001
        [ResponseType(typeof(StationeryDTO))]
        public async Task<IHttpActionResult> GetStationery(string id)
        {
            Stationery stationery = await db.Stationeries.FindAsync(id);
            if (stationery == null)
            {
                return NotFound();
            }

            StationeryDTO dto = new StationeryDTO()
            {
                BinNum = stationery.BinNum
            };

            return Ok(dto);
        }

        // PUT: api/Stationeries/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutStationery(string id, Stationery stationery)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != stationery.ItemNum)
            {
                return BadRequest();
            }

            db.Entry(stationery).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StationeryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Stationeries
        [ResponseType(typeof(Stationery))]
        public async Task<IHttpActionResult> PostStationery(Stationery stationery)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Stationeries.Add(stationery);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (StationeryExists(stationery.ItemNum))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = stationery.ItemNum }, stationery);
        }

        // DELETE: api/Stationeries/5
        [ResponseType(typeof(Stationery))]
        public async Task<IHttpActionResult> DeleteStationery(string id)
        {
            Stationery stationery = await db.Stationeries.FindAsync(id);
            if (stationery == null)
            {
                return NotFound();
            }

            db.Stationeries.Remove(stationery);
            await db.SaveChangesAsync();

            return Ok(stationery);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool StationeryExists(string id)
        {
            return db.Stationeries.Count(e => e.ItemNum == id) > 0;
        }
    }
}