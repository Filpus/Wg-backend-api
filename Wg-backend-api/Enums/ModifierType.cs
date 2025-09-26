using System.ComponentModel;

namespace Wg_backend_api.Enums
{
    public enum ModifierType
    {
        [Description("Zadowolenie populacji")]
        PopulationHappiness,

        [Description("Produkcja zasobów")]
        ResourceProduction,

        [Description("Dodanie zasobów")]
        ResourceAddition,

        [Description("Bonus ochotników")]
        VolunteerBonus,

        [Description("Siła frakcji")]
        FactionPower,

        [Description("Koszty utrzymania")]
        MaintenanceCost,
        [Description("Zużycie zasobów")]
        ResouerceUsage,
    }
}
