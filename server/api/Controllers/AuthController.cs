using System.Security.Authentication;
using api.Models;
using api.Security;
using api.Services.Interfaces;
using efscaffold;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController(IAuthService authService, ILogger<AuthController> logger, ITokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        if (string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
        {
            return BadRequest("Email and password are required");
        }

        try
        {
            var user = await authService.LoginAsync(loginDto);

            var response = new LoginResponseDto(
                user!.Id,
                user.Email,
                tokenService.CreateToken(user),
                "Login successful"
            );

            return Ok(response);
        }
        catch (InvalidCredentialException)
        {
            return Unauthorized("Invalid email or password");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during login");
            return StatusCode(500, "An error occurred during login");
        }
    }

    [HttpGet("userInfo")]
    public ActionResult<User?> GetUserInfo()
    {
        return authService.GetUserInfo(User);
    }
}