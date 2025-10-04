using System.Text.Json;
using Wg_backend_api.Enums;
using Wg_backend_api.Logic.Modifiers.Interfaces;
using Wg_backend_api.Logic.Modifiers.ModifierConditions;

namespace Wg_backend_api.Logic.Modifiers
{
    public static class ModifierConditionsMapper
    {
        public static Type GetConditionsType(ModifierType modifierType)
        {
            return modifierType switch
            {
                // Tylko populacja
                ModifierType.PopulationHappiness => typeof(PopulationConditions),
                ModifierType.VoluneerChange => typeof(PopulationConditions),

                // Populacja + zasób
                ModifierType.ResourceProduction => typeof(PopulationResourceConditions),
                ModifierType.ResouerceUsage => typeof(PopulationResourceConditions),

                // Tylko zasób
                ModifierType.ResourceChange => typeof(ResourceConditions),

                // Tylko frakcja
                ModifierType.FactionPower => typeof(FactionConditions),
                ModifierType.FactionContenment => typeof(FactionConditions),

                _ => throw new NotSupportedException($"Nieobsługiwany typ modyfikatora: {modifierType}")
            };
        }

        public static IBaseModifierConditions CreateConditions(ModifierType modifierType, Dictionary<string, object> conditionsDict)
        {
            var json = JsonSerializer.Serialize(conditionsDict);

            return modifierType switch
            {
                ModifierType.PopulationHappiness => JsonSerializer.Deserialize<PopulationConditions>(json),
                ModifierType.VoluneerChange => JsonSerializer.Deserialize<PopulationConditions>(json),

                ModifierType.ResourceProduction => JsonSerializer.Deserialize<PopulationResourceConditions>(json),
                ModifierType.ResouerceUsage => JsonSerializer.Deserialize<PopulationResourceConditions>(json),

                ModifierType.ResourceChange => JsonSerializer.Deserialize<ResourceConditions>(json),

                ModifierType.FactionPower => JsonSerializer.Deserialize<FactionConditions>(json),
                ModifierType.FactionContenment => JsonSerializer.Deserialize<FactionConditions>(json),

                _ => throw new NotSupportedException($"Nieobsługiwany typ modyfikatora: {modifierType}")
            };
        }
    }

}
