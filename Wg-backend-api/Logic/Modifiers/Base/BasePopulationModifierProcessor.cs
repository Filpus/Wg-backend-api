using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.Logic.Modifiers.ModifierConditions;
using Wg_backend_api.Models;

namespace Wg_backend_api.Logic.Modifiers.Base
{
    public abstract class BasePopulationProcessor : BaseCachedModifierProcessor<Population, PopulationConditions>
    {
        protected BasePopulationProcessor(GameDbContext context) : base(context) { }

        protected override IQueryable<Population> GetTargetEntities(int nationId, PopulationConditions conditions)
        {
            var query = this._context.Populations
                .Include(p => p.Location)
                .Where(p => p.Location.NationId == nationId);

            if (conditions.CultureId.HasValue)
            {
                query = query.Where(p => p.CultureId == conditions.CultureId.Value);
            }

            if (conditions.SocialGroupId.HasValue)
            {
                query = query.Where(p => p.SocialGroupId == conditions.SocialGroupId.Value);
            }

            if (conditions.ReligionId.HasValue)
            {
                query = query.Where(p => p.ReligionId == conditions.ReligionId.Value);
            }

            return query;
        }

        protected override int GetEntityId(Population entity)
        {
            return entity.Id ?? 0;
        }
    }
}
