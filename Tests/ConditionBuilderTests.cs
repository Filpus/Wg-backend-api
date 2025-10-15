// Plan (pseudokod, szczegółowo):
// 1. Utworzyć listę testowych encji Population z różnymi wartościami CultureId, SocialGroupId i LocationId.
// 2. Dla każdego scenariusza utworzyć PopulationConditionBuilder z baseQuery = list.AsQueryable().
// 3. Przygotować słownik conditions z różnymi typami wartości:
//    - int bezpośrednio,
//    - JsonElement (JsonDocument.Parse(...).RootElement),
//    - string reprezentujący liczbę.
// 4. Wywołać ApplyConditions(conditions) i Build().Sprawdzić, że wyniki są poprawnie przefiltrowane.
// 5. Sprawdzić, że wartości domyślne (0) są ignorowane i że Reset() przywraca oryginalne query.
// 6. Użyć NUnit do asercji wyników.

// Testy NUnit dla ConditionBuilder i PopulationConditionBuilder

using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Wg_backend_api.Logic.Modifiers.ConditionBuilder;
using Wg_backend_api.Models; // dopasuj przestrzeń nazw projektu, jeśli inna

namespace Tests
{
    [TestFixture]
    public class ConditionBuilderTests
    {
        private List<Population> _populations;

        [SetUp]
        public void SetUp()
        {
            // Przygotowanie danych testowych
            _populations = new List<Population>
            {
                new Population { CultureId = 1, SocialGroupId = 1, LocationId = 1 },
                new Population { CultureId = 2, SocialGroupId = 2, LocationId = 2 },
                new Population { CultureId = 2, SocialGroupId = 3, LocationId = 3 },
                new Population { CultureId = 3, SocialGroupId = 2, LocationId = 3 }
            };
        }

        [Test]
        public void ApplyConditions_FiltersByCultureId_WhenIntProvided()
        {
            var builder = new PopulationConditionBuilder(_populations.AsQueryable());
            var conditions = new Dictionary<string, object>
            {
                { "culture_id", 2 }
            };

            var result = builder.ApplyConditions(conditions).Build().ToList();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(p => p.CultureId == 2));
        }

        [Test]
        public void ApplyConditions_FiltersBySocialGroupId_WhenJsonElementProvided()
        {
            var builder = new PopulationConditionBuilder(_populations.AsQueryable());
            var jsonElement = JsonDocument.Parse("2").RootElement;
            var conditions = new Dictionary<string, object>
            {
                { "social_group_id", jsonElement }
            };

            var result = builder.ApplyConditions(conditions).Build().ToList();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(p => p.SocialGroupId == 2));
        }

        [Test]
        public void ApplyConditions_FiltersByLocalisationId_WhenStringNumberProvided()
        {
            var builder = new PopulationConditionBuilder(_populations.AsQueryable());
            var conditions = new Dictionary<string, object>
            {
                { "localisation_id", "3" } // powinno konwertować string -> int
            };

            var result = builder.ApplyConditions(conditions).Build().ToList();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(p => p.LocationId == 3));
        }

        [Test]
        public void ApplyConditions_IgnoresDefaultZeroValue_And_ResetRestoresOriginalQuery()
        {
            var builder = new PopulationConditionBuilder(_populations.AsQueryable());

            // zero powinno być traktowane jako brak warunku
            var conditionsWithZero = new Dictionary<string, object>
            {
                { "culture_id", 0 }
            };

            var resultZero = builder.ApplyConditions(conditionsWithZero).Build().ToList();
            Assert.AreEqual(_populations.Count, resultZero.Count);

            // zastosuj rzeczywisty filtr
            var conditions = new Dictionary<string, object>
            {
                { "culture_id", 2 }
            };
            var filtered = builder.ApplyConditions(conditions).Build().ToList();
            Assert.AreEqual(2, filtered.Count);

            // reset powinien przywrócić oryginalne query
            builder.Reset();
            var all = builder.Build().ToList();
            Assert.AreEqual(_populations.Count, all.Count);
        }
    }



}
