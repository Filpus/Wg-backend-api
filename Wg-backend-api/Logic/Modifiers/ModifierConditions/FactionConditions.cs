using Wg_backend_api.Logic.Modifiers.Interfaces;

namespace Wg_backend_api.Logic.Modifiers.ModifierConditions
{
    public class FactionConditions : IBaseModifierConditions
    {
        public int? FactionId { get; set; } 

 
        public override Dictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>();
            if (FactionId.HasValue) dict["FactionId"] = FactionId.Value;
            return dict;
        }
    }
}
