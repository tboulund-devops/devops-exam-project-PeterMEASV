using api.Controllers;
using api.Models;
using api.Services.Interfaces;
using efscaffold;
using efscaffold.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

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
        var userDto = new CreateUserDto("user1","user@example.com", "SecurePassword123!", "John Doe");
        var createdUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = userDto.Email,
            Name = userDto.Name ?? userDto.Email,
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
        var userDto = new CreateUserDto("user1","user@example.com", "SecurePassword123!", "Jane Doe");
        var createdUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = userDto.Email,
            Name = userDto.Name ?? userDto.Email,
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
        var userDto = new CreateUserDto("user1","", "password", "John Doe");
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
        var userDto = new CreateUserDto("user1",null!, "SecurePassword123!", "John Doe");
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
        var userDto = new CreateUserDto("user1","user@example.com", "", "John Doe");
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
        var userDto = new CreateUserDto("user1","duplicate@example.com", "SecurePassword123!", "John Doe");
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
        var userDto = new CreateUserDto("user1","user@example.com", "SecurePassword123!", "John Doe");
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
        var userDto = new CreateUserDto("user1","user@example.com", "SecurePassword123!", "John Doe");
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
        var userDto = new CreateUserDto("user1","user@example.com", "SecurePassword123!", "John Doe");
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
        var userDto = new CreateUserDto("user1","test@example.com", "SecurePassword123!", "Test User");
        var userId = Guid.NewGuid().ToString();
        var createdUser = new User
        {
            Id = userId,
            Email = userDto.Email,
            Name = userDto.Name ?? userDto.Email,
            Password = "hashed_password"
        };

        _mockUserService.Setup(s => s.CreateUserAsync(userDto)).ReturnsAsync(createdUser);

        // Act
        var result = await _controller.CreateUser(userDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedUser = Assert.IsType<User>(okResult.Value);
        Assert.Equal(userId, returnedUser.Id);
        Assert.Equal(userDto.Email, returnedUser.Email);
        Assert.Equal(userDto.Name, returnedUser.Name);
    }

    [Fact]
    public async Task CreateUser_WithWhitespaceEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var userDto = new CreateUserDto("user1","   ", "SecurePassword123!", "John Doe");
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
        var userDto = new CreateUserDto("user1","user@example.com", "   ", "John Doe");
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
        var userDto = new CreateUserDto("user1","user@example.com", "SecurePassword123!", null);
        var createdUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = userDto.Email,
            Name = "",
            Password = "hashed_password"
        };

        _mockUserService.Setup(s => s.CreateUserAsync(userDto)).ReturnsAsync(createdUser);

        // Act
        var result = await _controller.CreateUser(userDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedUser = Assert.IsType<User>(okResult.Value);
        Assert.Equal("", returnedUser.Name);
    }

    [Fact]
    public async Task GetAllFriendsForUser_WithValidUserId_ShouldReturnOkWithFriends()
    {
        // Arrange
        var userId = "user1";
        var friends = new List<User>
        {
            new User { Id = "friend1", Email = "friend1@example.com", Name = "Friend One", Password = "hashed" },
            new User { Id = "friend2", Email = "friend2@example.com", Name = "Friend Two", Password = "hashed" }
        };

        foreach (var friend in friends)
        {
            _mockUserService.Setup(s => s.AddFriendForUser(userId, friend.Id)).Returns(Task.CompletedTask);
        }

        _mockUserService.Setup(s => s.GetAllFriendsForUser(userId)).ReturnsAsync(friends);

        // Act
        var result = await _controller.GetAllFriendsForUser(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedFriends = Assert.IsType<List<User>>(okResult.Value);
        Assert.Equal(2, returnedFriends.Count);
    }

    [Fact]
    public async Task GetAllFriendsForUser_WithValidUserId_ShouldCallService()
    {
        // Arrange
        var userId = "user1";
        _mockUserService.Setup(s => s.GetAllFriendsForUser(userId)).ReturnsAsync(new List<User>());

        // Act
        await _controller.GetAllFriendsForUser(userId);

        // Assert
        _mockUserService.Verify(s => s.GetAllFriendsForUser(userId), Times.Once);
    }

    [Fact]
    public async Task GetAllFriendsForUser_WithNoFriends_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        var userId = "lonelyuser";
        _mockUserService.Setup(s => s.GetAllFriendsForUser(userId)).ReturnsAsync(new List<User>());

        // Act
        var result = await _controller.GetAllFriendsForUser(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedFriends = Assert.IsType<List<User>>(okResult.Value);
        Assert.Empty(returnedFriends);
    }

    [Fact]
    public async Task GetAllFriendsForUser_WhenUnexpectedExceptionThrown_ShouldReturnInternalServerError()
    {
        // Arrange
        var userId = "user1";
        _mockUserService.Setup(s => s.GetAllFriendsForUser(userId))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.GetAllFriendsForUser(userId);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("An error occurred while getting friends", statusCodeResult.Value);
    }

    [Fact]
    public async Task GetAllFriendsForUser_WhenUnexpectedExceptionThrown_ShouldLogError()
    {
        // Arrange
        var userId = "user1";
        var exception = new Exception("Unexpected error");
        _mockUserService.Setup(s => s.GetAllFriendsForUser(userId)).ThrowsAsync(exception);

        // Act
        await _controller.GetAllFriendsForUser(userId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error getting friends")),
                It.Is<Exception>(e => e == exception),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task AddFriendForUser_WithValidIds_ShouldReturnOk()
    {
        // Arrange
        var userId = "user1";
        var friendId = "friend1";
        _mockUserService.Setup(s => s.AddFriendForUser(userId, friendId)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AddFriendForUser(userId, friendId);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task AddFriendForUser_WithValidIds_ShouldCallService()
    {
        // Arrange
        var userId = "user1";
        var friendId = "friend1";
        _mockUserService.Setup(s => s.AddFriendForUser(userId, friendId)).Returns(Task.CompletedTask);

        // Act
        await _controller.AddFriendForUser(userId, friendId);

        // Assert
        _mockUserService.Verify(s => s.AddFriendForUser(userId, friendId), Times.Once);
    }

    [Fact]
    public async Task AddFriendForUser_WhenUnexpectedExceptionThrown_ShouldReturnInternalServerError()
    {
        // Arrange
        var userId = "user1";
        var friendId = "friend1";
        _mockUserService.Setup(s => s.AddFriendForUser(userId, friendId))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.AddFriendForUser(userId, friendId);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("An error occurred while adding friend", statusCodeResult.Value);
    }

    [Fact]
    public async Task AddFriendForUser_WhenUnexpectedExceptionThrown_ShouldLogError()
    {
        // Arrange
        var userId = "user1";
        var friendId = "friend1";
        var exception = new Exception("Unexpected error");
        _mockUserService.Setup(s => s.AddFriendForUser(userId, friendId)).ThrowsAsync(exception);

        // Act
        await _controller.AddFriendForUser(userId, friendId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error adding friend")),
                It.Is<Exception>(e => e == exception),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveFriendForUser_WithValidIds_ShouldReturnOk()
    {
        // Arrange
        var userId = "user1";
        var friendId = "friend1";
        _mockUserService.Setup(s => s.RemoveFriendForUser(userId, friendId)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RemoveFriendForUser(userId, friendId);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task RemoveFriendForUser_WithValidIds_ShouldCallService()
    {
        // Arrange
        var userId = "user1";
        var friendId = "friend1";
        _mockUserService.Setup(s => s.RemoveFriendForUser(userId, friendId)).Returns(Task.CompletedTask);

        // Act
        await _controller.RemoveFriendForUser(userId, friendId);

        // Assert
        _mockUserService.Verify(s => s.RemoveFriendForUser(userId, friendId), Times.Once);
    }

    [Fact]
    public async Task RemoveFriendForUser_WhenUnexpectedExceptionThrown_ShouldReturnInternalServerError()
    {
        // Arrange
        var userId = "user1";
        var friendId = "friend1";
        _mockUserService.Setup(s => s.RemoveFriendForUser(userId, friendId))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.RemoveFriendForUser(userId, friendId);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("An error occurred while removing friend", statusCodeResult.Value);
    }

    [Fact]
    public async Task RemoveFriendForUser_WhenUnexpectedExceptionThrown_ShouldLogError()
    {
        // Arrange
        var userId = "user1";
        var friendId = "friend1";
        var exception = new Exception("Unexpected error");
        _mockUserService.Setup(s => s.RemoveFriendForUser(userId, friendId)).ThrowsAsync(exception);

        // Act
        await _controller.RemoveFriendForUser(userId, friendId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error removing friend")),
                It.Is<Exception>(e => e == exception),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}