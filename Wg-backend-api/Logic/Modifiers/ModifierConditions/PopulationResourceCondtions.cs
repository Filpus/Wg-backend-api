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
            var dict = new Dictionary<string, object> { ["resource_id"] = ResourceId };
            if (CultureId.HasValue) dict["culture_id"] = CultureId.Value;
            if (SocialGroupId.HasValue) dict["social_group_id"] = SocialGroupId.Value;
            if (ReligionId.HasValue) dict["religion_id"] = ReligionId.Value;
            return dict;
        }
    }
}
