using Wg_backend_api.Enums;
using Wg_backend_api.Logic.Modifiers.Processors;

namespace Wg_backend_api.Logic.Modifiers
{
    public class ModifierProcessorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ModifierProcessorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IModifierProcessor GetProcessor(ModifierType type)
        {
            return type switch
            {
                ModifierType.PopulationHappiness => _serviceProvider.GetRequiredService<PopulationHappinessProcessor>(),
                // Dodaj kolejne gdy je zaimplementujesz
                _ => throw new NotSupportedException($"Nieobsługiwany typ modyfikatora: {type}")
            };
        }
    }
}
