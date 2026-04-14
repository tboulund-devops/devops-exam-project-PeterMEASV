using System.Security.Claims;
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

public class AuthServiceTest(MyDbContext dbContext, IPasswordHasher<User> passwordHasher)
{
    private async Task ClearDatabaseAsync()
    {
        dbContext.UsersMovies.RemoveRange(dbContext.UsersMovies);
        dbContext.Movies.RemoveRange(dbContext.Movies);
        dbContext.Users.RemoveRange(dbContext.Users);
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnUser()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        var email = "test@example.com";
        var password = "TestPassword123!";
        
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            Password = passwordHasher.HashPassword(null!, password),
            Name = "testuser"
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(dbContext, mockLogger.Object, passwordHasher);
        var loginDto = new LoginDto(email, password);

        // Act
        var result = await authService.LoginAsync(loginDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(email, result.Email);
        
        // Verify logger was called with correct message
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Login attempt for email {email}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Login successful")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ShouldThrowException()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(dbContext, mockLogger.Object, passwordHasher);
        var loginDto = new LoginDto("nonexistent@example.com", "password123");

        // Act & Assert
        await Assert.ThrowsAsync<System.Security.Authentication.InvalidCredentialException>(
            () => authService.LoginAsync(loginDto));
        
        // Verify logger was called
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrowException()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        var email = "test@example.com";
        var password = "TestPassword123!";
        
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            Password = passwordHasher.HashPassword(null!, password),
            Name = "testuser"
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(dbContext, mockLogger.Object, passwordHasher);
        var loginDto = new LoginDto(email, "WrongPassword");

        // Act & Assert
        await Assert.ThrowsAsync<System.Security.Authentication.InvalidCredentialException>(
            () => authService.LoginAsync(loginDto));
        
        // Verify logger was called with invalid password message
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid password")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithNullEmail_ShouldThrowException()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(dbContext, mockLogger.Object, passwordHasher);
        var loginDto = new LoginDto(null!, "password");

        // Act & Assert
        await Assert.ThrowsAsync<System.Security.Authentication.InvalidCredentialException>(
            () => authService.LoginAsync(loginDto));
    }

    [Fact]
    public async Task LoginAsync_WithNullPassword_ShouldThrowException()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(dbContext, mockLogger.Object, passwordHasher);
        var loginDto = new LoginDto("test@example.com", null!);

        // Act & Assert
        await Assert.ThrowsAsync<System.Security.Authentication.InvalidCredentialException>(
            () => authService.LoginAsync(loginDto));
    }

    [Fact]
    public async Task GetUserInfo_WithValidClaims_ShouldReturnUser()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        var userId = Guid.NewGuid().ToString();
        var email = "test@example.com";
        
        var user = new User
        {
            Id = userId,
            Email = email,
            Password = passwordHasher.HashPassword(null!, "password"),
            Name = "testuser"
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(dbContext, mockLogger.Object, passwordHasher);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email)
        };
        
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = authService.GetUserInfo(principal);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal(email, result.Email);
    }

    [Fact]
    public async Task GetUserInfo_WithInvalidUserId_ShouldReturnNull()
    {
        // Arrange
        await ClearDatabaseAsync();
        
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(dbContext, mockLogger.Object, passwordHasher);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "nonexistent-user-id")
        };
        
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = authService.GetUserInfo(principal);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetUserInfo_WithNoClaims_ShouldReturnNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(dbContext, mockLogger.Object, passwordHasher);
        
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = authService.GetUserInfo(principal);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetUserInfo_WithNullPrincipal_ShouldReturnNull()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<AuthService>>();
        var authService = new AuthService(dbContext, mockLogger.Object, passwordHasher);

        // Act & Assert
        var result = authService.GetUserInfo(null!);
        Assert.Null(result);
    }
}