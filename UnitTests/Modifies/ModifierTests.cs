using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wg_backend_api.Data;
using Wg_backend_api.Enums;
using Wg_backend_api.Logic.Modifires.Processors;
using Wg_backend_api.Models;
using Wg_backend_api.Services;
using Wg_backend_api.Logic.Modifires.ConditionBuilder;
using Wg_backend_api.Logic;
namespace ModifierTests
{
    [TestFixture]
    public class PopulationHappinessProcessorTests
    {
        private GameDbContext _context;
        private PopulationHappinessProcessor _processor;
        private Mock<ILogger<PopulationHappinessProcessor>> _mockLogger;

        [SetUp]
        public void Setup()
        {
            // Konfiguracja In-Memory Database dla testów
            var options = new DbContextOptionsBuilder<GameDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            _context = new GameDbContext(options, "test_schema");
            _mockLogger = new Mock<ILogger<PopulationHappinessProcessor>>();
            _processor = new PopulationHappinessProcessor(_context);

            SeedTestData();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private void SeedTestData()
        {
            var nation = new Nation { Id = 1, Name = "Test Nation" };
            var culture = new Culture { Id = 1, Name = "Test Culture" };
            var religion = new Religion { Id = 1, Name = "Test Religion" };
            var socialGroup = new SocialGroup { Id = 1, Name = "Mieszczanie", BaseHappiness = 50, Volunteers = 100 };
            var location = new Localisation { Id = 1, Name = "Stolica", Size = 5, Fortification = 3, NationId = 1, Nation = nation };

            var population1 = new Population
            {
                Id = 1,
                ReligionId = 1,
                CultureId = 1,
                SocialGroupId = 1,
                LocationId = 1,
                Happiness = 60.0f,
                Religion = religion,
                Culture = culture,
                SocialGroup = socialGroup,
                Location = location
            };

            var population2 = new Population
            {
                Id = 2,
                ReligionId = 1,
                CultureId = 1,
                SocialGroupId = 1,
                LocationId = 1,
                Happiness = 70.0f,
                Religion = religion,
                Culture = culture,
                SocialGroup = socialGroup,
                Location = location
            };

            _context.Nations.Add(nation);
            _context.Cultures.Add(culture);
            _context.Religions.Add(religion);
            _context.SocialGroups.Add(socialGroup);
            _context.Localisations.Add(location);
            _context.Populations.AddRange(population1, population2);

            _context.SaveChanges();
        }

        [Test]
        public async Task ProcessAsync_AddOperation_IncreasesHappiness()
        {
            var effects = new List<ModifierEffect>
            {
                new ModifierEffect
                {
                    Operation = "add",
                    Value = 10,
                    Conditions = new Dictionary<string, object>()
                }
            };

            var result = await _processor.ProcessAsync(1, effects, _context);

            NUnit.Framework.Assert.That(result.Success, Is.True);
            var populations = await _context.Populations
                .Where(p => p.Location.NationId == 1)
                .ToListAsync();

            NUnit.Framework.Assert.That(populations[0].Happiness, Is.EqualTo(70.0f));
            NUnit.Framework.Assert.That(populations[1].Happiness, Is.EqualTo(80.0f));
            NUnit.Framework.Assert.That(result.AffectedEntities.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task ProcessAsync_SubtractOperation_DecreasesHappiness()
        {
            var effects = new List<ModifierEffect>
            {
                new ModifierEffect
                {
                    Operation = "add",
                    Value = -15,
                    Conditions = new Dictionary<string, object>()
                }
            };

            var result = await _processor.ProcessAsync(1, effects, _context);

            NUnit.Framework.Assert.That(result.Success, Is.True);
            var populations = await _context.Populations
                .Where(p => p.Location.NationId == 1)
                .ToListAsync();

            NUnit.Framework.Assert.That(populations[0].Happiness, Is.EqualTo(45.0f));
            NUnit.Framework.Assert.That(populations[1].Happiness, Is.EqualTo(55.0f));
        }

        [Test]
        public async Task ProcessAsync_PercentageOperation_ModifiesHappinessByPercentage()
        {
            var effects = new List<ModifierEffect>
            {
                new ModifierEffect
                {
                    Operation = "percentage",
                    Value = 20,
                    Conditions = new Dictionary<string, object>()
                }
            };

            var result = await _processor.ProcessAsync(1, effects, _context);

            NUnit.Framework.Assert.That(result.Success, Is.True);
            var populations = await _context.Populations
                .Where(p => p.Location.NationId == 1)
                .ToListAsync();

            NUnit.Framework.Assert.That(populations[0].Happiness, Is.EqualTo(72.0f).Within(0.1f));
            NUnit.Framework.Assert.That(populations[1].Happiness, Is.EqualTo(84.0f).Within(0.1f));
        }

        [Test]
        public async Task ProcessAsync_WithCultureCondition_AffectsOnlySpecificCulture()
        {
            var culture2 = new Culture { Id = 2, Name = "Inna Kultura" };
            var population3 = new Population
            {
                Id = 3,
                ReligionId = 1,
                CultureId = 2,
                SocialGroupId = 1,
                LocationId = 1,
                Happiness = 50.0f,
                Culture = culture2
            };

            _context.Cultures.Add(culture2);
            _context.Populations.Add(population3);
            await _context.SaveChangesAsync();

            var effects = new List<ModifierEffect>
            {
                new ModifierEffect
                {
                    Operation = "add",
                    Value = 20,
                    Conditions = new Dictionary<string, object>
                    {
                        ["culture_id"] = 1
                    }
                }
            };

            var result = await _processor.ProcessAsync(1, effects, _context);

            NUnit.Framework.Assert.That(result.Success, Is.True);
            var allPopulations = await _context.Populations
                .Where(p => p.Location.NationId == 1)
                .ToListAsync();

            var culture1Populations = allPopulations.Where(p => p.CultureId == 1).ToList();
            var culture2Populations = allPopulations.Where(p => p.CultureId == 2).ToList();

            NUnit.Framework.Assert.That(culture1Populations[0].Happiness, Is.EqualTo(80.0f));
            NUnit.Framework.Assert.That(culture1Populations[1].Happiness, Is.EqualTo(90.0f));
            NUnit.Framework.Assert.That(culture2Populations[0].Happiness, Is.EqualTo(50.0f));
        }

        [Test]
        public async Task ProcessAsync_WithSocialGroupCondition_AffectsOnlySpecificSocialGroup()
        {
            var socialGroup2 = new SocialGroup
            {
                Id = 2,
                Name = "Szlachta",
                BaseHappiness = 80,
                Volunteers = 50
            };

            var population3 = new Population
            {
                Id = 3,
                ReligionId = 1,
                CultureId = 1,
                SocialGroupId = 2,
                LocationId = 1,
                Happiness = 80.0f,
                SocialGroup = socialGroup2
            };

            _context.SocialGroups.Add(socialGroup2);
            _context.Populations.Add(population3);
            await _context.SaveChangesAsync();

            var effects = new List<ModifierEffect>
            {
                new ModifierEffect
                {
                    Operation = "add",
                    Value = 10,
                    Conditions = new Dictionary<string, object>
                    {
                        ["social_group_id"] = 1
                    }
                }
            };

            var result = await _processor.ProcessAsync(1, effects, _context);

            var allPopulations = await _context.Populations
                .Where(p => p.Location.NationId == 1)
                .ToListAsync();

            var group1Populations = allPopulations.Where(p => p.SocialGroupId == 1).ToList();
            var group2Populations = allPopulations.Where(p => p.SocialGroupId == 2).ToList();

            NUnit.Framework.Assert.That(group1Populations[0].Happiness, Is.EqualTo(70.0f));
            NUnit.Framework.Assert.That(group1Populations[1].Happiness, Is.EqualTo(80.0f));
            NUnit.Framework.Assert.That(group2Populations[0].Happiness, Is.EqualTo(80.0f));
        }

        [Test]
        public async Task ProcessAsync_HappinessClampedTo100_WhenExceeding()
        {
            var effects = new List<ModifierEffect>
            {
                new ModifierEffect
                {
                    Operation = "add",
                    Value = 50,
                    Conditions = new Dictionary<string, object>()
                }
            };

            var result = await _processor.ProcessAsync(1, effects, _context);

            var populations = await _context.Populations
                .Where(p => p.Location.NationId == 1)
                .ToListAsync();

            NUnit.Framework.Assert.That(populations.All(p => p.Happiness <= 100), Is.True);
            NUnit.Framework.Assert.That(populations[0].Happiness, Is.EqualTo(100.0f));
            NUnit.Framework.Assert.That(populations[1].Happiness, Is.EqualTo(100.0f));
        }

        [Test]
        public async Task ProcessAsync_HappinessClampedTo0_WhenBelowZero()
        {
            var effects = new List<ModifierEffect>
            {
                new ModifierEffect
                {
                    Operation = "add",
                    Value = -80,
                    Conditions = new Dictionary<string, object>()
                }
            };

            var result = await _processor.ProcessAsync(1, effects, _context);

            var populations = await _context.Populations
                .Where(p => p.Location.NationId == 1)
                .ToListAsync();

            NUnit.Framework.Assert.That(populations.All(p => p.Happiness >= 0), Is.True);
            NUnit.Framework.Assert.That(populations[0].Happiness, Is.EqualTo(0.0f));
            NUnit.Framework.Assert.That(populations[1].Happiness, Is.EqualTo(0.0f));
        }

        [Test]
        public async Task ProcessAsync_NoPopulationsFound_ReturnsSuccessWithWarning()
        {
            _context.Populations.RemoveRange(_context.Populations);
            await _context.SaveChangesAsync();

            var effects = new List<ModifierEffect>
            {
                new ModifierEffect
                {
                    Operation = "add",
                    Value = 10,
                    Conditions = new Dictionary<string, object>()
                }
            };

            var result = await _processor.ProcessAsync(1, effects, _context);

            NUnit.Framework.Assert.That(result.Success, Is.True);
            NUnit.Framework.Assert.That(result.AffectedEntities.Count, Is.EqualTo(0));
            NUnit.Framework.Assert.That(result.Warnings.Any(w => w.Contains("Nie znaleziono encji")), Is.True);
        }

        [Test]
        public async Task ProcessAsync_MultipleEffects_AppliesAllEffects()
        {
            var effects = new List<ModifierEffect>
            {
                new ModifierEffect
                {
                    Operation = "add",
                    Value = 10,
                    Conditions = new Dictionary<string, object>()
                },
                new ModifierEffect
                {
                    Operation = "percentage",
                    Value = 20,
                    Conditions = new Dictionary<string, object>()
                }
            };

            var result = await _processor.ProcessAsync(1, effects, _context);

            NUnit.Framework.Assert.That(result.Success, Is.True);
            var populations = await _context.Populations
                .Where(p => p.Location.NationId == 1)
                .ToListAsync();

            NUnit.Framework.Assert.That(populations[0].Happiness, Is.EqualTo(84.0f).Within(0.1f));
            NUnit.Framework.Assert.That(populations[1].Happiness, Is.EqualTo(96.0f).Within(0.1f));
        }

        [Test]
        public async Task CanApplyAsync_WithValidNation_ReturnsTrue()
        {
            var canApply = await _processor.CanApplyAsync(1, new List<ModifierEffect>(), _context);

            NUnit.Framework.Assert.That(canApply, Is.True);
        }

        [Test]
        public async Task CanApplyAsync_WithInvalidNation_ReturnsFalse()
        {
            var canApply = await _processor.CanApplyAsync(999, new List<ModifierEffect>(), _context);

            NUnit.Framework.Assert.That(canApply, Is.False);
        }

        [Test]
        public void SupportedType_ReturnsPopulationHappiness()
        {
            NUnit.Framework.Assert.That(_processor.SupportedType, Is.EqualTo(ModifierType.PopulationHappiness));
        }
    }
}
