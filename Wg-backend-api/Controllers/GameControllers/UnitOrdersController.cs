using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Models;
using Wg_backend_api.Services;
namespace Wg_backend_api.Controllers.GameControllers
{
    [Route("api/UnitOrders")]
    [ApiController]
    public class UnitOrdersController : Controller
    {
        private readonly IGameDbContextFactory _gameDbContextFactory;
        private readonly ISessionDataService _sessionDataService;
        private GameDbContext _context;
        private int? _nationId;

        public UnitOrdersController(IGameDbContextFactory gameDbFactory, ISessionDataService sessionDataService)
        {
            _gameDbContextFactory = gameDbFactory;
            _sessionDataService = sessionDataService;

            string schema = _sessionDataService.GetSchema();
            if (string.IsNullOrEmpty(schema))
            {
                throw new InvalidOperationException("Brak schematu w sesji.");
            }
            _context = _gameDbContextFactory.Create(schema);
            string nationIdStr = _sessionDataService.GetNation();
            _nationId = string.IsNullOrEmpty(nationIdStr) ? null : int.Parse(nationIdStr);
        }
        // GET: api/UnitOrders
        // GET: api/UnitOrders/5
        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<UnitOrder>>> GetUnitOrders(int? id)
        {
            if (id.HasValue)
            {
                var unitOrder = await _context.UnitOrders.FindAsync(id);
                if (unitOrder == null)
                {
                    return NotFound();
                }
                return Ok(new List<UnitOrder> { unitOrder });
            }
            else
            {
                return await _context.UnitOrders.ToListAsync();
            }
        }

        // PUT: api/UnitOrders
        [HttpPut]
        public async Task<IActionResult> PutUnitOrders([FromBody] List<UnitOrder> unitOrders)
        {
            if (unitOrders == null || unitOrders.Count == 0)
            {
                return BadRequest("Brak danych do edycji.");
            }

            foreach (var unitOrder in unitOrders)
            {
                _context.Entry(unitOrder).State = EntityState.Modified;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Błąd podczas aktualizacji.");
            }

            return NoContent();
        }

        // POST: api/UnitOrders
        [HttpPost]
        public async Task<ActionResult<UnitOrder>> PostUnitOrders([FromBody] List<UnitOrder> unitOrders)
        {
            if (unitOrders == null || unitOrders.Count == 0)
            {
                return BadRequest("Brak danych do zapisania.");
            }

            foreach (UnitOrder unitOrder in unitOrders)
            {
                unitOrder.Id = null;
            }

            _context.UnitOrders.AddRange(unitOrders);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUnitOrders", new { id = unitOrders[0].Id }, unitOrders);
        }

        // DELETE: api/UnitOrders
        [HttpDelete]
        public async Task<ActionResult> DeleteUnitOrders([FromBody] List<int?> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("Brak ID do usunięcia.");
            }

            var unitOrders = await _context.UnitOrders.Where(r => ids.Contains(r.Id)).ToListAsync();

            if (unitOrders.Count == 0)
            {
                return NotFound("Nie znaleziono zamówień jednostek do usunięcia.");
            }

            _context.UnitOrders.RemoveRange(unitOrders);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("GetUnitOrdersByNationId/{nationId?}")]
        public async Task<ActionResult<IEnumerable<UnitOrderInfoDTO>>> GetNavUnitOrdersByNationId(int? nationId)
        {
            if (nationId == null)
            {
                nationId = _nationId;
            }
            var unitOrders = await _context.UnitOrders
                .Where(uo => uo.NationId == nationId)
                .Select(uo => new UnitOrderInfoDTO
                {
                    Id = uo.Id,
                    UnitTypeName = uo.UnitType.Name,
                    Quantity = uo.Quantity,
                    UsedManpower = uo.Quantity * uo.UnitType.VolunteersNeeded // Calculate used manpower  

                })
                .ToListAsync();

            return Ok(unitOrders);
        }


        [HttpGet("GetNavalUnitOrdersByNationId/{nationId?}")]
        public async Task<ActionResult<IEnumerable<UnitOrderInfoDTO>>> GetNavalUnitOrdersByNationId(int? nationId)
        {
            if (nationId == null)
            {
                nationId = _nationId;
            }
            var navalUnitOrders = await _context.UnitOrders
                .Where(uo => uo.NationId == nationId && uo.UnitType.IsNaval) // Assuming UnitType has an IsNaval property
                .Select(uo => new UnitOrderInfoDTO
                {
                    Id = uo.Id,
                    UnitTypeName = uo.UnitType.Name,
                    Quantity = uo.Quantity,
                    UsedManpower = uo.Quantity * uo.UnitType.VolunteersNeeded // Calculate used manpower  

                })
                .ToListAsync();

            return Ok(navalUnitOrders);
        }

        [HttpGet("GetLandUnitOrdersByNationId/{nationId?}")]
        public async Task<ActionResult<IEnumerable<UnitOrderInfoDTO>>> GetLandUnitOrdersByNationId(int? nationId)
        {
            if (nationId == null)
            {
                nationId = _nationId;
            }
            var landUnitOrders = await _context.UnitOrders
                .Where(uo => uo.NationId == nationId && !uo.UnitType.IsNaval) // Assuming UnitType has an IsNaval property
                .Select(uo => new UnitOrderInfoDTO
                {
                    Id = uo.Id,
                    UnitTypeName = uo.UnitType.Name,
                    Quantity = uo.Quantity,
                    UsedManpower = uo.Quantity * uo.UnitType.VolunteersNeeded // Calculate used manpower  

                })
                .ToListAsync();

            return Ok(landUnitOrders);
        }



        [HttpPost("AddRecruitOrder/{nationId?}")]
        public async Task<IActionResult> AddRecruitOrder(int? nationId, [FromBody] RecruitOrderDTO recruitOrder)
        {

            if ( nationId == null)
            {
                nationId = _nationId;
            }

            if (recruitOrder == null || recruitOrder.Count <= 0)
            {
                return BadRequest("Nieprawidłowe dane zlecenia rekrutacji.");
            }

            var nationExists = await _context.Nations.AnyAsync(n => n.Id == nationId);
            if (!nationExists)
            {
                return NotFound("Nie znaleziono państwa o podanym ID.");
            }

            var unitTypeExists = await _context.UnitTypes.AnyAsync(ut => ut.Id == recruitOrder.TroopTypeId);
            if (!unitTypeExists)
            {
                return NotFound("Nie znaleziono typu jednostki o podanym ID.");
            }

            var newUnitOrder = new UnitOrder
            {
                NationId = (int)nationId,
                UnitTypeId = recruitOrder.TroopTypeId,
                Quantity = recruitOrder.Count
            };

            _context.UnitOrders.Add(newUnitOrder);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUnitOrders", new { id = newUnitOrder.Id }, newUnitOrder);
        }

        [HttpPatch("UpdateRecruitmentCount")]
        public async Task<IActionResult> UpdateRecruitmentCount([FromBody] EditOrderDTO editOrder)
        {
            if (editOrder == null || editOrder.OrderId <= 0 || editOrder.NewCount < 0)
            {
                return BadRequest("Nieprawidłowe dane zlecenia edycji.");
            }

            var unitOrder = await _context.UnitOrders.FindAsync(editOrder.OrderId);
            if (unitOrder == null)
            {  
                return NotFound("Nie znaleziono zamówienia jednostek o podanym ID.");
            }

            unitOrder.Quantity = editOrder.NewCount;

            _context.Entry(unitOrder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Błąd podczas aktualizacji.");
            }

            return Ok(unitOrder);
        }



    }
}
