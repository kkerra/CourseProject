using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using TelemarketingApp.WebApi.Controllers;
using TelemarketingApp.WebApi.DataContexts;
using TelemarketingApp.WebApi.DTOs;
using TelemarketingApp.WebApi.Models;

namespace TelemarketingApp.Test
{
    public class ClientsControllerTests
    {
        private readonly Mock<AppDbContext> _mockContext;
        private readonly ClientsController _controller;

        public ClientsControllerTests()
        {
            _mockContext = new Mock<AppDbContext>();
            _controller = new ClientsController(_mockContext.Object);
        }

        [Fact]
        public async Task CreateClient_ReturnsBadRequest_WhenSurnameIsEmpty()
        {
            // Arrange
            var request = new CreateClientRequest
            {
                Surname = "",
                Name = "John",
                PhoneNumber = "1234567890"
            };

            // Act
            var result = await _controller.CreateClient(request);

            // Assert
            var badRequestResult = Assert.IsType<ActionResult<Client>>(result);
            var response = badRequestResult.Result as BadRequestObjectResult;

            Assert.NotNull(response); // Ensure that response is not null
            var message = response?.Value?.GetType().GetProperty("message")?.GetValue(response?.Value, null);

            Assert.Equal("Фамилия, имя и телефон обязательны", message);
        }
    }
}