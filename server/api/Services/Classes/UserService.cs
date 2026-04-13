using api.Models;
using api.Security;
using api.Services.Interfaces;
using efscaffold;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Classes;

public class UserService(
    MyDbContext context,
    ILogger<UserService> logger,
    IPasswordHasher<User> passwordHasher) : IUserService
{
    public async Task<User> CreateUserAsync(CreateUserDTO userDto)
    {
        logger.LogInformation("Creating user {Email}", userDto.email);

        if (string.IsNullOrWhiteSpace(userDto.email) || string.IsNullOrWhiteSpace(userDto.password))
        {
            throw new ArgumentException("Fill out all fields");
        }

        var existingUser = await context.Users
            .FirstOrDefaultAsync(u => u.Email == userDto.email);

        if (existingUser != null)
        {
            throw new InvalidOperationException("User already exists");
        }

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = userDto.email,
            Name = userDto.name,
            Password = passwordHasher.HashPassword(null!, userDto.password)
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        logger.LogInformation("Created user {UserId} - {Email}", user.Id, user.Email);

        return user;
    }
}