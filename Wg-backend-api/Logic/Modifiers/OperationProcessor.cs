using Wg_backend_api.Enums;

namespace Wg_backend_api.Logic.Modifiers
{
    public static class OperationProcessor
    {
        public static float ApplyOperation(float currentValue, float modifierValue, ModifierOperation operation)
        {
            return operation switch
            {
                ModifierOperation.Add => currentValue + modifierValue,
                ModifierOperation.Multiply => currentValue * modifierValue,
                ModifierOperation.Percentage => currentValue * (1 + (modifierValue / 100)),
                _ => currentValue
            };
        }

        public static float ReverseOperation(float currentValue, float modifierValue, ModifierOperation operation)
        {
            return operation switch
            {
                ModifierOperation.Add => currentValue - modifierValue,
                ModifierOperation.Multiply => modifierValue != 0 ? currentValue / modifierValue : currentValue,
                ModifierOperation.Percentage => currentValue / (1 + (modifierValue / 100)),
                _ => currentValue
            };
        }
    }
}
