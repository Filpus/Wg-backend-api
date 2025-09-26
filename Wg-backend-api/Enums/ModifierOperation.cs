using System.ComponentModel;

namespace Wg_backend_api.Enums
{
    public enum ModifierOperation
    {
        [Description("Dodaj/Odejmij wartość")]
        Add,        // currentValue + modifierValue

        [Description("Pomnóż przez wartość")]
        Multiply,   // currentValue * modifierValue

        [Description("Zmień o procent")]
        Percentage, // currentValue * (1 + modifierValue/100)

        [Description("Ustaw na wartość")]
        Set         // currentValue = modifierValue
    }
}
