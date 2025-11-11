using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.Enums;
using Wg_backend_api.Logic.Modifiers.Interfaces;
using Wg_backend_api.Models;

namespace Wg_backend_api.Logic.Modifiers.Base
{
    public abstract class BaseCachedModifierProcessor<TEntity, TConditions> : IModifierProcessor
     where TEntity : class
     where TConditions : IBaseModifierConditions
    {
        protected readonly GameDbContext _context;
        protected readonly ILogger _logger;

        protected BaseCachedModifierProcessor(GameDbContext context, ILogger logger)
        {
            this._context = context;
            this._logger = logger;
        }

        public abstract ModifierType SupportedType { get; }

        // Abstrakcyjne metody z pełnym typowaniem
        protected abstract IQueryable<TEntity> GetTargetEntities(int nationId, TConditions conditions);
        protected abstract void ApplyToEntity(TEntity entity, ModifierOperation operation, float value);
        protected abstract void RevertFromEntity(TEntity entity, ModifierOperation operation, float value);
        protected abstract int GetEntityId(TEntity entity);

        public async Task<ModifierApplicationResult> ProcessAsync(int nationId, List<ModifierEffect> effects, GameDbContext context)
        {
            var result = new ModifierApplicationResult { Success = true };

            try
            {
                foreach (var effect in effects)
                {
                    if (ModifierConditionsMapper.CreateConditions(this.SupportedType, effect.Conditions) is not TConditions conditions)
                    {
                        result.Success = false;
                        result.Message = $"Nieprawidłowe warunki dla {this.SupportedType}";
                        return result;
                    }

                    var entities = await GetTargetEntities(nationId, conditions).ToListAsync();

                    if (!entities.Any())
                    {
                        result.Success = false;
                        result.Message = $"Nie znaleziono encji do modyfikacji dla {this.SupportedType}";
                        return result;
                    }

                    var operation = Enum.Parse<ModifierOperation>(effect.Operation, true);

                    foreach (var entity in entities)
                    {
                        ApplyToEntity(entity, operation, effect.Value);
                    }

                    result.AffectedEntities.Add($"affected_{typeof(TEntity).Name}_count", entities.Count);
                }

                await context.SaveChangesAsync();
                result.Message = $"Zakończono efekty modyfikatora {this.SupportedType}";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Błąd podczas aplikowania {this.SupportedType}: {ex.Message}";
            }

            return result;
        }

        public async Task<ModifierApplicationResult> RevertAsync(int nationId, List<ModifierEffect> effects, GameDbContext context)
        {
            var result = new ModifierApplicationResult { Success = true };

            foreach (var effect in effects)
            {
                if (ModifierConditionsMapper.CreateConditions(this.SupportedType, effect.Conditions) is not TConditions conditions)
                {
                    result.Warnings.Add($"Nieprawidłowe warunki dla cofania modyfikatora {this.SupportedType}");
                    continue;
                }

                var entities = await GetTargetEntities(nationId, conditions).ToListAsync();
                var operation = Enum.Parse<ModifierOperation>(effect.Operation, true);

                foreach (var entity in entities)
                {
                    RevertFromEntity(entity, operation, effect.Value);
                }

                result.AffectedEntities.Add($"reverted_{typeof(TEntity).Name}_count", entities.Count);

                this._logger?.LogInformation($"Cofnięto {this.SupportedType} na {entities.Count} encji typu {typeof(TEntity).Name}");
            }

            await context.SaveChangesAsync();
            result.Message = $"Cofnięto efekty modyfikatora {this.SupportedType}";
            return result;
        }

    }

}
