using Wg_backend_api.Enums;
using Wg_backend_api.Logic.Modifiers.Processors;

namespace Wg_backend_api.Logic.Modifiers
{
    public class ModifierProcessorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ModifierProcessorFactory(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public IModifierProcessor GetProcessor(ModifierType type)
        {
            return type switch
            {
                ModifierType.PopulationHappiness => this._serviceProvider.GetRequiredService<PopulationHappinessProcessor>(),
                ModifierType.ResourceProduction => this._serviceProvider.GetRequiredService<PopulationResourceProductionProcessor>(),
                ModifierType.ResouerceUsage => this._serviceProvider.GetRequiredService<PopulationResourceUsageProcessor>(),
                ModifierType.ResourceChange => this._serviceProvider.GetRequiredService<ResourceChangeProcessor>(),
                ModifierType.VoluneerChange => this._serviceProvider.GetRequiredService<PopulationVolunteerProcessor>(),
                ModifierType.FactionPower => this._serviceProvider.GetRequiredService<FactionPowerProcessor>(),
                ModifierType.FactionContenment => this._serviceProvider.GetRequiredService<FactionContentmentProcessor>(),

                // Dodaj kolejne gdy je zaimplementujesz
                _ => throw new NotSupportedException($"Nieobsługiwany typ modyfikatora: {type}")
            };
        }
    }
}
