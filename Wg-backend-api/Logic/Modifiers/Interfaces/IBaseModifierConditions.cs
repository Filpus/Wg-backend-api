using System.Text.Json.Serialization;
using Wg_backend_api.Logic.Modifiers.ModifierConditions;

namespace Wg_backend_api.Logic.Modifiers.Interfaces
{

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(ResourceConditions), "resource")]
    [JsonDerivedType(typeof(PopulationConditions), "population")]
    [JsonDerivedType(typeof(PopulationResourceConditions), "population_resource")]
    [JsonDerivedType(typeof(FactionConditions), "faction")]
    public abstract class IBaseModifierConditions
    {
        public abstract Dictionary<string, object> ToDictionary();
    }

}
