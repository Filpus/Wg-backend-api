using Wg_backend_api.Data;
using Wg_backend_api.Models;

namespace Wg_backend_api.Logic.Modifiers
{
    public class ModifierManager
    {
        private readonly GameDbContext _context;
        private readonly ModifierProcessorFactory _factory;

        public ModifierManager(GameDbContext context, ModifierProcessorFactory factory)
        {
            _context = context;
            _factory = factory;
        }

        public async Task<List<ModifierApplicationResult>> ApplyEventToNationAsync(int eventId, int nationId)
        {
            var results = new List<ModifierApplicationResult>();

            // TO DO: Implementacja gdy będziesz mieć tabele wydarzeń z modyfikatorami

            return results;
        }
    }
}
