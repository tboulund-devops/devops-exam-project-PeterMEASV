using System.Security.Claims;
using api.Models;
using efscaffold;

namespace api.Services.Interfaces;

public interface IAuthService
{
    Task<User?> LoginAsync(LoginDto loginDto);
    User? GetUserInfo(ClaimsPrincipal principal);
}