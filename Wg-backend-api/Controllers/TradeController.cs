using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.Models;

namespace Wg_backend_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TradeController : Controller
    {
        private readonly AppDbContext _context;
        public TradeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("TradeAgreement")]
        public async Task<ActionResult<TradeAgreement>> PostTradeAgreement([FromBody] TradeAgreement tradeAgreement)
        {
            if (tradeAgreement == null)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            tradeAgreement.Id = null;
            _context.TradeAgreement.Add(tradeAgreement);
            await _context.SaveChangesAsync();

            var latestTradeAgreement = await _context.TradeAgreement
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync();

            

            return Ok(latestTradeAgreement?.Id);
        }

        [HttpPost("OfferedResources")]
        public async Task<ActionResult<IEnumerable<OfferedResource>>> PostOfferedResources([FromBody] List<OfferedResource> offeredResources)
        {
            if (offeredResources == null || offeredResources.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            foreach (var resource in offeredResources)
            {
                resource.Id = null;
            }

            _context.OfferedResources.AddRange(offeredResources);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOfferedResources", new { id = offeredResources[0].Id }, offeredResources);
        }

        [HttpPost("WantedResources")]
        public async Task<ActionResult<IEnumerable<WantedResource>>> PostWantedResources([FromBody] List<WantedResource> wantedResources)
        {
            if (wantedResources == null || wantedResources.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            foreach (var resource in wantedResources)
            {
                resource.Id = null;
            }

            _context.WantedResource.AddRange(wantedResources);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetWantedResources", new { id = wantedResources[0].Id }, wantedResources);
        }
    }
}
