using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
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
        private int? _nationId;


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
            _nationId = _sessionDataService.GetNation() != null ? int.Parse(_sessionDataService.GetNation()) : null;
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

        [HttpGet("OfferedTradeAgreements/{nationId?}")]
        public async Task<ActionResult<IEnumerable<TradeAgreementInfoDTO>>> GetOfferedTradeAgreements(int? nationId)
        {
            if (nationId == null)
            {
                nationId = _nationId;
            }
            var tradeAgreements = await _context.TradeAgreements
                .Where(t => t.OferingNationId == nationId)
                .Select(t => new TradeAgreementInfoDTO
                {
                    Id = t.Id,
                    OfferingNationName = _context.Nations.FirstOrDefault(n => n.Id == t.OferingNationId).Name,
                    ReceivingNationName = _context.Nations.FirstOrDefault(n => n.Id == t.ReceivingNationId).Name,
                    IsActive = t.isAccepted,
                    Duration = t.Duration, // Assuming duration is not stored in the database  
                    OfferedResources = t.OfferedResources.Select(r => new ResourceAmountDto
                    {
                        ResourceId = r.ResourceId,
                        ResourceName = _context.Resources.FirstOrDefault(res => res.Id == r.ResourceId).Name,
                        Amount = r.Quantity
                    }).ToList(),
                    RequestedResources = t.WantedResources.Select(r => new ResourceAmountDto
                    {
                        ResourceId = r.ResourceId,
                        ResourceName = _context.Resources.FirstOrDefault(res => res.Id == r.ResourceId).Name,
                        Amount = r.Amount
                    }).ToList()
                })
                .ToListAsync();

            return Ok(tradeAgreements);
        }

        [HttpGet("ReceivedTradeAgreements/{nationId?}")]
        public async Task<ActionResult<IEnumerable<TradeAgreementInfoDTO>>> GetReceivedTradeAgreements(int? nationId)
        {
            if (nationId == null)
            {
                nationId = _nationId;
            }
            var tradeAgreements = await _context.TradeAgreements
                .Where(t => t.ReceivingNationId == nationId)
                .Select(t => new TradeAgreementInfoDTO
                {
                    Id = t.Id,
                    OfferingNationName = _context.Nations.FirstOrDefault(n => n.Id == t.OferingNationId).Name,
                    ReceivingNationName = _context.Nations.FirstOrDefault(n => n.Id == t.ReceivingNationId).Name,
                    IsActive = t.isAccepted,
                    Duration = t.Duration, // Assuming duration is not stored in the database  
                    OfferedResources = t.OfferedResources.Select(r => new ResourceAmountDto
                    {
                        ResourceId = r.ResourceId,
                        ResourceName = _context.Resources.FirstOrDefault(res => res.Id == r.ResourceId).Name,
                        Amount = r.Quantity
                    }).ToList(),
                    RequestedResources = t.WantedResources.Select(r => new ResourceAmountDto
                    {
                        ResourceId = r.ResourceId,
                        ResourceName = _context.Resources.FirstOrDefault(res => res.Id == r.ResourceId).Name,
                        Amount = r.Amount
                    }).ToList()
                })
                .ToListAsync();

            return Ok(tradeAgreements);
        }

        [HttpPost("CreateTradeAgreementWithResources/{offeringNationId?}")]
        public async Task<ActionResult<int>> CreateTradeAgreementWithResources(int? offeringNationId, [FromBody] OfferTradeAgreementDTO offerTradeAgreementDTO)
        {
            if (offeringNationId == null)
            {
                offeringNationId = _nationId;
            }

            if (offerTradeAgreementDTO == null || (offerTradeAgreementDTO.offeredResources.Count == 0 && offerTradeAgreementDTO.requestedResources.Count == 0))
            {
                return BadRequest("Brak danych do zapisania.");
            }

            // Tworzenie nowej umowy handlowej
            var tradeAgreement = new TradeAgreement
            {
                OferingNationId = (int)offeringNationId,
                ReceivingNationId = offerTradeAgreementDTO.receivingNationId,
                OfferedResources = new List<OfferedResource>(),
                WantedResources = new List<WantedResource>()
            };



            // Dodanie oferowanych zasobów z przypisaniem ID umowy
            // Po zapisaniu obiektu w bazie danych, właściwość Id jest automatycznie przypisana.  
            _context.TradeAgreements.Add(tradeAgreement);
            await _context.SaveChangesAsync();

            // W tym momencie tradeAgreement.Id zawiera wartość wygenerowaną przez bazę danych.  
            if (tradeAgreement.Id.HasValue)
            {
                Console.WriteLine($"Wygenerowane ID: {tradeAgreement.Id.Value}");
            }
            else
            {
                Console.WriteLine("ID nie zostało przypisane.");
            }
            foreach (var resource in offerTradeAgreementDTO.offeredResources)
            {
                tradeAgreement.OfferedResources.Add(new OfferedResource
                {
                    ResourceId = resource.ResourceId,
                    TradeAgreementId = tradeAgreement.Id.Value,
                    Quantity = resource.Amount,
                });
            }

            // Dodanie chcianych zasobów z przypisaniem ID umowy
            foreach (var resource in offerTradeAgreementDTO.requestedResources)
            {
                tradeAgreement.WantedResources.Add(new WantedResource
                {
                    ResourceId = resource.ResourceId,
                    TradeAgreementId = tradeAgreement.Id.Value,
                    Amount = resource.Amount,
                });
            }

            // Zapisanie zasobów w bazie danych
            _context.OfferedResources.AddRange(tradeAgreement.OfferedResources);
            _context.WantedResources.AddRange(tradeAgreement.WantedResources);
            await _context.SaveChangesAsync();

            return Ok(tradeAgreement.Id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTradeAgreement(int id)
        {
            var tradeAgreement = await _context.TradeAgreements.FindAsync(id);
            if (tradeAgreement == null)
            {
                return NotFound(new { error = "Umowa handlowa nie została znaleziona." });
            }
            _context.TradeAgreements.Remove(tradeAgreement);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("AcceptTrade/{id}")]
        public async Task<IActionResult> AcceptTrade(int id) {
            var tradeAgreement = await _context.TradeAgreements.FindAsync(id);
            if (tradeAgreement == null)
            {
                return NotFound(new {error =  "Umowa handlowa nie została znaleziona."});
            }
            if (tradeAgreement.isAccepted)
            {
                return BadRequest(new { error = "Umowa handlowa została już zaakceptowana." });
            }
            tradeAgreement.isAccepted = true;
            _context.TradeAgreements.Update(tradeAgreement);
            await _context.SaveChangesAsync();
            return Ok(new { message =  "Umowa handlowa została zaakceptowana."});
        }
    }
}

