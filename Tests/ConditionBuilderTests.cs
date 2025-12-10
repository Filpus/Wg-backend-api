using Wg_backend_api.Enums;
using Wg_backend_api.Logic.Modifiers;
using Wg_backend_api.Logic.Modifiers.ModifierConditions;

namespace Tests
{
    [TestFixture]
    public class ConditionBuilderTests
    {
        private static IReadOnlyDictionary<ModifierType, Type> ExpectedMappings = new Dictionary<ModifierType, Type>
        {
            // Tylko populacja
            { ModifierType.PopulationHappiness, typeof(PopulationConditions) },
            { ModifierType.VoluneerChange, typeof(PopulationConditions) },

            // Populacja + zasób
            { ModifierType.ResourceProduction, typeof(PopulationResourceConditions) },
            { ModifierType.ResouerceUsage, typeof(PopulationResourceConditions) },

            // Tylko zasób
            { ModifierType.ResourceChange, typeof(ResourceConditions) },

            // Tylko frakcja
            { ModifierType.FactionPower, typeof(FactionConditions) },
            { ModifierType.FactionContenment, typeof(FactionConditions) }
        };

        [Test]
        public void GetConditionsType_ReturnsExpectedTypes_ForAllKnownModifiers()
        {
            foreach (var kv in ExpectedMappings)
            {
                var modifier = kv.Key;
                var expectedType = kv.Value;

                var actual = ModifierConditionsMapper.GetConditionsType(modifier);

                Assert.AreEqual(expectedType, actual, $"Expected type for {modifier} to be {expectedType}, but was {actual}.");
            }
        }

        [Test]
        public void CreateConditions_ReturnsInstanceOfExpectedType_ForAllKnownModifiers()
        {
            var emptyDict = new Dictionary<string, object>();

            foreach (var kv in ExpectedMappings)
            {
                var modifier = kv.Key;
                var expectedType = kv.Value;

                var result = ModifierConditionsMapper.CreateConditions(modifier, emptyDict);

                Assert.IsNotNull(result, $"CreateConditions returned null for modifier {modifier}");
                Assert.IsInstanceOf(expectedType, result, $"CreateConditions returned wrong type for {modifier}");
            }
        }

        [Test]
        public void UnsupportedModifier_ThrowsNotSupportedException_ForBothMethods()
        {
            var unsupported = (ModifierType)int.MaxValue;
            var emptyDict = new Dictionary<string, object>();

            Assert.Throws<NotSupportedException>(() => ModifierConditionsMapper.GetConditionsType(unsupported));
            Assert.Throws<NotSupportedException>(() => ModifierConditionsMapper.CreateConditions(unsupported, emptyDict));
        }
    }
}
