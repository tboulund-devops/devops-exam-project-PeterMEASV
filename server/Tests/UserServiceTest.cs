using api.Models;
using api.Security;
using api.Services.Classes;
using api.Services.Interfaces;
using efscaffold;
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

        var userDto = new CreateUserDTO(
            email: "newuser@example.com",
            password: "SecurePassword123!",
            name: "John Doe"
        );

        // Act
        var result = await userService.CreateUserAsync(userDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userDto.email, result.Email);
        Assert.Equal(userDto.name, result.Name);
        Assert.NotNull(result.Id);

        // Verify logger was called with creation messages
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Creating user {userDto.email}")),
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

        var userDto = new CreateUserDTO(
            email: "persist@example.com",
            password: "SecurePassword123!",
            name: "Jane Doe"
        );

        // Act
        var createdUser = await userService.CreateUserAsync(userDto);

        // Assert - Verify user is actually in the database
        var userInDb = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == userDto.email);
        Assert.NotNull(userInDb);
        Assert.Equal(createdUser.Id, userInDb.Id);
        Assert.Equal(userDto.email, userInDb.Email);
    }

    [Fact]
    public async Task CreateUserAsync_WithNullEmail_ShouldThrowArgumentException()
    {
        // Arrange
        await ClearDatabaseAsync();

        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(dbContext, mockLogger.Object, passwordHasher);

        var userDto = new CreateUserDTO(
            email: null!,
            password: "SecurePassword123!",
            name: "John Doe"
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

        var userDto = new CreateUserDTO(
            email: "",
            password: "SecurePassword123!",
            name: "John Doe"
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

        var userDto = new CreateUserDTO(
            email: "user@example.com",
            password: null!,
            name: "John Doe"
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

        var userDto = new CreateUserDTO(
            email: "user@example.com",
            password: "",
            name: "John Doe"
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
        var userDto1 = new CreateUserDTO(
            email: email,
            password: "SecurePassword123!",
            name: "First User"
        );

        var userDto2 = new CreateUserDTO(
            email: email,
            password: "DifferentPassword123!",
            name: "Second User"
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
        var userDto = new CreateUserDTO(
            email: "hashtest@example.com",
            password: password,
            name: "Test User"
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

        var userDto = new CreateUserDTO(
            email: "noname@example.com",
            password: "SecurePassword123!",
            name: null
        );

        // Act
        var result = await userService.CreateUserAsync(userDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userDto.email, result.Email);
        Assert.Equal(userDto.email, result.Name);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldGenerateUniqueIds()
    {
        // Arrange
        await ClearDatabaseAsync();

        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(dbContext, mockLogger.Object, passwordHasher);

        var userDto1 = new CreateUserDTO(
            email: "user1@example.com",
            password: "SecurePassword123!",
            name: "User One"
        );

        var userDto2 = new CreateUserDTO(
            email: "user2@example.com",
            password: "SecurePassword123!",
            name: "User Two"
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

        var userDto = new CreateUserDTO(
            email: "   ",
            password: "SecurePassword123!",
            name: "John Doe"
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

        var userDto = new CreateUserDTO(
            email: "user@example.com",
            password: "   ",
            name: "John Doe"
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => userService.CreateUserAsync(userDto));
    }
}