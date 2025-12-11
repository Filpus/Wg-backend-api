using Microsoft.EntityFrameworkCore;
using Wg_backend_api.Data;
using Wg_backend_api.DTO;
using Wg_backend_api.Enums;
using Wg_backend_api.Logic.Modifiers.ModifierConditions;
using Wg_backend_api.Logic.Modifiers.Processors;
using Wg_backend_api.Models;

namespace Tests.Procesors
{
    public class ResourceChangeProcessorTests
    {
        private GameDbContext _context;
        private ResourceChangeProcessor _processor;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<GameDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GameDbContext(options, "test");
            _processor = new ResourceChangeProcessor(_context);

            SeedTestData();
        }
        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }
        private void SeedTestData()
        {
            // Seed test data
            var nation = new Nation { Id = 1, Name = "TestNation", Color = "XXX" };
            var location = new Localisation { Id = 1, NationId = 1, Name = "TestLocation" };
            var resource = new Resource { Id = 1, Name = "Food" };
            var locResource = new LocalisationResource
            {
                Id = 1,
                LocationId = 1,
                ResourceId = 1,
                Amount = 100f,
                Location = location
            };

            var eventEntity = new Wg_backend_api.Models.Event { Id = 1, Name = "TestEvent" };
            var modifier = new Modifiers
            {
                Id = 1,
                EventId = 1,
                ModifierType = ModifierType.ResourceChange,
                Effects = new ModifierEffect() { Operation = ModifierOperation.Add, Value = 50, Conditions = new ResourceConditions() { ResourceId = 1 } }
            };
            var relatedEvent = new RelatedEvents { EventId = 1, NationId = 1 };

            _context.AddRange(nation, location, resource, locResource, eventEntity, modifier, relatedEvent);
            _context.SaveChanges();
        }

        [Test]
        public async Task CalculateChangeAsync_AddOperation_ReturnsCorrectValue()
        {
            // Arrange
            var balance = new ResourceBalanceDto { ResourceId = 1 };

            // Act
            var result = await _processor.CalculateChangeAsync(1, balance);

            // Assert
            Assert.AreEqual(50f, result);
        }

        [Test]
        public async Task CalculateChangeAsync_NoModifiers_ReturnsZero()
        {
            // Arrange
            var balance = new ResourceBalanceDto { ResourceId = 999 }; // Non-existing resource

            // Act
            var result = await _processor.CalculateChangeAsync(1, balance);

            // Assert
            Assert.AreEqual(0f, result);
        }

        [Test]
        public async Task CalculateChangeAsync_PercentageOperation_ReturnsCorrectValue()
        {
            // Arrange
            var modifier = new Modifiers
            {
                Id = 2,
                EventId = 1,
                ModifierType = ModifierType.ResourceChange,
                Effects = new ModifierEffect() { Operation = ModifierOperation.Percentage, Value = 20, Conditions = new ResourceConditions() { ResourceId = 1 } }
            };
            _context.Modifiers.Add(modifier);
            _context.SaveChanges();

            var balance = new ResourceBalanceDto { ResourceId = 1 };

            // Act
            var result = await _processor.CalculateChangeAsync(1, balance);

            // Assert
            Assert.AreEqual(70f, result); // 50 (Add) + 20% of 100 = 70
        }
    }

}