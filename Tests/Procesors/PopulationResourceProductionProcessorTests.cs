﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wg_backend_api.Data;
using Wg_backend_api.Logic.Modifiers.Processors;
using Wg_backend_api.Models;

namespace Tests.Procesors
{
    [TestFixture]
    public class PopulationResourceProductionProcessorTests
    {
        private GameDbContext _context;
        private PopulationResourceProductionProcessor _processor;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<GameDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GameDbContext(options, "Test");
            _processor = new PopulationResourceProductionProcessor(
                _context,
                NullLogger<PopulationResourceProductionProcessor>.Instance);

            SeedTestData();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        private void SeedTestData()
        {
            // 1. Wymagane dla Nation
            var religion = new Religion { Id = 1, Name = "TestReligion" };
            var culture = new Culture { Id = 1, Name = "TestCulture" };

            // 2. Nation
            var nation = new Nation
            {
                Id = 1,
                Name = "Polska",
                Color = "#FF0000",
                ReligionId = 1,
                CultureId = 1
            };

            // 3. Localisation (KLUCZOWE!)
            var location = new Localisation
            {
                Id = 1,
                Name = "TestLocation",
                NationId = 1  // ← Bez tego nie znajdzie populacji!
            };

            // 4. SocialGroup i Resource
            var socialGroup = new SocialGroup { Id = 1, Name = "Szlachta" };
            var resource = new Resource { Id = 1, Name = "Food" };

            // 5. Population
            var population = new Population
            {
                Id = 1,
                LocationId = 1,
                SocialGroupId = 1
            };

            // 6. ProductionShare
            var productionShare = new PopulationProductionShare
            {
                Id = 1,
                PopulationId = 1,
                ResourcesId = 1,  // Lub ResourceId - sprawdź model!
                Coefficient = 0.5f
            };

            _context.AddRange(religion, culture, nation, location, socialGroup,
                             resource, population, productionShare);
            _context.SaveChanges();
        }

        [Test]
        public async Task ProcessAsync_ProductionIncrease_UpdatesShare()
        {
            // Arrange
            var effects = new List<ModifierEffect>
        {
            new()
            {
                Operation = "Add",
                Value = 0.2f,
                Conditions = new Dictionary<string, object>
                {
                    ["SocialGroupId"] = 1,
                    ["ResourceId"] = 1
                    

                }
            }
        };

            // Act
            var result = await _processor.ProcessAsync(1, effects, _context);

            // Assert - dodaj debug
            Console.WriteLine($"Success: {result.Success}");
            Console.WriteLine($"Message: {result.Message}");

            Assert.That(result.Success, Is.True);

            var share = await _context.PopulationProductionShares.FindAsync(1);
            Assert.That(share.Coefficient, Is.EqualTo(0.7f).Within(0.01f));
        }

        [TestCase(0.1f, 0.6f, TestName = "Small production increase")]
        [TestCase(-0.2f, 0.3f, TestName = "Production decrease")]
        [TestCase(0.3f, 0.8f, TestName = "Large production increase")]
        [TestCase(-0.6f, 0.0f, TestName = "Production decrease with floor at 0")]
        public async Task ProcessAsync_VariousProductionChanges_UpdatesCorrectly(
            float changeValue, float expectedResult)
        {
            // Arrange
            var effects = new List<ModifierEffect>
        {
            new()
            {
                Operation = "Add",
                Value = changeValue,
                Conditions = new Dictionary<string, object>
                {
                    ["SocialGroupId"] = 1,  // Lub SocialGroupId
                    ["ResourceId"] = 1       // Lub ResourceId
                }
            }
        };

            // Act
            var result = await _processor.ProcessAsync(1, effects, _context);

            // Assert
            Assert.That(result.Success, Is.True, result.Message);

            var share = await _context.PopulationProductionShares.FindAsync(1);
            Assert.That(share.Coefficient, Is.EqualTo(expectedResult).Within(0.01f));
        }

        [Test]
        public async Task RevertAsync_ProductionChange_RestoresOriginalValue()
        {
            // Arrange
            var effects = new List<ModifierEffect>
        {
            new()
            {
                Operation = "Add",
                Value = 0.2f,
                Conditions = new Dictionary<string, object>
                {
                    ["SocialGroupId"] = 1,
                    ["ResourceId"] = 1
                }
            }
        };

            // Act
            await _processor.ProcessAsync(1, effects, _context);
            var result = await _processor.RevertAsync(1, effects, _context);

            // Assert
            Assert.That(result.Success, Is.True);

            var share = await _context.PopulationProductionShares.FindAsync(1);
            Assert.That(share.Coefficient, Is.EqualTo(0.5f).Within(0.01f));
        }
    }

}
