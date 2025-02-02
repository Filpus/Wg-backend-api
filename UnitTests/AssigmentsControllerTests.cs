using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wg_backend_api.Controllers;
using Wg_backend_api.Data;
using Wg_backend_api.Models;
using Xunit;

namespace Wg_backend_api.Tests
{
    public class AssignmentsControllerTests
    {
        private readonly AppDbContext _context;
        private readonly AssignmentsController _controller;

        public AssignmentsControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _controller = new AssignmentsController(_context);
        }

        [Fact]
        public async Task GetAssignments_ReturnsAllAssignments()
        {
            // Arrange
            var assignments = new List<Assignment>
            {
                new Assignment { NationId = 1, UserId = 1, DateAcquired = DateTime.UtcNow, IsActive = true },
                new Assignment { NationId = 2, UserId = 2, DateAcquired = DateTime.UtcNow, IsActive = false }
            };
            await _context.Assignment.AddRangeAsync(assignments);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetAssignment();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Assignment>>>(result);
            var returnValue = Assert.IsType<List<Assignment>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetAssignments_ReturnAllWhenEmpty()
        {
            // Arrange
            var assignments = new List<Assignment> { };
    
            await _context.Assignment.AddRangeAsync(assignments);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetAssignment();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Assignment>>>(result);
            var returnValue = Assert.IsType<List<Assignment>>(actionResult.Value);
            Assert.Equal(0, returnValue.Count);
        }

        [Fact]
        public async Task GetAssignment_ReturnsSingleAssignment_WhenIdIsValid()
        {
            // Arrange
            var assignment = new Assignment { NationId = 1, UserId = 1, DateAcquired = DateTime.UtcNow, IsActive = true };
            _context.Assignment.Add(assignment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetAssignment(assignment.Id);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Assignment>>(result);
            Assert.Equal(assignment.Id, actionResult.Value.Id);
        }

        [Fact]
        public async Task GetAssignment_ReturnsNotFound_WhenIdIsInvalid()
        {
            // Act
            var result = await _controller.GetAssignment(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostAssignment_AddsAssignmentsSuccessfully()
        {
            // Arrange
            var assignments = new Assignment[]
            {
                new Assignment { NationId = 1, UserId = 1, DateAcquired = DateTime.UtcNow, IsActive = true },
                new Assignment { NationId = 2, UserId = 2, DateAcquired = DateTime.UtcNow, IsActive = false }
            };

            // Act
            var result = await _controller.PostAssignment(assignments);

            // Assert
            Assert.IsType<OkResult>(result.Result);
            Assert.Equal(2, _context.Assignment.Count());
        }

        [Fact]
        public async Task PostAssignment_AddOnlyBadData()
        {
            // Arrange
            var assignments = new Assignment[]
            {
                new Assignment { NationId = -1, UserId = 1, DateAcquired = DateTime.UtcNow, IsActive = true }
            };

            // Act
            var result = await _controller.PostAssignment(assignments);

            // Assert
            Assert.IsType<BadRequestResult>(result.Result);
            Assert.Equal(0, _context.Assignment.Count());
        }

        [Fact]
        public async Task PostAssignment_AddSomeBadData()
        {
            // Arrange
            var assignments = new Assignment[]
            {
                new Assignment { NationId = -1, UserId = 1, DateAcquired = DateTime.UtcNow, IsActive = true },
                new Assignment { NationId = 1, UserId = 1, DateAcquired = DateTime.UtcNow, IsActive = true }
            };

            // Act
            var result = await _controller.PostAssignment(assignments);

            // Assert
            Assert.IsType<BadRequestResult>(result.Result);
            Assert.Equal(0, _context.Assignment.Count());
        }

        [Fact]
        public async Task PostAssignment_AddEmpty()
        {
            // Arrange
            var assignments = new Assignment[]{};

            // Act
            var result = await _controller.PostAssignment(assignments);

            // Assert
            Assert.IsType<OkResult>(result.Result);
            Assert.Equal(0, _context.Assignment.Count());
        }


    }
}
