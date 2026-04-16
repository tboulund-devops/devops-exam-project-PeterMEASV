using efscaffold.Models;

namespace api.Services.Interfaces;
using api.Models;
using efscaffold;
public interface IUserService
{
    Task<User> CreateUserAsync(CreateUserDto userDto);
    Task<List<User>> GetAllFriendsForUser(string userId);
    Task AddFriendForUser(string userId, string friendId);
    Task RemoveFriendForUser(string userId, string friendId);
}