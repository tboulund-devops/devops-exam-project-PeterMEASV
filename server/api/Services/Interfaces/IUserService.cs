using efscaffold.Models;

namespace api.Services.Interfaces;
using api.Models;
using efscaffold;
public interface IUserService
{
    Task<User> CreateUserAsync(CreateUserDto userDto);
}