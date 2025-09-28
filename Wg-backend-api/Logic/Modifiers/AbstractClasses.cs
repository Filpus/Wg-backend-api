using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Wg_backend_api.Data;
using Wg_backend_api.Enums;
using Wg_backend_api.Models;

namespace Wg_backend_api.Logic.Modifiers
{
    /// <summary>
    /// Abstrakcyjna klasa bazowa dla wszystkich condition builderów.
    /// Dostarcza wspólną funkcjonalność dla filtrowania encji według warunków z JSON.
    /// </summary>
    /// <typeparam name="TEntity">Typ encji Entity Framework</typeparam>
    public abstract class ConditionBuilder<TEntity> : IConditionBuilder<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Query które jest modyfikowane przez zastosowanie warunków
        /// </summary>
        protected IQueryable<TEntity> Query { get; set; }

        /// <summary>
        /// Oryginalne query do resetowania
        /// </summary>
        private readonly IQueryable<TEntity> _originalQuery;

        protected ConditionBuilder(IQueryable<TEntity> baseQuery)
        {
            Query = baseQuery ?? throw new ArgumentNullException(nameof(baseQuery));
            _originalQuery = baseQuery;
        }

        /// <summary>
        /// Konkretne implementacje definiują jak mapować warunki na filtry
        /// </summary>
        /// <param name="conditions">Słownik warunków z JSON modyfikatora</param>
        /// <returns>Builder z zastosowanymi warunkami</returns>
        public abstract IConditionBuilder<TEntity> ApplyConditions(Dictionary<string, object> conditions);

        /// <summary>
        /// Zwraca finalne query z wszystkimi zastosowanymi filtrami
        /// </summary>
        public virtual IQueryable<TEntity> Build() => Query;

        /// <summary>
        /// Resetuje query do stanu początkowego
        /// </summary>
        public virtual IConditionBuilder<TEntity> Reset()
        {
            Query = _originalQuery;
            return this;
        }

