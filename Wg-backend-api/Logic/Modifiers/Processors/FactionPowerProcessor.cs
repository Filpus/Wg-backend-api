using Wg_backend_api.Data;
using Wg_backend_api.Enums;
using Wg_backend_api.Logic.Modifiers.Base;
using Wg_backend_api.Models;

namespace Wg_backend_api.Logic.Modifiers.Processors
{
    public class FactionPowerProcessor : BaseFactionProcessor
    {
        public override ModifierType SupportedType => ModifierType.FactionPower;

        public FactionPowerProcessor(GameDbContext context, ILogger<FactionPowerProcessor> logger)
            : base(context, logger) { }

        protected override void ApplyToEntity(Faction entity, ModifierOperation operation, float value)
        {
            var oldValue = entity.Power;
            entity.Power = (int)OperationProcessor.ApplyOperation(entity.Power, value, operation);

            _logger?.LogDebug($"Faction {entity.Id}: Power {oldValue} → {entity.Power}");
        }

        protected override void RevertFromEntity(Faction entity, ModifierOperation operation, float value)
        {
            var oldValue = entity.Power;
            entity.Power = (int)OperationProcessor.ReverseOperation(entity.Power, (float)value, operation);
            entity.Power = Math.Max(0, Math.Min(100, entity.Power));

            _logger?.LogDebug($"Faction {entity.Id}: Power reverted {oldValue} → {entity.Power}");
        }
    }

    public class FactionContentmentProcessor : BaseFactionProcessor
    {
        public override ModifierType SupportedType => ModifierType.FactionContenment;

        public FactionContentmentProcessor(GameDbContext context, ILogger<FactionPowerProcessor> logger)
            : base(context, logger) { }

        protected override void ApplyToEntity(Faction entity, ModifierOperation operation, float value)
        {
            var oldValue = entity.Contentment;
            entity.Contentment = (int)OperationProcessor.ApplyOperation(entity.Contentment, (float)value, operation);

            _logger?.LogDebug($"Faction {entity.Id}: Power {oldValue} → {entity.Contentment}");
        }

        protected override void RevertFromEntity(Faction entity, ModifierOperation operation, float value)
        {
            var oldValue = entity.Contentment;
            entity.Contentment = (int)OperationProcessor.ReverseOperation(entity.Contentment, value, operation);
            entity.Contentment = Math.Max(0, Math.Min(100, entity.Contentment));

            _logger?.LogDebug($"Faction {entity.Id}: Power reverted {oldValue} → {entity.Contentment}");
        }
    }
}
