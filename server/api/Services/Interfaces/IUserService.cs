namespace api.Services.Interfaces;
using api.Models;
using efscaffold;
public interface IUserService
{
    Task<User> CreateUserAsync(CreateUserDTO userDto);
}