        /// <summary>
        /// Bezpiecznie pobiera i konwertuje wartość warunku
        /// </summary>
        /// <typeparam name="T">Typ na który konwertować</typeparam>
        /// <param name="conditions">Słownik warunków</param>
        /// <param name="key">Klucz warunku</param>
        /// <param name="value">Wyjściowa wartość</param>
        /// <returns>True jeśli warunek istnieje i da się skonwertować</returns>
        protected bool TryGetCondition<T>(Dictionary<string, object> conditions, string key, out T value)
        {
            value = default(T);

            if (!conditions.TryGetValue(key, out var rawValue))
                return false;

            try
            {
                if (rawValue == null)
                    return false;

                // Handle JsonElement from System.Text.Json
                if (rawValue is JsonElement jsonElement)
                {
                    value = JsonSerializer.Deserialize<T>(jsonElement.GetRawText());
                }
                else
                {
                    value = (T)Convert.ChangeType(rawValue, typeof(T));
                }

                return value != null && !value.Equals(default(T));
            }
            catch (Exception ex)
            {
                // Log warning about invalid condition value
                System.Diagnostics.Debug.WriteLine($"Nie można skonwertować warunku '{key}' na typ {typeof(T).Name}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Pomocnicza metoda do bezpiecznej konwersji wartości
        /// </summary>
        protected T ConvertValue<T>(object value)
        {
            try
            {
                if (value is JsonElement jsonElement)
                {
                    return JsonSerializer.Deserialize<T>(jsonElement.GetRawText());
                }
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// Sprawdza czy wszystkie wymagane warunki są obecne
        /// </summary>
        /// <param name="conditions">Słownik warunków</param>
        /// <param name="requiredConditions">Lista wymaganych kluczy</param>
        /// <returns>True jeśli wszystkie wymagane warunki są obecne</returns>
        protected bool ValidateRequiredConditions(Dictionary<string, object> conditions, params string[] requiredConditions)
        {
            return requiredConditions.All(required =>
                conditions.ContainsKey(required) &&
                conditions[required] != null);
        }
    }
    /// <summary>
    /// Abstrakcyjna klasa bazowa dla wszystkich procesorów modyfikatorów.
    /// Implementuje wspólną logikę przetwarzania i wymaga od podklas zdefiniowania specyficznych operacji.
    /// </summary>
    /// <typeparam name="TEntity">Typ encji na której operuje modyfikator</typeparam>
    public abstract class BaseModifierProcessor<TEntity> : IModifierProcessor
        where TEntity : class
    {
        protected readonly GameDbContext _context;
        protected readonly ILogger<BaseModifierProcessor<TEntity>> _logger;

        protected BaseModifierProcessor(GameDbContext context, ILogger<BaseModifierProcessor<TEntity>> logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
        }

        /// <summary>
        /// Typ modyfikatora obsługiwany przez ten procesor
        /// </summary>
        public abstract ModifierType SupportedType { get; }

        /// <summary>
        /// Kategoria modyfikatora (dla organizacji w UI)
        /// </summary>
        public virtual ModifierCategory Category { get; protected set; }

        /// <summary>
        /// Buduje bazowe query dla encji należących do danego narodu
        /// </summary>
        /// <param name="nationId">ID narodu</param>
        /// <returns>IQueryable z encjami narodu</returns>
        protected abstract IQueryable<TEntity> GetBaseQuery(int nationId);

        /// <summary>
        /// Tworzy condition builder dla tego typu encji
        /// </summary>
        /// <param name="baseQuery">Bazowe query do filtrowania</param>
        /// <returns>Condition builder</returns>
        protected abstract ConditionBuilder<TEntity> CreateConditionBuilder(IQueryable<TEntity> baseQuery);

        /// <summary>
        /// Aplikuje efekt modyfikatora na pojedynczą encję
        /// </summary>
        /// <param name="entity">Encja do modyfikacji</param>
        /// <param name="effect">Efekt do zastosowania</param>
        /// <returns>Informacje o zmianie dla logowania</returns>
        protected abstract ModifierChangeRecord ApplyToEntity(TEntity entity, ModifierEffect effect);

        /// <summary>
        /// Odwraca efekt modyfikatora na pojedynczej encji
        /// </summary>
        /// <param name="entity">Encja do modyfikacji</param>
        /// <param name="effect">Efekt do odwrócenia</param>
        /// <returns>Informacje o zmianie</returns>
        protected virtual ModifierChangeRecord RevertFromEntity(TEntity entity, ModifierEffect effect)
        {
            // Domyślna implementacja - odwróć wartości operacji
            var reversedEffect = new ModifierEffect
            {
                Operation = effect.Operation,
                Value = effect.Operation switch
                {
                    "add" => -effect.Value,
                    "multiply" => effect.Value != 0 ? 1 / effect.Value : 0,
                    "percentage" => -effect.Value,
                    "set" => effect.Value, // Set nie może być łatwo odwrócone
                    _ => effect.Value
                },
                Conditions = effect.Conditions
            };

            return ApplyToEntity(entity, reversedEffect);
        }

        /// <summary>
        /// Pobiera ID encji (dla logowania)
        /// </summary>
        protected abstract int GetEntityId(TEntity entity);

        /// <summary>
        /// Główna metoda przetwarzania modyfikatora
        /// </summary>
        public virtual async Task<ModifierApplicationResult> ProcessAsync(int nationId, List<ModifierEffect> effects, GameDbContext context)
        {
            var result = new ModifierApplicationResult { Success = true };
            var changeRecords = new List<ModifierChangeRecord>();

            try
            {
                foreach (var effect in effects)
                {
                    // Walidacja efektu
                    if (!ValidateEffect(effect))
                    {
                        result.Warnings.Add($"Nieprawidłowy efekt: {effect.Operation} {effect.Value}");
                        continue;
                    }

                    // Pobierz i przefiltruj encje
                    var entities = await GetTargetEntities(nationId, effect.Conditions).ToListAsync();

                    if (entities.Count == 0)
                    {
                        result.Warnings.Add($"Nie znaleziono encji spełniających warunki dla {SupportedType}");
                        continue;
                    }

                    // Aplikuj modyfikator na każdej encji
                    foreach (var entity in entities)
                    {
                        var changeRecord = ApplyToEntity(entity, effect);
                        if (changeRecord != null)
                        {
                            changeRecords.Add(changeRecord);
                        }
                    }

                    _logger?.LogDebug($"Zastosowano efekt {effect.Operation} {effect.Value} na {entities.Count} encji");
                }

                await context.SaveChangesAsync();

                result.AffectedEntities = changeRecords
                    .GroupBy(cr => cr.EntityId)
                    .ToDictionary(
                        g => $"{typeof(TEntity).Name}_{g.Key}",
                        g => (object)g.Last() // wybiera najnowszą zmianę dla encji
                    );

                result.Message = $"Pomyślnie zmodyfikowano {changeRecords.Count} encji typu {typeof(TEntity).Name}";

                _logger?.LogInformation($"Procesor {SupportedType} zmodyfikował {changeRecords.Count} encji dla narodu {nationId}");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Błąd podczas przetwarzania modyfikatora {SupportedType}: {ex.Message}";
                _logger?.LogError(ex, $"Błąd w procesorze {SupportedType} dla narodu {nationId}");
            }

            return result;
        }

        /// <summary>
        /// Odwraca modyfikator
        /// </summary>
        public virtual async Task<ModifierApplicationResult> RevertAsync(int nationId, List<ModifierEffect> effects, GameDbContext context)
        {
            var result = new ModifierApplicationResult { Success = true };
            var changeRecords = new List<ModifierChangeRecord>();

            try
            {
                foreach (var effect in effects)
                {
                    var entities = await GetTargetEntities(nationId, effect.Conditions).ToListAsync();

                    foreach (var entity in entities)
                    {
                        var changeRecord = RevertFromEntity(entity, effect);
                        if (changeRecord != null)
                        {
                            changeRecords.Add(changeRecord);
                        }
                    }
                }

                await context.SaveChangesAsync();

                result.AffectedEntities = changeRecords.ToDictionary(
                    cr => $"{typeof(TEntity).Name}_{cr.EntityId}",
                    cr => (object)cr
                );

                result.Message = $"Pomyślnie odwrócono modyfikator na {changeRecords.Count} encjach";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Błąd podczas odwracania modyfikatora {SupportedType}: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Sprawdza czy modyfikator można zastosować
        /// </summary>
        public virtual async Task<bool> CanApplyAsync(int nationId, List<ModifierEffect> effects, GameDbContext context)
        {
            try
            {
                // Podstawowa walidacja - sprawdź czy istnieją jakiekolwiek encje do modyfikacji
                var hasTargetEntities = await GetBaseQuery(nationId).AnyAsync();
                return hasTargetEntities;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Pobiera encje które mają być zmodyfikowane
        /// </summary>
        protected virtual IQueryable<TEntity> GetTargetEntities(int nationId, Dictionary<string, object> conditions)
        {
            var baseQuery = GetBaseQuery(nationId);
            return CreateConditionBuilder(baseQuery)
                .ApplyConditions(conditions)
                .Build();
        }

        /// <summary>
        /// Waliduje czy efekt jest prawidłowy
        /// </summary>
        protected virtual bool ValidateEffect(ModifierEffect effect)
        {
            if (string.IsNullOrEmpty(effect.Operation))
                return false;

            if (!Enum.TryParse<ModifierOperation>(effect.Operation, true, out _))
                return false;

            // Dodatkowe walidacje można dodać w podklasach
            return true;
        }
    }




}
