using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using Wg_backend_api.Models;

namespace Wg_backend_api.DTO
{
    public class ArmiesDTO
    {
        public int ArmyId { get; set; }
        public string ArmyName { get; set; }
        public int LocationId { get; set; }
        public string NationId { get; set; }
        public bool IsNaval { get; set; }
        public int TotalStrength { get; set; }
    }
    public class ArmiesInfoDTO
    {
        public int ArmyId { get; set; }
        public string ArmyName { get; set; }
        public string Location { get; set; }
        public string Nation { get; set; }
        public bool IsNaval { get; set; }

        public List<TroopsAgregatedDTO> Units { get; set; }
        public int TotalStrength { get; set; }
    }

    public class UnitTypeDTO
    {
        public int UnitId { get; set; }
        public string Description { get; set; }

        public string UnitName { get; set; }
        public string UnitType { get; set; }
        public int Quantity { get; set; }
        public int Melee { get; set; }
        public int Range { get; set; }
        public int Defense { get; set; }
        public int Speed { get; set; }
        public int Morale { get; set; }
        public bool IsNaval { get; set; }
    }
    public class UnitTypeInfoDTO
    {
        public int UnitId { get; set; }
        public string Description { get; set; }
        public string UnitTypeName { get; set; }
        public int Quantity { get; set; }
        public int Melee { get; set; }
        public int Range { get; set; }
        public int Defense { get; set; }
        public int Speed { get; set; }
        public int Morale { get; set; }
        public bool IsNaval { get; set; }
        public List<ResourceAmountDto> ConsumedResources { get; set; }
        public List<ResourceAmountDto> ProductionCost { get; set; }

    }
    public class TroopDTO
    {
        public int? Id { get; set; }
        public int UnitTypeId { get; set; }
        public int ArmyId { get; set; }
        public int Quantity { get; set; }
    }


    public class TroopInfoDTO
    {
        public int? Id { get; set; }
        public string UnitTypeName { get; set; }
        public int Quantity { get; set; }
    }

    public class TroopsAgregatedDTO
    {
        public int? Id { get; set; }
        public int TroopCount { get; set; }
        public string UnitTypeName { get; set; }
        public int Quantity { get; set; }
    }

    public class RecruitOrderDTO
    {
        public int TroopTypeId { get; set; }
        public int Count { get; set; }
    }

    public class EditOrderDTO
    {
        public int OrderId { get; set; }
        public int NewCount { get; set; }
    }

    public class UnitOrderDTO
    {
        public int? Id { get; set; }
        public int UnitTypeId { get; set; }

        public int NationId { get; set; }

        public int Quantity { get; set; }
    }

    public class UnitOrderInfoDTO
    {
        public int? Id { get; set; }
        public string UnitTypeName { get; set; }
        public int UnitTypeId { get; set; }
        public int Quantity { get; set; }
        public int UsedManpower { get; set; }

    }

    public class ManpowerInfoDTO
    {
        public int TotalMappower { get; set; }
        public int AvailableManpower { get; set; }
        public int RecruitingLandManpower { get; set; }
        public int RecruitingNavalManpower { get; set; }
        public int ManpowerInLandArmies { get; set; }
        public int ManpowerInNavalArmies { get; set; }


    }
}