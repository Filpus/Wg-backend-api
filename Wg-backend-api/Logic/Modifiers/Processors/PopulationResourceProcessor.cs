using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.Enums;
using Wg_backend_api.Logic.Modifiers.Base;
using Wg_backend_api.Logic.Modifiers.ModifierConditions;
using Wg_backend_api.Models;

namespace Wg_backend_api.Logic.Modifiers.Processors
{
    public class PopulationResourceProductionProcessor : BaseCachedModifierProcessor<PopulationProductionShare, PopulationResourceConditions>
    {
        public override ModifierType SupportedType => ModifierType.ResourceProduction;

        public PopulationResourceProductionProcessor(GameDbContext context, ILogger<PopulationResourceProductionProcessor> logger)
            : base(context, logger) { }

        protected override IQueryable<PopulationProductionShare> GetTargetEntities(int nationId, PopulationResourceConditions conditions)
        {
            var query = _context.PopulationProductionShares
                .Include(pps => pps.Population)
                .Include(pps => pps.Population.Location)
                .Where(pps => pps.Population.Location.NationId == nationId)
                .Where(pps => pps.ResourcesId == conditions.ResourceId);

            if (conditions.CultureId.HasValue)
                query = query.Where(pps => pps.Population.CultureId == conditions.CultureId.Value);
            if (conditions.SocialGroupId.HasValue)
                query = query.Where(pps => pps.Population.SocialGroupId == conditions.SocialGroupId.Value);
            if (conditions.ReligionId.HasValue)
                query = query.Where(pps => pps.Population.ReligionId == conditions.ReligionId.Value);

            return query;
        }

        protected override void ApplyToEntity(PopulationProductionShare entity, ModifierOperation operation, float value)
        {
            var oldValue = entity.Coefficient;
            entity.Coefficient = OperationProcessor.ApplyOperation(entity.Coefficient, value, operation);
            entity.Coefficient = Math.Max(0, entity.Coefficient);

            _logger?.LogDebug($"PopulationProductionShare {entity.Id}: Coefficient {oldValue} → {entity.Coefficient} (Resource: {entity.ResourcesId})");
        }

        protected override void RevertFromEntity(PopulationProductionShare entity, ModifierOperation operation, float value)
        {
            var oldValue = entity.Coefficient;
            entity.Coefficient = OperationProcessor.ReverseOperation(entity.Coefficient, value, operation);
            entity.Coefficient = Math.Max(0, entity.Coefficient);

            _logger?.LogDebug($"PopulationProductionShare {entity.Id}: Coefficient reverted {oldValue} → {entity.Coefficient} (Resource: {entity.ResourcesId})");
        }

        protected override int GetEntityId(PopulationProductionShare entity) => entity.Id ?? 0;
    }

    public class PopulationResourceUsageProcessor : BaseCachedModifierProcessor<PopulationUsedResource, PopulationResourceConditions>
    {
        public override ModifierType SupportedType => ModifierType.ResouerceUsage;

        public PopulationResourceUsageProcessor(GameDbContext context, ILogger<PopulationResourceUsageProcessor> logger)
            : base(context, logger) { }

        protected override IQueryable<PopulationUsedResource> GetTargetEntities(int nationId, PopulationResourceConditions conditions)
        {
            var query = _context.populationUsedResources
                .Include(pur => pur.Population)
                .Include(pur => pur.Population.Location)
                .Where(pur => pur.Population.Location.NationId == nationId)
                .Where(pur => pur.ResourcesId == conditions.ResourceId);

            if (conditions.CultureId.HasValue)
                query = query.Where(pur => pur.Population.CultureId == conditions.CultureId.Value);
            if (conditions.SocialGroupId.HasValue)
                query = query.Where(pur => pur.Population.SocialGroupId == conditions.SocialGroupId.Value);
            if (conditions.ReligionId.HasValue)
                query = query.Where(pur => pur.Population.ReligionId == conditions.ReligionId.Value);


            return query;
        }

        protected override void ApplyToEntity(PopulationUsedResource entity, ModifierOperation operation, float value)
        {
            var oldValue = entity.Amount;
            entity.Amount = OperationProcessor.ApplyOperation(entity.Amount, value, operation);
            entity.Amount = Math.Max(0, entity.Amount);

            _logger?.LogDebug($"PopulationUsedResource {entity.Id}: Amount {oldValue} → {entity.Amount} (Resource: {entity.ResourcesId})");
        }

        protected override void RevertFromEntity(PopulationUsedResource entity, ModifierOperation operation, float value)
        {
            var oldValue = entity.Amount;
            entity.Amount = OperationProcessor.ReverseOperation(entity.Amount, (float)value, operation);
            entity.Amount = Math.Max(0, entity.Amount);

            _logger?.LogDebug($"PopulationUsedResource {entity.Id}: Amount reverted {oldValue} → {entity.Amount} (Resource: {entity.ResourcesId})");
        }

        protected override int GetEntityId(PopulationUsedResource entity) => entity.Id ?? 0;
    }
}
