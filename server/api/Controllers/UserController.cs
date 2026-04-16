using api.Models;
using api.Services.Interfaces;
using efscaffold;
using efscaffold.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService userService, ILogger<UserController> logger) : ControllerBase
{
    [HttpPost("create")]
    [AllowAnonymous]
    public async Task<ActionResult<User>> CreateUser([FromBody] CreateUserDto userDto)
    {
        try
        {
            var user = await userService.CreateUserAsync(userDto);
            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user");
            return StatusCode(500, "An error occurred while creating the user");
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<User>>> GetAllFriendsForUser(string userId)
    {
        try
        {
            var friends = await userService.GetAllFriendsForUser(userId);
            return Ok(friends);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting friends");
            return StatusCode(500, "An error occurred while getting friends");
        }
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> AddFriendForUser(string userId, string friendId)
    {
        try
        {
            await userService.AddFriendForUser(userId, friendId);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding friend");
            return StatusCode(500, "An error occurred while adding friend");
        }
    }

    [HttpDelete]
    [AllowAnonymous]
    public async Task<ActionResult> RemoveFriendForUser(string userId, string friendId)
    {
        try
        {
            await userService.RemoveFriendForUser(userId, friendId);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing friend");
            return StatusCode(500, "An error occurred while removing friend");
        }
    }
}