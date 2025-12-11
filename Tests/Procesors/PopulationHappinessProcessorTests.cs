using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wg_backend_api.Data;
using Wg_backend_api.Enums;
using Wg_backend_api.Logic.Modifiers.Processors;
using Wg_backend_api.Logic.Modifiers.ModifierConditions;
using Wg_backend_api.Models;

namespace Tests.Procesors
{
    [TestFixture]
    public class PopulationHappinessProcessorTests
    {
        private GameDbContext _context;
        private PopulationHappinessProcessor _processor;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<GameDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GameDbContext(options, "Test");
            _processor = new PopulationHappinessProcessor(_context);

            SeedTestData();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        private void SeedTestData()
        {
            var religion = new Religion
            {
                Id = 1,
                Name = "TestReligion"
            };

            var culture = new Culture
            {
                Id = 1,
                Name = "TestCulture"
            };

            var nation = new Nation
            {
                Id = 1,
                Name = "TestNation",
                Color = "#FF0000",
                ReligionId = 1,
                CultureId = 1
            };

            var location = new Localisation
            {
                Id = 1,
                Name = "TestLoc",
                NationId = 1
            };

            var socialGroup = new SocialGroup
            {
                Id = 1,
                Name = "Workers"
            };

            var population = new Population
            {
                Id = 1,
                LocationId = 1,
                SocialGroupId = 1,
                Happiness = 75f
            };

            _context.AddRange(religion, culture, nation, location, socialGroup, population);
            _context.SaveChanges();
        }

        [Test]
        public async Task ProcessAsync_HappinessIncrease_UpdatesHappinessCorrectly()
        {
            // Arrange
            var effects = new List<ModifierEffect>
        {
            new()
            {
                Operation = ModifierOperation.Add,
                Value = 10,
                Conditions = new PopulationConditions { SocialGroupId = 1 }
            }
        };

            // Act
            var result = await _processor.ProcessAsync(1, effects, _context);

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message.ToLower(), Does.Contain("happiness"));

            var updatedPopulation = await _context.Populations.FindAsync(1);
            Assert.That(updatedPopulation.Happiness, Is.EqualTo(85f).Within(0.01f));
        }

        [Test]
        public async Task ProcessAsync_InvalidSocialGroup_ReturnsNoAffectedEntities()
        {
            // Arrange
            var effects = new List<ModifierEffect>
            {
                new()
                {
                    Operation = ModifierOperation.Add,
                    Value = 10,
                    Conditions = new PopulationConditions { SocialGroupId = 999 }
                }
            };

            // Act
            var result = await _processor.ProcessAsync(1, effects, _context);

            // Assert
            // Jeśli procesor zwraca Success=false:
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Does.Contain("nie znaleziono").IgnoreCase);

            // Albo jeśli procesor zwraca Success=true ale 0 encji:
            // Assert.That(result.AffectedEntities["affected_Population_count"], Is.EqualTo(0));
        }

        [TestCase(10f, 85f, TestName = "Positive happiness change")]
        [TestCase(-5f, 70f, TestName = "Negative happiness change")]
        [TestCase(0f, 75f, TestName = "Zero happiness change")]
        public async Task ProcessAsync_VariousHappinessValues_UpdatesCorrectly(float change, float expected)
        {
            // Arrange
            var effects = new List<ModifierEffect>
        {
            new()
            {
                Operation = ModifierOperation.Add,
                Value = change,
                Conditions = new PopulationConditions { SocialGroupId = 1 }
            }
        };

            // Act
            var result = await _processor.ProcessAsync(1, effects, _context);

            // Assert
            Assert.That(result.Success, Is.True);

            var updatedPopulation = await _context.Populations.FindAsync(1);
            Assert.That(updatedPopulation.Happiness, Is.EqualTo(expected).Within(0.01f));
        }

        [Test]
        [Timeout(5000)]
        public async Task ProcessAsync_LargeDataset_CompletesInReasonableTime()
        {
            // Arrange - Create many populations
            for (int i = 2; i <= 100; i++)
            {
                _context.Populations.Add(new Population
                {
                    Id = i,
                    LocationId = 1,
                    SocialGroupId = 1,
                    Happiness = 50f
                });
            }
            _context.SaveChanges();

            var effects = new List<ModifierEffect>
        {
            new()
            {
                Operation = ModifierOperation.Add,
                Value = 5,
                Conditions = new PopulationConditions { SocialGroupId = 1 }
            }
        };

            // Act
            var result = await _processor.ProcessAsync(1, effects, _context);

            // Assert
            Assert.That(result.Success, Is.True);
            
            // The processor uses dynamic keys with timestamps, so we need to find the affected entities
            var affectedEntitiesKey = result.AffectedEntities.Keys.FirstOrDefault(k => k.StartsWith("affected_Population_"));
            Assert.That(affectedEntitiesKey, Is.Not.Null, "Expected to find an affected_Population key in results");
            
            var changeRecord = result.AffectedEntities[affectedEntitiesKey] as ModifierChangeRecord;
            Assert.That(changeRecord, Is.Not.Null);
            Assert.That(changeRecord.Change, Is.EqualTo(100));
        }
    }


}
