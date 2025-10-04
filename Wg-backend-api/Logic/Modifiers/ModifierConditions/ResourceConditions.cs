using Wg_backend_api.Logic.Modifiers.Interfaces;

namespace Wg_backend_api.Logic.Modifiers.ModifierConditions
{
    public class ResourceConditions : IBaseModifierConditions
    {
        public int ResourceId { get; set; }


        public override Dictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object> { ["resource_id"] = ResourceId };
            return dict;
        }
    }

}
