using Wg_backend_api.Data;
using Wg_backend_api.Enums;
using Wg_backend_api.Models;

namespace Wg_backend_api.Logic.Modifires
{
    /// <summary>
    /// Interfejs dla procesorów modyfikatorów. Każdy typ modyfikatora ma swój procesor.
    /// </summary>
    public interface IModifierProcessor
    {
        /// <summary>
        /// Typ modyfikatora obsługiwany przez ten procesor
        /// </summary>
        ModifierType SupportedType { get; }

        /// <summary>
        /// Przetwarza listę efektów modyfikatora dla danego narodu
        /// </summary>
        /// <param name="nationId">ID narodu do którego zastosować modyfikator</param>
        /// <param name="effects">Lista efektów do zastosowania</param>
        /// <param name="context">Kontekst bazy danych</param>
        /// <returns>Wynik aplikacji modyfikatora</returns>
        Task<ModifierApplicationResult> ProcessAsync(int nationId, List<ModifierEffect> effects, GameDbContext context);

        /// <summary>
        /// Odwraca efekt modyfikatora (dla modyfikatorów czasowych)
        /// </summary>
        /// <param name="nationId">ID narodu</param>
        /// <param name="effects">Lista efektów do odwrócenia</param>
        /// <param name="context">Kontekst bazy danych</param>
        /// <returns>Wynik usunięcia modyfikatora</returns>
        Task<ModifierApplicationResult> RevertAsync(int nationId, List<ModifierEffect> effects, GameDbContext context);

        /// <summary>
        /// Sprawdza czy modyfikator może być zastosowany do danego narodu
        /// </summary>
        /// <param name="nationId">ID narodu</param>
        /// <param name="effects">Lista efektów do sprawdzenia</param>
        /// <param name="context">Kontekst bazy danych</param>
        /// <returns>True jeśli można zastosować</returns>
        Task<bool> CanApplyAsync(int nationId, List<ModifierEffect> effects, GameDbContext context);
    }
    /// <summary>
    /// Interfejs dla builderów warunków. Pozwala na dynamiczne budowanie zapytań z warunkami.
    /// </summary>
    /// <typeparam name="TEntity">Typ encji na której operujemy</typeparam>
    public interface IConditionBuilder<TEntity>
    {
        /// <summary>
        /// Aplikuje warunki z słownika na query
        /// </summary>
        /// <param name="conditions">Słownik warunków z JSON</param>
        /// <returns>Builder z zastosowanymi warunkami</returns>
        IConditionBuilder<TEntity> ApplyConditions(Dictionary<string, object> conditions);

        /// <summary>
        /// Buduje finalne query z wszystkimi zastosowanymi warunkami
        /// </summary>
        /// <returns>IQueryable z zastosowanymi filtrami</returns>
        IQueryable<TEntity> Build();

        /// <summary>
        /// Resetuje wszystkie warunki do stanu początkowego
        /// </summary>
        /// <returns>Builder w stanie początkowym</returns>
        IConditionBuilder<TEntity> Reset();
    }



}
