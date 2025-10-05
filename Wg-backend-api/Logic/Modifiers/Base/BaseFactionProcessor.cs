using Wg_backend_api.Data;
using Wg_backend_api.Logic.Modifiers.ModifierConditions;
using Wg_backend_api.Models;

namespace Wg_backend_api.Logic.Modifiers.Base
{
    public abstract class BaseFactionProcessor : BaseCachedModifierProcessor<Faction, FactionConditions>
    {
        protected BaseFactionProcessor(GameDbContext context, ILogger logger) : base(context, logger) { }

        protected override IQueryable<Faction> GetTargetEntities(int nationId, FactionConditions conditions)
        {
            var query = _context.Factions.Where(f => f.NationId == nationId);

            if (conditions.FactionId.HasValue)
                query = query.Where(f => f.Id == conditions.FactionId.Value);

            return query;
        }

        protected override int GetEntityId(Faction entity) => entity.Id ?? 0;
    }
}
