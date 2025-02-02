using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wg_backend_api.Controllers;
using Wg_backend_api.Data;
using Wg_backend_api.Models;
using Xunit;

namespace Wg_backend_api.Tests
{
    public class ReligionControllerTests
    {
        private readonly Mock<AppDbContext> _mockContext;
        private readonly ReligionsControler _controller;

        public ReligionControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _mockContext = new Mock<AppDbContext>(options);
            _controller = new ReligionsControler(_mockContext.Object);
        }

        [Fact]
        public async Task GetReligions_ReturnsAllReligions()
        {
            // Arrange
            var religions = new List<Religion>
            {
                new Religion { Id = 1, Name = "Religion1" },
                new Religion { Id = 2, Name = "Religion2" }
            };

            _mockContext.Setup(m => m.Religions.ToListAsync())
                .ReturnsAsync(religions);

            // Act
            var result = await _controller.GetReligions(null);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Religion>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnValue = Assert.IsType<List<Religion>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetReligions_ReturnsReligionById()
        {
            // Arrange
            var religion = new Religion { Id = 1, Name = "Religion1" };

            _mockContext.Setup(m => m.Religions.FindAsync(1))
                .ReturnsAsync(religion);

            // Act
            var result = await _controller.GetReligions(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Religion>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnValue = Assert.IsType<List<Religion>>(okResult.Value);
            Assert.Single(returnValue);
            Assert.Equal("Religion1", returnValue[0].Name);
        }

        [Fact]
        public async Task PostReligions_AddsNewReligions()
        {
            // Arrange
            var religions = new List<Religion>
            {
                new Religion { Name = "Religion1" },
                new Religion { Name = "Religion2" }
            };

            // Act
            var result = await _controller.PostReligions(religions);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Religion>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnValue = Assert.IsType<List<Religion>>(createdAtActionResult.Value);
            Assert.Equal(2, returnValue.Count);
            _mockContext.Verify(m => m.Religions.AddRange(religions), Times.Once);
            _mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }
    }
}
