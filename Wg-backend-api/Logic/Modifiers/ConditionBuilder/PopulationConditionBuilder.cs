using Wg_backend_api.Models;

namespace Wg_backend_api.Logic.Modifiers.ConditionBuilder
{
    public class PopulationConditionBuilder : ConditionBuilder<Population>
    {
        public PopulationConditionBuilder(IQueryable<Population> baseQuery) : base(baseQuery) { }

        public override ConditionBuilder<Population> ApplyConditions(Dictionary<string, object> conditions)
        {
            if (TryGetCondition<int>(conditions, "culture_id", out var cultureId))
            {
                Query = Query.Where(p => p.CultureId == cultureId);
            }
            if (TryGetCondition<int>(conditions, "social_group_id", out var socialGroupId))
            {
                Query = Query.Where(p => p.SocialGroupId == socialGroupId);
            }
            if (TryGetCondition<int>(conditions, "localisation_id", out var localisationId))
            {
                Query = Query.Where(p => p.LocationId == localisationId);
            }
            return this;
        }
    }
}  
