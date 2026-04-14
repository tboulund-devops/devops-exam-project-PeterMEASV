using api.Controllers;
using api.Models;
using api.Services.Interfaces;
using efscaffold;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests;

public class UserControllerTest
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<ILogger<UserController>> _mockLogger;
    private readonly UserController _controller;

    public UserControllerTest()
    {
        _mockUserService = new Mock<IUserService>();
        _mockLogger = new Mock<ILogger<UserController>>();
        _controller = new UserController(_mockUserService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateUser_WithValidData_ShouldReturnOkWithUser()
    {
        // Arrange
        var userDto = new CreateUserDTO("user@example.com", "SecurePassword123!", "John Doe");
        var createdUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = userDto.email,
            Name = userDto.name,
            Password = "hashed_password"
        };

        _mockUserService.Setup(s => s.CreateUserAsync(userDto)).ReturnsAsync(createdUser);

        // Act
        var result = await _controller.CreateUser(userDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(createdUser, okResult.Value);
        _mockUserService.Verify(s => s.CreateUserAsync(userDto), Times.Once);
    }

    [Fact]
    public async Task CreateUser_WithValidData_ShouldCallService()
    {
        // Arrange
        var userDto = new CreateUserDTO("user@example.com", "SecurePassword123!", "Jane Doe");
        var createdUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = userDto.email,
            Name = userDto.name,
            Password = "hashed_password"
        };

        _mockUserService.Setup(s => s.CreateUserAsync(userDto)).ReturnsAsync(createdUser);

        // Act
        await _controller.CreateUser(userDto);

        // Assert
        _mockUserService.Verify(s => s.CreateUserAsync(userDto), Times.Once);
    }

    [Fact]
    public async Task CreateUser_WithArgumentException_ShouldReturnBadRequest()
    {
        // Arrange
        var userDto = new CreateUserDTO("", "password", "John Doe");
        _mockUserService.Setup(s => s.CreateUserAsync(userDto))
            .ThrowsAsync(new ArgumentException("Fill out all fields"));

        // Act
        var result = await _controller.CreateUser(userDto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Fill out all fields", badRequest.Value);
    }

    [Fact]
    public async Task CreateUser_WithNullEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var userDto = new CreateUserDTO(null!, "SecurePassword123!", "John Doe");
        _mockUserService.Setup(s => s.CreateUserAsync(userDto))
            .ThrowsAsync(new ArgumentException("Fill out all fields"));

        // Act
        var result = await _controller.CreateUser(userDto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Fill out all fields", badRequest.Value);
    }

    [Fact]
    public async Task CreateUser_WithEmptyPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var userDto = new CreateUserDTO("user@example.com", "", "John Doe");
        _mockUserService.Setup(s => s.CreateUserAsync(userDto))
            .ThrowsAsync(new ArgumentException("Fill out all fields"));

        // Act
        var result = await _controller.CreateUser(userDto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Fill out all fields", badRequest.Value);
    }

    [Fact]
    public async Task CreateUser_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var userDto = new CreateUserDTO("duplicate@example.com", "SecurePassword123!", "John Doe");
        _mockUserService.Setup(s => s.CreateUserAsync(userDto))
            .ThrowsAsync(new InvalidOperationException("User already exists"));

        // Act
        var result = await _controller.CreateUser(userDto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("User already exists", badRequest.Value);
    }

    [Fact]
    public async Task CreateUser_WithInvalidOperationException_ShouldReturnBadRequest()
    {
        // Arrange
        var userDto = new CreateUserDTO("user@example.com", "SecurePassword123!", "John Doe");
        _mockUserService.Setup(s => s.CreateUserAsync(userDto))
            .ThrowsAsync(new InvalidOperationException("User already exists"));

        // Act
        var result = await _controller.CreateUser(userDto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("User already exists", badRequest.Value);
    }

    [Fact]
    public async Task CreateUser_WhenUnexpectedExceptionThrown_ShouldReturnInternalServerError()
    {
        // Arrange
        var userDto = new CreateUserDTO("user@example.com", "SecurePassword123!", "John Doe");
        _mockUserService.Setup(s => s.CreateUserAsync(userDto))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.CreateUser(userDto);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("An error occurred while creating the user", statusCodeResult.Value);
    }

    [Fact]
    public async Task CreateUser_WhenUnexpectedExceptionThrown_ShouldLogError()
    {
        // Arrange
        var userDto = new CreateUserDTO("user@example.com", "SecurePassword123!", "John Doe");
        var exception = new Exception("Unexpected error");
        _mockUserService.Setup(s => s.CreateUserAsync(userDto))
            .ThrowsAsync(exception);

        // Act
        await _controller.CreateUser(userDto);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error creating user")),
                It.Is<Exception>(e => e == exception),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnCreatedUserData()
    {
        // Arrange
        var userDto = new CreateUserDTO("test@example.com", "SecurePassword123!", "Test User");
        var userId = Guid.NewGuid().ToString();
        var createdUser = new User
        {
            Id = userId,
            Email = userDto.email,
            Name = userDto.name,
            Password = "hashed_password"
        };

        _mockUserService.Setup(s => s.CreateUserAsync(userDto)).ReturnsAsync(createdUser);

        // Act
        var result = await _controller.CreateUser(userDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedUser = Assert.IsType<User>(okResult.Value);
        Assert.Equal(userId, returnedUser.Id);
        Assert.Equal(userDto.email, returnedUser.Email);
        Assert.Equal(userDto.name, returnedUser.Name);
    }

    [Fact]
    public async Task CreateUser_WithWhitespaceEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var userDto = new CreateUserDTO("   ", "SecurePassword123!", "John Doe");
        _mockUserService.Setup(s => s.CreateUserAsync(userDto))
            .ThrowsAsync(new ArgumentException("Fill out all fields"));

        // Act
        var result = await _controller.CreateUser(userDto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Fill out all fields", badRequest.Value);
    }

    [Fact]
    public async Task CreateUser_WithWhitespacePassword_ShouldReturnBadRequest()
    {
        // Arrange
        var userDto = new CreateUserDTO("user@example.com", "   ", "John Doe");
        _mockUserService.Setup(s => s.CreateUserAsync(userDto))
            .ThrowsAsync(new ArgumentException("Fill out all fields"));

        // Act
        var result = await _controller.CreateUser(userDto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Fill out all fields", badRequest.Value);
    }

    [Fact]
    public async Task CreateUser_WithoutName_ShouldSucceed()
    {
        // Arrange
        var userDto = new CreateUserDTO("user@example.com", "SecurePassword123!", null);
        var createdUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = userDto.email,
            Name = null,
            Password = "hashed_password"
        };

        _mockUserService.Setup(s => s.CreateUserAsync(userDto)).ReturnsAsync(createdUser);

        // Act
        var result = await _controller.CreateUser(userDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedUser = Assert.IsType<User>(okResult.Value);
        Assert.Null(returnedUser.Name);
    }
}