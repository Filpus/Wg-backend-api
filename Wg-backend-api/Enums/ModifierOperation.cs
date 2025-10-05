using System.ComponentModel;

namespace Wg_backend_api.Enums
{
    public enum ModifierOperation
    {
        Add,        // currentValue + modifierValue

        Multiply,   // currentValue * modifierValue

        Percentage, // currentValue * (1 + modifierValue/100)


    }
}
