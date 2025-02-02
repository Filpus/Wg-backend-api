using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Wg_backend_api.Controllers;
using Wg_backend_api.Data;
using Wg_backend_api.Models;
using Xunit;


namespace Wg_backend_api.Tests
{
    public class ReligionsControllerTests
    {
        private readonly AppDbContext _context;
        private readonly ReligionsControler _controller;

        public ReligionsControllerTests()
        {
            // Używamy InMemoryDatabase tylko dla testów
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unikaj konfliktów nazw baz danych
                .Options;

            _context = new AppDbContext(options);
            _controller = new ReligionsControler(_context);
        }

        [Fact]
        public async Task GetReligions_ReturnsAllReligions_WhenNoIdIsPassed()
        {
            // Arrange
            var religions = new List<Religion>
            {
                new Religion { Name = "Religion1" },
                new Religion { Name = "Religion2" }
            };
            await _context.Religions.AddRangeAsync(religions);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetReligions(null);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Religion>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnValue = Assert.IsType<List<Religion>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetReligions_ReturnsSingleReligion_WhenIdIsPassed()
        {
            // Arrange
            var religion = new Religion { Name = "Religion1" };
            _context.Religions.Add(religion);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetReligions(religion.Id);

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Religion>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnValue = Assert.IsType<List<Religion>>(okResult.Value);
            Assert.Single(returnValue);
            Assert.Equal("Religion1", returnValue[0].Name);
        }
        [Fact]
        public async Task GetRelions_ReturnsNotFound_WhenIdIsInvalid()
        {
            // Act
            var result = await _controller.GetReligions(1);
            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }
        [Fact]
        public async Task GetReligions_ReturnEmptyArray()
        {
            // Act
            var result = await _controller.GetReligions(null);
            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Religion>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnValue = Assert.IsType<List<Religion>>(okResult.Value);
            Assert.Empty(returnValue);
        }
        [Fact]
        public async Task PostReligions_ReturnsCreatedAtActionResult_WhenReligionsAreValid()
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
            Assert.Equal("Religion1", returnValue[0].Name);
            Assert.Equal("Religion2", returnValue[1].Name);
        }
        [Fact]
        public async Task PostReligions_PostOneReligionWhenValid()
        {
            // Arrange
            var religions = new List<Religion>
            {
                new Religion { Name = "Religion1" }
            };
            // Act
            var result = await _controller.PostReligions(religions);
            // Assert
            var actionResult = Assert.IsType<ActionResult<Religion>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnValue = Assert.IsType<List<Religion>>(createdAtActionResult.Value);
            Assert.Single(returnValue);
            Assert.Equal("Religion1", returnValue[0].Name);
        }

        [Fact]
        public async Task PostReligions_WrongReligionsDataType()
        {
            // Arrange
            var religions = new List<Religion>
            {
                new Religion { Name = null }
            };
            // Act
            var result = await _controller.PostReligions(religions);
            // Assert
            var actionResult = Assert.IsType<ActionResult<Religion>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal("Brak nazwy religii.", badRequestResult.Value);
        }

        [Fact]
        public async Task PostReligions_OneWrogReligionInList()
        {
            // Arrange
            var religions = new List<Religion>
            {
                new Religion { Name = "Religion1" },
                new Religion { Name = null }
            };
            // Act
            var result = await _controller.PostReligions(religions);
            // Assert
            var actionResult = Assert.IsType<ActionResult<Religion>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal("Brak nazwy religii.", badRequestResult.Value);
        }

        [Fact]
        public async Task PostReligions_emptyList()
        {
            // Act
            var result = await _controller.PostReligions(new List<Religion>());
            // Assert
            var actionResult = Assert.IsType<ActionResult<Religion>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal("Brak danych do zapisania.", badRequestResult.Value);
        }
            [Fact]
        public async Task PostReligions_ReturnsBadRequest_WhenNoReligionsArePassed()
        {
            // Act
            var result = await _controller.PostReligions(new List<Religion>());

            // Assert
            var actionResult = Assert.IsType<ActionResult<Religion>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal("Brak danych do zapisania.", badRequestResult.Value);
        }
    }
}
