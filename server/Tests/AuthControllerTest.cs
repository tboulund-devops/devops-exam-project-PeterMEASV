using System.Security.Authentication;
using System.Security.Claims;
using api.Controllers;
using api.Models;
using api.Security;
using api.Services.Interfaces;
using efscaffold;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests;

public class AuthControllerTest
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;

    public AuthControllerTest()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockTokenService = new Mock<ITokenService>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_mockAuthService.Object, _mockLogger.Object, _mockTokenService.Object);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkWithToken()
    {
        // Arrange
        var loginDto = new LoginDTO("user@example.com", "SecurePassword123!");
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = loginDto.Email,
            Name = "John Doe",
            Password = "hashed_password"
        };
        var token = "jwt_token_here";

        _mockAuthService.Setup(s => s.LoginAsync(loginDto)).ReturnsAsync(user);
        _mockTokenService.Setup(t => t.CreateToken(user)).Returns(token);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<LoginResponseDTO>(okResult.Value);
        Assert.Equal(user.Id, response.id);
        Assert.Equal(user.Email, response.email);
        Assert.Equal(token, response.token);
        Assert.Equal("Login successful", response.message);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldCallAuthService()
    {
        // Arrange
        var loginDto = new LoginDTO("user@example.com", "SecurePassword123!");
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = loginDto.Email,
            Name = "John Doe",
            Password = "hashed_password"
        };

        _mockAuthService.Setup(s => s.LoginAsync(loginDto)).ReturnsAsync(user);
        _mockTokenService.Setup(t => t.CreateToken(user)).Returns("token");

        // Act
        await _controller.Login(loginDto);

        // Assert
        _mockAuthService.Verify(s => s.LoginAsync(loginDto), Times.Once);
        _mockTokenService.Verify(t => t.CreateToken(user), Times.Once);
    }

    [Fact]
    public async Task Login_WithNullEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var loginDto = new LoginDTO(null!, "SecurePassword123!");

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Email and password are required", badRequest.Value);
        _mockAuthService.Verify(s => s.LoginAsync(It.IsAny<LoginDTO>()), Times.Never);
    }

    [Fact]
    public async Task Login_WithEmptyEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var loginDto = new LoginDTO("", "SecurePassword123!");

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Email and password are required", badRequest.Value);
    }

    [Fact]
    public async Task Login_WithWhitespaceEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var loginDto = new LoginDTO("   ", "SecurePassword123!");

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Email and password are required", badRequest.Value);
    }

    [Fact]
    public async Task Login_WithNullPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var loginDto = new LoginDTO("user@example.com", null!);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Email and password are required", badRequest.Value);
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var loginDto = new LoginDTO("user@example.com", "");

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Email and password are required", badRequest.Value);
    }

    [Fact]
    public async Task Login_WithWhitespacePassword_ShouldReturnBadRequest()
    {
        // Arrange
        var loginDto = new LoginDTO("user@example.com", "   ");

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Email and password are required", badRequest.Value);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDTO("user@example.com", "WrongPassword");
        _mockAuthService.Setup(s => s.LoginAsync(loginDto))
            .ThrowsAsync(new InvalidCredentialException("Invalid email or password"));

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.Equal("Invalid email or password", unauthorizedResult.Value);
    }

    [Fact]
    public async Task Login_WithNonexistentUser_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDTO("nonexistent@example.com", "SecurePassword123!");
        _mockAuthService.Setup(s => s.LoginAsync(loginDto))
            .ThrowsAsync(new InvalidCredentialException("Invalid email or password"));

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.Equal("Invalid email or password", unauthorizedResult.Value);
    }

    [Fact]
    public async Task Login_WhenUnexpectedExceptionThrown_ShouldReturnInternalServerError()
    {
        // Arrange
        var loginDto = new LoginDTO("user@example.com", "SecurePassword123!");
        _mockAuthService.Setup(s => s.LoginAsync(loginDto))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("An error occurred during login", statusCodeResult.Value);
    }

    [Fact]
    public async Task Login_WhenUnexpectedExceptionThrown_ShouldLogError()
    {
        // Arrange
        var loginDto = new LoginDTO("user@example.com", "SecurePassword123!");
        var exception = new Exception("Unexpected error");
        _mockAuthService.Setup(s => s.LoginAsync(loginDto))
            .ThrowsAsync(exception);

        // Act
        await _controller.Login(loginDto);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unexpected error during login")),
                It.Is<Exception>(e => e == exception),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Login_ShouldReturnTokenInResponse()
    {
        // Arrange
        var loginDto = new LoginDTO("user@example.com", "SecurePassword123!");
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = loginDto.Email,
            Name = "John Doe",
            Password = "hashed_password"
        };
        var token = "jwt_token_abc123";

        _mockAuthService.Setup(s => s.LoginAsync(loginDto)).ReturnsAsync(user);
        _mockTokenService.Setup(t => t.CreateToken(user)).Returns(token);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<LoginResponseDTO>(okResult.Value);
        Assert.Equal(token, response.token);
    }

    [Fact]
    public void GetUserInfo_WithAuthenticatedUser_ShouldReturnUser()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            Name = "John Doe",
            Password = "hashed_password"
        };

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        _mockAuthService.Setup(s => s.GetUserInfo(principal)).Returns(user);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = principal }
        };

        // Act
        var result = _controller.GetUserInfo();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Value?.Id);
        Assert.Equal("user@example.com", result.Value?.Email);
    }

    [Fact]
    public void GetUserInfo_WithUnauthenticatedUser_ShouldReturnNull()
    {
        // Arrange
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);

        _mockAuthService.Setup(s => s.GetUserInfo(principal)).Returns((User?)null);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = principal }
        };

        // Act
        var result = _controller.GetUserInfo();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Value);
    }

    [Fact]
    public void GetUserInfo_ShouldCallAuthService()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "user123")
        };

        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        var principal = new ClaimsPrincipal(identity);

        var user = new User { Id = "user123", Email = "test@example.com", Name = "Test", Password = "hashed" };
        _mockAuthService.Setup(s => s.GetUserInfo(It.IsAny<ClaimsPrincipal>())).Returns(user);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = principal }
        };

        // Act
        _controller.GetUserInfo();

        // Assert
        _mockAuthService.Verify(s => s.GetUserInfo(It.IsAny<ClaimsPrincipal>()), Times.Once);
    }

    [Fact]
    public async Task Login_ShouldReturnCorrectLoginResponseMessage()
    {
        // Arrange
        var loginDto = new LoginDTO("user@example.com", "SecurePassword123!");
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = loginDto.Email,
            Name = "John Doe",
            Password = "hashed_password"
        };

        _mockAuthService.Setup(s => s.LoginAsync(loginDto)).ReturnsAsync(user);
        _mockTokenService.Setup(t => t.CreateToken(user)).Returns("token");

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<LoginResponseDTO>(okResult.Value);
        Assert.Equal("Login successful", response.message);
    }

    [Fact]
    public async Task Login_ShouldReturnUserIdAndEmailInResponse()
    {
        // Arrange
        var loginDto = new LoginDTO("user@example.com", "SecurePassword123!");
        var userId = Guid.NewGuid().ToString();
        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            Name = "John Doe",
            Password = "hashed_password"
        };

        _mockAuthService.Setup(s => s.LoginAsync(loginDto)).ReturnsAsync(user);
        _mockTokenService.Setup(t => t.CreateToken(user)).Returns("token");

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<LoginResponseDTO>(okResult.Value);
        Assert.Equal(userId, response.id);
        Assert.Equal("user@example.com", response.email);
    }
}