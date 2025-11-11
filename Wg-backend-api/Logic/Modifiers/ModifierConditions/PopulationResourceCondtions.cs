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
            var dict = new Dictionary<string, object> { ["ResourceId"] = this.ResourceId };
            if (this.CultureId.HasValue)
            {
                dict["CultureId"] = this.CultureId.Value;
            }

            if (this.SocialGroupId.HasValue)
            {
                dict["SocialGroupId"] = this.SocialGroupId.Value;
            }

            if (this.ReligionId.HasValue)
            {
                dict["ReligionId"] = this.ReligionId.Value;
            }

            return dict;
        }
    }
}
