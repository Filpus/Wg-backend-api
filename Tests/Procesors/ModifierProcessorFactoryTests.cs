using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wg_backend_api.Data;
using Wg_backend_api.Enums;
using Wg_backend_api.Logic.Modifiers;
using Wg_backend_api.Logic.Modifiers.Processors;

namespace Tests.Procesors
{
    [TestFixture]
    public class ModifierProcessorFactoryTests
    {
        private ServiceProvider _serviceProvider;
        private ModifierProcessorFactory _factory;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var services = new ServiceCollection();

            // Register all processors
            services.AddScoped<ResourceChangeProcessor>();
            services.AddScoped<PopulationHappinessProcessor>();
            services.AddScoped<PopulationResourceProductionProcessor>();
            services.AddScoped<PopulationResourceUsageProcessor>();
            services.AddScoped<PopulationVolunteerProcessor>();
            services.AddScoped<FactionPowerProcessor>();
            services.AddScoped<FactionContentmentProcessor>();

            // Add DbContext
            services.AddDbContext<GameDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

            services.AddLogging();

            _serviceProvider = services.BuildServiceProvider();
            _factory = new ModifierProcessorFactory(_serviceProvider);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _serviceProvider?.Dispose();
        }

        [Test]
        public void GetProcessor_ResourceChange_ReturnsCorrectType()
        {
            // Act
            var processor = _factory.GetProcessor(ModifierType.ResourceChange);

            // Assert
            Assert.That(processor, Is.InstanceOf<ResourceChangeProcessor>());
        }

        [Test]
        public void GetProcessor_PopulationHappiness_ReturnsCorrectType()
        {
            // Act
            var processor = _factory.GetProcessor(ModifierType.PopulationHappiness);

            // Assert
            Assert.That(processor, Is.InstanceOf<PopulationHappinessProcessor>());
        }

        [TestCase(ModifierType.ResourceProduction, typeof(PopulationResourceProductionProcessor))]
        [TestCase(ModifierType.ResouerceUsage, typeof(PopulationResourceUsageProcessor))]
        [TestCase(ModifierType.VoluneerChange, typeof(PopulationVolunteerProcessor))]
        [TestCase(ModifierType.FactionPower, typeof(FactionPowerProcessor))]
        [TestCase(ModifierType.FactionContenment, typeof(FactionContentmentProcessor))]
        public void GetProcessor_AllTypes_ReturnsCorrectProcessors(ModifierType type, Type expectedType)
        {
            // Act
            var processor = _factory.GetProcessor(type);

            // Assert
            Assert.That(processor, Is.InstanceOf(expectedType));
        }

        [Test]
        public void GetProcessor_InvalidType_ThrowsNotSupportedException()
        {
            // Arrange
            var invalidType = (ModifierType)999;

            // Act & Assert
            var exception = Assert.Throws<NotSupportedException>(() => _factory.GetProcessor(invalidType));
            Assert.That(exception.Message, Does.Contain("Nieobsługiwany typ modyfikatora"));
        }


    }

}
