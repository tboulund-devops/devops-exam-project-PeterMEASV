using api.Models;
using api.Services.Interfaces;
using efscaffold;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("[controller]")]
public class MovieController(IMovieService movieService) : ControllerBase
{

    [HttpGet]
    [Route(nameof(GetAllMovies))]
    public async Task<ActionResult<List<Movie>>> GetAllMovies()
    {
        try
        {
            return Ok(await movieService.GetAllMovies());
        }
        catch (Exception)
        {
            return BadRequest("Could not fetch all movies");
        }
    }

    [HttpGet]
    [Route(nameof(GetMoviesByUser))]
    public async Task<ActionResult<List<Movie>>> GetMoviesByUser([FromBody] string userId)
    {
        try
        {
            return Ok(await movieService.GetMoviesByUser(userId));
        }
        catch (Exception)
        {
            return BadRequest("Could not fetch movies by user");
        }
    }

    [HttpPost]
    [Route(nameof(RemoveMovieFromUser))]
    public async Task<ActionResult<List<Movie>>> RemoveMovieFromUser(string userId, string movieId)
    {
        try
        {
            await movieService.RemoveMovieFromUser(userId, movieId);
            return Ok();
        }
        catch (Exception)
        {
            return BadRequest("Could not remove movie from user");
        }
    }
    
    [HttpPost]
    [Route(nameof(AddMovieToUser))]
    public async Task<ActionResult<Movie>> AddMovieToUser([FromBody] string userId, string movieId)
    {
        try
        {
            await movieService.AddMovieToUser(userId, movieId);
            return Ok();
        }
        catch (Exception)
        {
            return BadRequest("Could not add movie to user");
        }
    }
    
    [HttpPatch]
    [Route(nameof(EditMovie))]
    public async Task<ActionResult<Movie>> EditMovie([FromBody] Movie movie)
    {
        try
        {
            return Ok(await movieService.EditMovie(movie));
        }
        catch (Exception)
        {
            return BadRequest("Could not edit movie");
        }
    }

    [HttpPost]
    [Route(nameof(CreateMovie))]
    public async Task<ActionResult<Movie>> CreateMovie([FromBody] CreateMovieDto movie, string userID)
    {
        try
        {
            return Ok(await movieService.CreateMovie(movie, userID));
        }
        catch (Exception)
        {
            return BadRequest("Could not create movie");
        }
    }
}