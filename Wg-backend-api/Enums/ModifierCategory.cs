using System.ComponentModel;

namespace Wg_backend_api.Enums
{
    public enum ModifierCategory
    {
        [Description("Ekonomiczne")]
        Economic,

        [Description("Społeczne")]
        Social,

        [Description("Wojskowe")]
        Military,

        [Description("Dyplomatyczne")]
        Diplomatic,

        [Description("Kulturowe")]
        Cultural,

        [Description("Administracyjne")]
        Administrative
    }
}
