using api.Models;
using api.Security;
using api.Services.Interfaces;
using efscaffold;
using efscaffold.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Classes;

public class UserService(
    MyDbContext context,
    ILogger<UserService> logger,
    IPasswordHasher<User> passwordHasher) : IUserService
{
    public async Task<User> CreateUserAsync(CreateUserDto userDto)
    {
        logger.LogInformation("Creating user {Email}", userDto.Email);

        if (string.IsNullOrWhiteSpace(userDto.Email) || string.IsNullOrWhiteSpace(userDto.Password))
        {
            throw new ArgumentException("Fill out all fields");
        }

        var existingUser = await context.Users
            .FirstOrDefaultAsync(u => u.Email == userDto.Email);

        if (existingUser != null)
        {
            throw new InvalidOperationException("User already exists");
        }

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = userDto.Email,
            Name = userDto.Name ?? userDto.Email,
            Password = passwordHasher.HashPassword(null!, userDto.Password)
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        logger.LogInformation("Created user {UserId} - {Email}", user.Id, user.Email);

        return user;
    }
}