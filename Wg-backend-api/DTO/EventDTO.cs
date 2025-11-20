using Wg_backend_api.Enums;
using Wg_backend_api.Logic.Modifiers.Interfaces;

namespace Wg_backend_api.DTO
{
    public class ModifierDto
    {
        public int? ModifierId { get; set; }
        public ModifierType ModifierType { get; set; }
        public ModifierEffectDto Effect { get; set; }
        public int EffectCount { get; set; }
    }

    public class ModifierEffectDto
    {
        public ModifierOperation Operation { get; set; }
        public decimal Value { get; set; }
        public IBaseModifierConditions Conditions { get; set; }
    }

    public class EventDto
    {
        public int? EventId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool? IsActive { get; set; }

        public List<ModifierDto> Modifiers { get; set; } = [];
    }
    public class AssignEventDto
    {
        public int EventId { get; set; }
        public int? NationId { get; set; }
    }

    public class AssignEventInfoDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public string? EventDescription { get; set; }
        public int NationId { get; set; }
        public string NationName { get; set; }
    }

    public class OptionPackDTO
    {
        public List<ResourceDto> Resources { get; set; }
        public List<ReligionDTO> Religions { get; set; }
        public List<CultureDTO> Cultures { get; set; }
        public List<SocialGroupInfoDTO> SocialGroups { get; set; }
        public List<FactionDTO> Factions { get; set; }
    }

}
