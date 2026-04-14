using api.Models;
using api.Security;
using api.Services.Classes;
using api.Services.Interfaces;
using efscaffold;
using efscaffold.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests;

public class UserServiceTest(MyDbContext dbContext, IPasswordHasher<User> passwordHasher)
{
    private async Task ClearDatabaseAsync()
    {
        dbContext.UsersMovies.RemoveRange(dbContext.UsersMovies);
        dbContext.Movies.RemoveRange(dbContext.Movies);
        dbContext.Users.RemoveRange(dbContext.Users);
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateUserAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange
        await ClearDatabaseAsync();

        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(dbContext, mockLogger.Object, passwordHasher);

        var userDto = new CreateUserDto(
            Email: "newuser@example.com",
            Password: "SecurePassword123!",
            Name: "John Doe"
        );

        // Act
        var result = await userService.CreateUserAsync(userDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userDto.Email, result.Email);
        Assert.Equal(userDto.Name, result.Name);
        Assert.NotNull(result.Id);

        // Verify logger was called with creation messages
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Creating user {userDto.Email}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Created user")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_WithValidData_ShouldPersistToDatabase()
    {
        // Arrange
        await ClearDatabaseAsync();

        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(dbContext, mockLogger.Object, passwordHasher);

        var userDto = new CreateUserDto(
            Email: "persist@example.com",
            Password: "SecurePassword123!",
            Name: "Jane Doe"
        );

        // Act
        var createdUser = await userService.CreateUserAsync(userDto);

        // Assert - Verify user is actually in the database
        var userInDb = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email);
        Assert.NotNull(userInDb);
        Assert.Equal(createdUser.Id, userInDb.Id);
        Assert.Equal(userDto.Email, userInDb.Email);
    }

    [Fact]
    public async Task CreateUserAsync_WithNullEmail_ShouldThrowArgumentException()
    {
        // Arrange
        await ClearDatabaseAsync();

        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(dbContext, mockLogger.Object, passwordHasher);

        var userDto = new CreateUserDto(
            Email: null!,
            Password: "SecurePassword123!",
            Name: "John Doe"
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => userService.CreateUserAsync(userDto));
    }

    [Fact]
    public async Task CreateUserAsync_WithEmptyEmail_ShouldThrowArgumentException()
    {
        // Arrange
        await ClearDatabaseAsync();

        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(dbContext, mockLogger.Object, passwordHasher);

        var userDto = new CreateUserDto(
            Email: "",
            Password: "SecurePassword123!",
            Name: "John Doe"
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => userService.CreateUserAsync(userDto));
    }

    [Fact]
    public async Task CreateUserAsync_WithNullPassword_ShouldThrowArgumentException()
    {
        // Arrange
        await ClearDatabaseAsync();

        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(dbContext, mockLogger.Object, passwordHasher);

        var userDto = new CreateUserDto(
            Email: "user@example.com",
            Password: null!,
            Name: "John Doe"
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => userService.CreateUserAsync(userDto));
    }

    [Fact]
    public async Task CreateUserAsync_WithEmptyPassword_ShouldThrowArgumentException()
    {
        // Arrange
        await ClearDatabaseAsync();

        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(dbContext, mockLogger.Object, passwordHasher);

        var userDto = new CreateUserDto(
            Email: "user@example.com",
            Password: "",
            Name: "John Doe"
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => userService.CreateUserAsync(userDto));
    }

    [Fact]
    public async Task CreateUserAsync_WithDuplicateEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        await ClearDatabaseAsync();

        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(dbContext, mockLogger.Object, passwordHasher);

        var email = "duplicate@example.com";
        var userDto1 = new CreateUserDto(
            Email: email,
            Password: "SecurePassword123!",
            Name: "First User"
        );

        var userDto2 = new CreateUserDto(
            Email: email,
            Password: "DifferentPassword123!",
            Name: "Second User"
        );

        // Create first user
        await userService.CreateUserAsync(userDto1);

        // Act & Assert - Try to create user with same email
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => userService.CreateUserAsync(userDto2));
    }

    [Fact]
    public async Task CreateUserAsync_ShouldHashPassword()
    {
        // Arrange
        await ClearDatabaseAsync();

        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(dbContext, mockLogger.Object, passwordHasher);

        var password = "SecurePassword123!";
        var userDto = new CreateUserDto(
            Email: "hashtest@example.com",
            Password: password,
            Name: "Test User"
        );

        // Act
        var result = await userService.CreateUserAsync(userDto);

        // Assert
        Assert.NotNull(result.Password);
        // Password should be hashed, not plaintext
        Assert.NotEqual(password, result.Password);
        // Verify password starts with argon2id marker
        Assert.StartsWith("argon2id$", result.Password);
    }

    [Fact]
    public async Task CreateUserAsync_WithoutName_ShouldCreateUserSuccessfully()
    {
        // Arrange
        await ClearDatabaseAsync();

        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(dbContext, mockLogger.Object, passwordHasher);

        var userDto = new CreateUserDto(
            Email: "noname@example.com",
            Password: "SecurePassword123!",
            Name: null
        );

        // Act
        var result = await userService.CreateUserAsync(userDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userDto.Email, result.Email);
        Assert.Equal(userDto.Email, result.Name);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldGenerateUniqueIds()
    {
        // Arrange
        await ClearDatabaseAsync();

        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(dbContext, mockLogger.Object, passwordHasher);

        var userDto1 = new CreateUserDto(
            Email: "user1@example.com",
            Password: "SecurePassword123!",
            Name: "User One"
        );

        var userDto2 = new CreateUserDto(
            Email: "user2@example.com",
            Password: "SecurePassword123!",
            Name: "User Two"
        );

        // Act
        var result1 = await userService.CreateUserAsync(userDto1);
        var result2 = await userService.CreateUserAsync(userDto2);

        // Assert
        Assert.NotEqual(result1.Id, result2.Id);
    }

    [Fact]
    public async Task CreateUserAsync_WithWhitespaceEmail_ShouldThrowArgumentException()
    {
        // Arrange
        await ClearDatabaseAsync();

        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(dbContext, mockLogger.Object, passwordHasher);

        var userDto = new CreateUserDto(
            Email: "   ",
            Password: "SecurePassword123!",
            Name: "John Doe"
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => userService.CreateUserAsync(userDto));
    }

    [Fact]
    public async Task CreateUserAsync_WithWhitespacePassword_ShouldThrowArgumentException()
    {
        // Arrange
        await ClearDatabaseAsync();

        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(dbContext, mockLogger.Object, passwordHasher);

        var userDto = new CreateUserDto(
            Email: "user@example.com",
            Password: "   ",
            Name: "John Doe"
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => userService.CreateUserAsync(userDto));
    }
}