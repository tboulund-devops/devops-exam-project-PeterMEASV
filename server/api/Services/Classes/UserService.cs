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
            .FirstOrDefaultAsync(u => u.Id == userDto.userId || u.Email == userDto.Email);

        if (existingUser != null)
        {
            throw new InvalidOperationException("User already exists");
        }

        var user = new User
        {
            Id = userDto.userId,
            Email = userDto.Email,
            Name = userDto.Name ?? userDto.Email,
            Password = passwordHasher.HashPassword(null!, userDto.Password)
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        logger.LogInformation("Created user {UserId} - {Email}", user.Id, user.Email);

        return user;
    }

    public async Task<List<User>> GetAllFriendsForUser(string userId)
    {
        try
        {
            User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }
        }
        catch (Exception)
        {
            throw new KeyNotFoundException("Failed to get user");
        }
        
        // Get friends where user is the initiator (userId column)
        List<String> friendIdsAsInitiator = await context.UsersUsers
            .Where(uu => uu.UserId == userId)
            .Select(uu => uu.FriendId)
            .ToListAsync();
        
        // Get friends where user is the recipient (friendId column)
        List<String> friendIdsAsRecipient = await context.UsersUsers
            .Where(uu => uu.FriendId == userId)
            .Select(uu => uu.UserId)
            .ToListAsync();
        
        // Combine both lists and remove duplicates
        var allFriendIds = friendIdsAsInitiator.Concat(friendIdsAsRecipient).Distinct().ToList();
        
        return await context.Users.Where(u => allFriendIds.Contains(u.Id)).ToListAsync();
    }

    public async Task AddFriendForUser(string userId, string friendId)
    {
        try
        {
            User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }
        }
        catch (Exception)
        {
            throw new KeyNotFoundException("Failed to get user");
        }
        
        try
        {
            User? friend = await context.Users.FirstOrDefaultAsync(f => f.Id == friendId);
            if (friend == null)
            {
                throw new KeyNotFoundException("Friend user not found");
            }
        }
        catch (Exception)
        {
            throw new KeyNotFoundException("Failed to get Friend user");
        }
        
        // Check if friendship already exists in either direction
        var existingFriendship = await context.UsersUsers.FirstOrDefaultAsync(uu =>
            (uu.UserId == userId && uu.FriendId == friendId) ||
            (uu.UserId == friendId && uu.FriendId == userId));
        
        if (existingFriendship != null)
        {
            throw new InvalidOperationException("Friendship already exists");
        }
        
        // Add friendship bidirectionally
        await context.UsersUsers.AddAsync(new UsersUser { UserId = userId, FriendId = friendId });
        await context.UsersUsers.AddAsync(new UsersUser { UserId = friendId, FriendId = userId });
        await context.SaveChangesAsync();
    }

    public async Task RemoveFriendForUser(string userId, string friendId)
    {
        try
        {
            User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }
        }
        catch (Exception)
        {
            throw new KeyNotFoundException("Failed to get user");
        }
        
        try
        {
            User? friend = await context.Users.FirstOrDefaultAsync(f => f.Id == friendId);
            if (friend == null)
            {
                throw new KeyNotFoundException("Friend user not found");
            }
        }
        catch (Exception)
        {
            throw new KeyNotFoundException("Failed to get Friend user");
        }
        
        // Remove friendship in both directions
        var friendshipsToRemove = await context.UsersUsers
            .Where(um => 
                (um.UserId == userId && um.FriendId == friendId) ||
                (um.UserId == friendId && um.FriendId == userId))
            .ToListAsync();
        
        if (friendshipsToRemove.Count == 0)
        {
            throw new KeyNotFoundException("User friend not found");
        }
        
        context.UsersUsers.RemoveRange(friendshipsToRemove);
        await context.SaveChangesAsync();
    }
}