using Wg_backend_api.Logic.Modifiers.Interfaces;

namespace Wg_backend_api.Logic.Modifiers.ModifierConditions
{
    public class PopulationConditions : IBaseModifierConditions
    {
        public int? CultureId { get; set; }
        public int? SocialGroupId { get; set; }
        public int? ReligionId { get; set; }



        public override Dictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>();
            if (CultureId.HasValue) dict["CultureId"] = CultureId.Value;
            if (SocialGroupId.HasValue) dict["SocialGroupId"] = SocialGroupId.Value;
            if (ReligionId.HasValue) dict["ReligionId"] = ReligionId.Value;
            return dict;
        }
    }

}
