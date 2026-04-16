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
        dbContext.UsersUsers.RemoveRange(dbContext.UsersUsers);
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
            userId: "newuser",
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
            userId: "persist",
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
            userId: "null",
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
            userId: "empty",
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
            userId: "null",
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
            userId: "empty",
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
            userId: "first",
            Email: email,
            Password: "SecurePassword123!",
            Name: "First User"
        );

        var userDto2 = new CreateUserDto(
            userId: "second",
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
            userId: "hashtest",
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
            userId: "noname",
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
            userId: "unique",
            Email: "user1@example.com",
            Password: "SecurePassword123!",
            Name: "User One"
        );

        var userDto2 = new CreateUserDto(
            userId: "unique2",
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
            userId: "whitespace",
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
            userId: "whitespace",
            Email: "user@example.com",
            Password: "   ",
            Name: "John Doe"
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => userService.CreateUserAsync(userDto));
    }

    [Fact]
    public async Task GetAllFriendsForUser_WithNoFriends_ShouldReturnEmptyList()
    {
        // Arrange
        await ClearDatabaseAsync();

        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(dbContext, mockLogger.Object, passwordHasher);

        var userDto = new CreateUserDto(userId: "lonely", Email: "lonely@example.com", Password: "SecurePassword123!", Name: "Lonely User");
        await userService.CreateUserAsync(userDto);

        // Act
        var result = await userService.GetAllFriendsForUser("lonely");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllFriendsForUser_AfterAddingFriend_ShouldReturnFriend()
    {
        // Arrange
        await ClearDatabaseAsync();

        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(dbContext, mockLogger.Object, passwordHasher);

        await userService.CreateUserAsync(new CreateUserDto(userId: "userA", Email: "userA@example.com", Password: "SecurePassword123!", Name: "User A"));
        await userService.CreateUserAsync(new CreateUserDto(userId: "userB", Email: "userB@example.com", Password: "SecurePassword123!", Name: "User B"));

        await userService.AddFriendForUser("userA", "userB");

        // Act
        var friends = await userService.GetAllFriendsForUser("userA");

        // Assert
        Assert.Single(friends);
        Assert.Equal("userB", friends[0].Id);
    }

    [Fact]
    public async Task GetAllFriendsForUser_FriendshipIsBidirectional_ShouldReturnFriendForBothUsers()
    {
        // Arrange
        await ClearDatabaseAsync();

        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(dbContext, mockLogger.Object, passwordHasher);

        await userService.CreateUserAsync(new CreateUserDto(userId: "userA", Email: "userA@example.com", Password: "SecurePassword123!", Name: "User A"));
        await userService.CreateUserAsync(new CreateUserDto(userId: "userB", Email: "userB@example.com", Password: "SecurePassword123!", Name: "User B"));

        await userService.AddFriendForUser("userA", "userB");

        // Act
        var friendsOfA = await userService.GetAllFriendsForUser("userA");
        var friendsOfB = await userService.GetAllFriendsForUser("userB");

        // Assert
        Assert.Single(friendsOfA);
        Assert.Single(friendsOfB);
        Assert.Equal("userB", friendsOfA[0].Id);
        Assert.Equal("userA", friendsOfB[0].Id);
    }

    [Fact]
    public async Task GetAllFriendsForUser_WithNonExistentUser_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        await ClearDatabaseAsync();

        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(dbContext, mockLogger.Object, passwordHasher);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => userService.GetAllFriendsForUser("doesnotexist"));
    }
    

    [Fact]
    public async Task AddFriendForUser_WithValidIds_ShouldPersistFriendship()
    {
        // Arrange
        await ClearDatabaseAsync();

        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(dbContext, mockLogger.Object, passwordHasher);

        await userService.CreateUserAsync(new CreateUserDto(userId: "userA", Email: "userA@example.com", Password: "SecurePassword123!", Name: "User A"));
        await userService.CreateUserAsync(new CreateUserDto(userId: "userB", Email: "userB@example.com", Password: "SecurePassword123!", Name: "User B"));

        // Act
        await userService.AddFriendForUser("userA", "userB");

        // Assert
        var friendships = dbContext.UsersUsers.Where(uu =>
            (uu.UserId == "userA" && uu.FriendId == "userB") ||
            (uu.UserId == "userB" && uu.FriendId == "userA")).ToList();
        Assert.Equal(2, friendships.Count);
    }

    [Fact]
    public async Task AddFriendForUser_WithDuplicateFriendship_ShouldThrowInvalidOperationException()
    {
        // Arrange
        await ClearDatabaseAsync();

        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(dbContext, mockLogger.Object, passwordHasher);

        await userService.CreateUserAsync(new CreateUserDto(userId: "userA", Email: "userA@example.com", Password: "SecurePassword123!", Name: "User A"));
        await userService.CreateUserAsync(new CreateUserDto(userId: "userB", Email: "userB@example.com", Password: "SecurePassword123!", Name: "User B"));

        await userService.AddFriendForUser("userA", "userB");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => userService.AddFriendForUser("userA", "userB"));
    }

    [Fact]
    public async Task AddFriendForUser_WithNonExistentUser_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        await ClearDatabaseAsync();

        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(dbContext, mockLogger.Object, passwordHasher);

        await userService.CreateUserAsync(new CreateUserDto(userId: "userA", Email: "userA@example.com", Password: "SecurePassword123!", Name: "User A"));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => userService.AddFriendForUser("userA", "ghost"));
    }

    [Fact]
    public async Task AddFriendForUser_WithNonExistentRequestingUser_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        await ClearDatabaseAsync();

        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(dbContext, mockLogger.Object, passwordHasher);

        await userService.CreateUserAsync(new CreateUserDto(userId: "userB", Email: "userB@example.com", Password: "SecurePassword123!", Name: "User B"));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => userService.AddFriendForUser("ghost", "userB"));
    }
    

    [Fact]
    public async Task RemoveFriendForUser_WithExistingFriendship_ShouldRemoveFriendship()
    {
        // Arrange
        await ClearDatabaseAsync();

        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(dbContext, mockLogger.Object, passwordHasher);

        await userService.CreateUserAsync(new CreateUserDto(userId: "userA", Email: "userA@example.com", Password: "SecurePassword123!", Name: "User A"));
        await userService.CreateUserAsync(new CreateUserDto(userId: "userB", Email: "userB@example.com", Password: "SecurePassword123!", Name: "User B"));

        await userService.AddFriendForUser("userA", "userB");

        // Act
        await userService.RemoveFriendForUser("userA", "userB");

        // Assert
        var friends = await userService.GetAllFriendsForUser("userA");
        Assert.Empty(friends);
    }

    [Fact]
    public async Task RemoveFriendForUser_ShouldRemoveFriendshipInBothDirections()
    {
        // Arrange
        await ClearDatabaseAsync();

        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(dbContext, mockLogger.Object, passwordHasher);

        await userService.CreateUserAsync(new CreateUserDto(userId: "userA", Email: "userA@example.com", Password: "SecurePassword123!", Name: "User A"));
        await userService.CreateUserAsync(new CreateUserDto(userId: "userB", Email: "userB@example.com", Password: "SecurePassword123!", Name: "User B"));

        await userService.AddFriendForUser("userA", "userB");
        await userService.RemoveFriendForUser("userA", "userB");

        // Assert B no longer has A as friend either
        var friendsOfB = await userService.GetAllFriendsForUser("userB");
        Assert.Empty(friendsOfB);
    }

    [Fact]
    public async Task RemoveFriendForUser_WithNonExistentFriendship_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        await ClearDatabaseAsync();

        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(dbContext, mockLogger.Object, passwordHasher);

        await userService.CreateUserAsync(new CreateUserDto(userId: "userA", Email: "userA@example.com", Password: "SecurePassword123!", Name: "User A"));
        await userService.CreateUserAsync(new CreateUserDto(userId: "userB", Email: "userB@example.com", Password: "SecurePassword123!", Name: "User B"));

        // Act & Assert - no friendship exists yet
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => userService.RemoveFriendForUser("userA", "userB"));
    }
}