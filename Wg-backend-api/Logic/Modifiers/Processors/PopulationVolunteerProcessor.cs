using Wg_backend_api.Data;
using Wg_backend_api.Enums;
using Wg_backend_api.Logic.Modifiers.Base;
using Wg_backend_api.Models;

namespace Wg_backend_api.Logic.Modifiers.Processors
{
    public class PopulationVolunteerProcessor : BasePopulationProcessor
    {
        public override ModifierType SupportedType => ModifierType.VoluneerChange;

        public PopulationVolunteerProcessor(GameDbContext context, ILogger<PopulationVolunteerProcessor> logger)
            : base(context) { }

        protected override void ApplyToEntity(Population entity, ModifierOperation operation, float value)
        {
            var oldValue = entity.Volunteers;
            entity.Volunteers = (int)OperationProcessor.ApplyOperation(entity.Volunteers, value, operation);
            entity.Volunteers = Math.Max(0, entity.Volunteers);

        }

        protected override void RevertFromEntity(Population entity, ModifierOperation operation, float value)
        {
            var oldValue = entity.Volunteers;
            entity.Volunteers = (int)OperationProcessor.ReverseOperation(entity.Volunteers, value, operation);
            entity.Volunteers = Math.Max(0, entity.Volunteers);

        }

    }
}
