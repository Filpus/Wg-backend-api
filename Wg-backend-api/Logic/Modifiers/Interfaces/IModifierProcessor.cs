using Wg_backend_api.Data;
using Wg_backend_api.Enums;
using Wg_backend_api.Models;

namespace Wg_backend_api.Logic.Modifiers.Interfaces
{
    public interface IModifierProcessor
    {
        public ModifierType SupportedType { get; }
        public Task<ModifierApplicationResult> ProcessAsync(int nationId, List<ModifierEffect> effects, GameDbContext context);
        public Task<ModifierApplicationResult> RevertAsync(int nationId, List<ModifierEffect> effects, GameDbContext context);
    }
}
