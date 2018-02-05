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
    //Authors: Ton That Minh Nhat
    public class StationeriesController : ApiController
    {
        private readonly StationeryRepository _stationeryRepo = new StationeryRepository();

        // GET: api/Stationeries
        [HttpGet]
        [Route("api/Stationeries")]
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
        [HttpGet]
        [Route("api/Stationeries/{id}")]
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stationeryRepo.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}