using Wg_backend_api.Enums;
using Wg_backend_api.Logic.Modifiers.Interfaces;

namespace Wg_backend_api.Models
{
    /// <summary>
    /// Rekord opisujący zmianę wartości na encji
    /// </summary>
    public class ModifierChangeRecord
    {
        /// <summary>
        /// ID zmodyfikowanej encji
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Typ encji
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// Nazwa właściwości która została zmieniona
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Wartość przed zmianą
        /// </summary>
        public object OldValue { get; set; }

        /// <summary>
        /// Wartość po zmianie
        /// </summary>
        public object NewValue { get; set; }

        /// <summary>
        /// Różnica (NewValue - OldValue)
        /// </summary>
        public object Change { get; set; }

    }

    /// <summary>
    /// Wynik zastosowania modyfikatora
    /// </summary>
    public class ModifierApplicationResult
    {
        /// <summary>
        /// Czy operacja zakończyła się sukcesem
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Komunikat o wyniku operacji
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Słownik zmodyfikowanych encji (klucz -> dane o zmianie)
        /// </summary>
        public Dictionary<string, object> AffectedEntities { get; set; } = [];

        /// <summary>
        /// Lista ostrzeżeń (nieblokujące problemy)
        /// </summary>
        public List<string> Warnings { get; set; } = [];

        /// <summary>
        /// Dodatkowe metadane o operacji
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = [];
    }

    /// <summary>
    /// Reprezentuje pojedynczy efekt modyfikatora z JSON
    /// </summary>

    public class ModifierEffect
    {
        public ModifierOperation Operation { get; set; }
        public float Value { get; set; }
        public IBaseModifierConditions Conditions { get; set; }

    }



}
