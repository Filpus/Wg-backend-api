using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.Enums;
using Wg_backend_api.Logic.Modifires.ConditionBuilder;
using Wg_backend_api.Models;

namespace Wg_backend_api.Logic.Modifires.Processors
{ 
public class PopulationHappinessProcessor : BaseModifierProcessor<Population>
{
    public override ModifierType SupportedType => ModifierType.PopulationHappiness;

    public PopulationHappinessProcessor(GameDbContext context) : base(context) { }

        protected override IQueryable<Population> GetBaseQuery(int nationId)
        {
            // Poprawione: Include nawigacji Location, nie LocationId
            return _context.Populations
                .Include(p => p.Location)
                .Where(p => p.Location != null && p.Location.NationId == nationId);
        }

    protected override ConditionBuilder<Population> CreateConditionBuilder(IQueryable<Population> baseQuery)
    {
        return new PopulationConditionBuilder(baseQuery);
    }

    protected override ModifierChangeRecord ApplyToEntity(Population entity, ModifierEffect effect)
    {
        var operation = Enum.Parse<ModifierOperation>(effect.Operation, ignoreCase: true);
        
        float oldValue = entity.Happiness;
            entity.Happiness = OperationProcessor.ApplyOperation(
            entity.Happiness,
            (float)effect.Value,
            operation
        );

        // Clamp 0-100
        entity.Happiness = Math.Max(0, Math.Min(100, entity.Happiness));
        return new ModifierChangeRecord
        {
            EntityId = (int)entity.Id,
            EntityType = nameof(Population),
            PropertyName = nameof(entity.Happiness),
            OldValue = oldValue,
            NewValue = entity.Happiness,
            Change = entity.Happiness - oldValue,
        };
        }

    protected override int GetEntityId(Population entity) => (int)entity.Id;
}
}
