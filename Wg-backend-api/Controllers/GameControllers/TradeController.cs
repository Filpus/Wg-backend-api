using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.Models;
using Wg_backend_api.Services;

namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TradeController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;

        public TradeController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
        {
            _gameDbContextFactory = gameDbFactory;
            _sessionDataService = sessionDataService;

            string schema = _sessionDataService.GetSchema();
            if (string.IsNullOrEmpty(schema))
            {
                throw new InvalidOperationException("Brak schematu w sesji.");
            }
            _context = _gameDbContextFactory.Create(schema);
        }

        [HttpPost("TradeAgreement")]
        public async Task<ActionResult<TradeAgreement>> PostTradeAgreement([FromBody] TradeAgreement tradeAgreement)
        {
            if (tradeAgreement == null)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            tradeAgreement.Id = null;
            _context.TradeAgreements.Add(tradeAgreement);
            await _context.SaveChangesAsync();

            var latestTradeAgreement = await _context.TradeAgreements
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

            _context.WantedResources.AddRange(wantedResources);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetWantedResources", new { id = wantedResources[0].Id }, wantedResources);
        }
    }
}
