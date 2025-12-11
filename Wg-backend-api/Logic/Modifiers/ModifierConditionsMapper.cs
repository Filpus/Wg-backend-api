using System;
using System.Collections.Generic;
using Wg_backend_api.Enums;
using Wg_backend_api.Logic.Modifiers.Interfaces;
using Wg_backend_api.Logic.Modifiers.ModifierConditions;

namespace Wg_backend_api.Logic.Modifiers
{
    /// <summary>
    /// Maps ModifierTypes to their corresponding Conditions types
    /// </summary>
    public static class ModifierConditionsMapper
    {
        /// <summary>
        /// Returns the Type of Conditions for a given ModifierType
        /// </summary>
        public static Type GetConditionsType(ModifierType modifierType)
        {
            return modifierType switch
            {
                ModifierType.PopulationHappiness => typeof(PopulationConditions),
                ModifierType.VoluneerChange => typeof(PopulationConditions),
                ModifierType.ResourceProduction => typeof(PopulationResourceConditions),
                ModifierType.ResouerceUsage => typeof(PopulationResourceConditions),
                ModifierType.ResourceChange => typeof(ResourceConditions),
                ModifierType.FactionPower => typeof(FactionConditions),
                ModifierType.FactionContenment => typeof(FactionConditions),
                _ => throw new NotSupportedException($"ModifierType {modifierType} is not supported")
            };
        }

        /// <summary>
        /// Creates an instance of Conditions for a given ModifierType from a dictionary
        /// </summary>
        public static IBaseModifierConditions CreateConditions(ModifierType modifierType, Dictionary<string, object> data)
        {
            return modifierType switch
            {
                ModifierType.PopulationHappiness => CreatePopulationConditions(data),
                ModifierType.VoluneerChange => CreatePopulationConditions(data),
                ModifierType.ResourceProduction => CreatePopulationResourceConditions(data),
                ModifierType.ResouerceUsage => CreatePopulationResourceConditions(data),
                ModifierType.ResourceChange => CreateResourceConditions(data),
                ModifierType.FactionPower => CreateFactionConditions(data),
                ModifierType.FactionContenment => CreateFactionConditions(data),
                _ => throw new NotSupportedException($"ModifierType {modifierType} is not supported")
            };
        }

        private static PopulationConditions CreatePopulationConditions(Dictionary<string, object> data)
        {
            var conditions = new PopulationConditions();
            
            if (data.TryGetValue("CultureId", out var cultureId))
                conditions.CultureId = Convert.ToInt32(cultureId);
            if (data.TryGetValue("SocialGroupId", out var socialGroupId))
                conditions.SocialGroupId = Convert.ToInt32(socialGroupId);
            if (data.TryGetValue("ReligionId", out var religionId))
                conditions.ReligionId = Convert.ToInt32(religionId);
                
            return conditions;
        }

        private static PopulationResourceConditions CreatePopulationResourceConditions(Dictionary<string, object> data)
        {
            var conditions = new PopulationResourceConditions();
            
            if (data.TryGetValue("CultureId", out var cultureId))
                conditions.CultureId = Convert.ToInt32(cultureId);
            if (data.TryGetValue("SocialGroupId", out var socialGroupId))
                conditions.SocialGroupId = Convert.ToInt32(socialGroupId);
            if (data.TryGetValue("ReligionId", out var religionId))
                conditions.ReligionId = Convert.ToInt32(religionId);
            if (data.TryGetValue("ResourceId", out var resourceId))
                conditions.ResourceId = Convert.ToInt32(resourceId);
                
            return conditions;
        }

        private static ResourceConditions CreateResourceConditions(Dictionary<string, object> data)
        {
            var conditions = new ResourceConditions();
            
            if (data.TryGetValue("ResourceId", out var resourceId))
                conditions.ResourceId = Convert.ToInt32(resourceId);
                
            return conditions;
        }

        private static FactionConditions CreateFactionConditions(Dictionary<string, object> data)
        {
            var conditions = new FactionConditions();
            
            if (data.TryGetValue("FactionId", out var factionId))
                conditions.FactionId = Convert.ToInt32(factionId);
                
            return conditions;
        }
    }
}
