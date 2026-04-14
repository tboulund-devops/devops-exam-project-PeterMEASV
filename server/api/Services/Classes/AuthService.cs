using System.Security.Authentication;
using System.Security.Claims;
using api.Models;
using api.Services.Interfaces;
using efscaffold;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Classes;

public class AuthService(
    MyDbContext context,
    ILogger<AuthService> logger,
    IPasswordHasher<User> passwordHasher) : IAuthService
{
    public async Task<User?> LoginAsync(LoginDto loginDto)
    {
        logger.LogInformation("Login attempt for email {Email}", loginDto.Email);

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

        if (user == null)
        {
            logger.LogWarning("Login failed: User with email {Email} not found", loginDto.Email);
            throw new InvalidCredentialException("Invalid email or password");
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.Password, loginDto.Password);

        if (result != PasswordVerificationResult.Success)
        {
            logger.LogWarning("Login failed: Invalid password");
            throw new InvalidCredentialException("Invalid email or password");
        }

        logger.LogInformation("Login successful for user {UserId} - {Email}", user.Id, user.Email);
        return user;
    }

    public User? GetUserInfo(ClaimsPrincipal principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return Queryable.SingleOrDefault(
            context.Users.AsNoTracking(),
            user => user.Id == userId
        );
    }
    
}