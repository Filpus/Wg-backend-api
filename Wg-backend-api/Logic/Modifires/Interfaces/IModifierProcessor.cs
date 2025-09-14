using Wg_backend_api.Data;
using Wg_backend_api.Enums;
using Wg_backend_api.Models;

namespace Wg_backend_api.Logic.Modifires.Interfaces
{
    public interface IModifierProcessor
    {
        ModifierType SupportedType { get; }
        Task<ModifierApplicationResult> ProcessAsync(int nationId, List<ModifierEffect> effects, GameDbContext context);
        Task<ModifierApplicationResult> RevertAsync(int nationId, List<ModifierEffect> effects, GameDbContext context);
        Task<bool> CanApplyAsync(int nationId, List<ModifierEffect> effects, GameDbContext context);
    }
}
