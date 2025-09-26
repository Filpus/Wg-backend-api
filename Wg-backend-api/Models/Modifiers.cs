using System.Text.Json;

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
        public Dictionary<string, object> AffectedEntities { get; set; } = new();

        /// <summary>
        /// Lista ostrzeżeń (nieblokujące problemy)
        /// </summary>
        public List<string> Warnings { get; set; } = new();

        /// <summary>
        /// Dodatkowe metadane o operacji
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Reprezentuje pojedynczy efekt modyfikatora z JSON
    /// </summary>

    public class ModifierEffect
    {
        /// <summary>
        /// Typ operacji: "add", "multiply", "percentage", "set"
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// Wartość modyfikatora
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Warunki targetowania z JSON
        /// </summary>
        public Dictionary<string, object> Conditions { get; set; } = new();

        /// <summary>
        /// Pomocnicza metoda do pobierania warunków z bezpieczną konwersją
        /// </summary>
        public T GetCondition<T>(string key, T defaultValue = default(T))
        {
            if (Conditions?.ContainsKey(key) == true)
            {
                try
                {
                    var value = Conditions[key];
                    if (value is JsonElement element)
                    {
                        return JsonSerializer.Deserialize<T>(element.GetRawText());
                    }
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }
    }

}
