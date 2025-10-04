using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.Enums;
using Wg_backend_api.Logic.Modifiers.Base;
using Wg_backend_api.Logic.Modifiers.ConditionBuilder;
using Wg_backend_api.Models;

namespace Wg_backend_api.Logic.Modifiers.Processors
{ 
public class PopulationHappinessProcessor : BasePopulationProcessor
{
        public override ModifierType SupportedType => ModifierType.PopulationHappiness;

        public PopulationHappinessProcessor(GameDbContext context, ILogger<PopulationHappinessProcessor> logger)
            : base(context, logger) { }

        protected override void ApplyToEntity(Population entity, ModifierOperation operation, float value)
        {
            var oldValue = entity.Happiness;
            entity.Happiness = OperationProcessor.ApplyOperation(entity.Happiness, value, operation);
            entity.Happiness = Math.Max(0, Math.Min(100, entity.Happiness)); // Clamp 0-100

            _logger?.LogDebug($"Population {entity.Id}: Happiness {oldValue} → {entity.Happiness}");
        }

        protected override void RevertFromEntity(Population entity, ModifierOperation operation, float value)
        {
            var oldValue = entity.Happiness;
            entity.Happiness = OperationProcessor.ReverseOperation(entity.Happiness, value, operation);
            entity.Happiness = Math.Max(0, Math.Min(100, entity.Happiness));

            _logger?.LogDebug($"Population {entity.Id}: Happiness reverted {oldValue} → {entity.Happiness}");
        }
    }

}
