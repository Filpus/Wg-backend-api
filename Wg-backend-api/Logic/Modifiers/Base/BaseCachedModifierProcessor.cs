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

        protected BaseCachedModifierProcessor(GameDbContext context)
        {
            this._context = context;
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
                    if (effect.Conditions is not TConditions conditions)
                    {
                        result.Success = false;
                        result.Message = $"Nieprawidłowe warunki dla {this.SupportedType}. Oczekiwano {typeof(TConditions).Name}, otrzymano {effect.Conditions?.GetType().Name ?? "null"}";
                        return result;
                    }

                    var entities = await GetTargetEntities(nationId, conditions).ToListAsync();

                    if (!entities.Any())
                    {
                        continue;
                    }

                    var operation = Enum.Parse<ModifierOperation>(effect.Operation.ToString(), true);

                    foreach (var entity in entities)
                    {
                        ApplyToEntity(entity, operation, (float)effect.Value);
                    }

                    result.AffectedEntities.Add(
                        $"affected_{typeof(TEntity).Name}_{DateTime.UtcNow.Ticks}",
                        new ModifierChangeRecord
                        {
                            EntityType = typeof(TEntity).Name,
                            PropertyName = this.SupportedType.ToString(),
                            Change = entities.Count
                        }
                    );
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

            try
            {
                foreach (var effect in effects)
                {
                    if (effect.Conditions is not TConditions conditions)
                    {
                        result.Warnings.Add($"Nieprawidłowe warunki dla cofania modyfikatora {this.SupportedType}. Oczekiwano {typeof(TConditions).Name}");
                        continue;
                    }

                    var entities = await GetTargetEntities(nationId, conditions).ToListAsync();

                    if (!entities.Any())
                    {
                        result.Warnings.Add($"Brak encji do cofania dla {this.SupportedType}");
                        continue;
                    }

                    var operation = Enum.Parse<ModifierOperation>(effect.Operation.ToString(), true);

                    foreach (var entity in entities)
                    {
                        RevertFromEntity(entity, operation, (float)effect.Value);
                    }

                    result.AffectedEntities.Add(
                        $"reverted_{typeof(TEntity).Name}_{DateTime.UtcNow.Ticks}",
                        new ModifierChangeRecord
                        {
                            EntityType = typeof(TEntity).Name,
                            PropertyName = this.SupportedType.ToString(),
                            Change = -entities.Count
                        }
                    );
                }

                await context.SaveChangesAsync();
                result.Message = $"Cofnięto efekty modyfikatora {this.SupportedType}";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Błąd podczas cofania {this.SupportedType}: {ex.Message}";
            }

            return result;
        }
    }
}
