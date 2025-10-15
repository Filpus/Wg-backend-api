using Wg_backend_api.Logic.Modifiers.Interfaces;

namespace Wg_backend_api.Logic.Modifiers.ModifierConditions
{
    public class PopulationResourceConditions : IBaseModifierConditions
    {
        public int ResourceId { get; set; }
        public int? CultureId { get; set; }
        public int? SocialGroupId { get; set; }
        public int? ReligionId { get; set; }


        public override Dictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object> { ["ResourceId"] = ResourceId };
            if (CultureId.HasValue) dict["CultureId"] = CultureId.Value;
            if (SocialGroupId.HasValue) dict["SocialGroupId"] = SocialGroupId.Value;
            if (ReligionId.HasValue) dict["ReligionId"] = ReligionId.Value;
            return dict;
        }
    }
}
