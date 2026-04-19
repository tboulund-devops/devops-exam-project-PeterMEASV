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
    public async Task<ActionResult<List<MovieWithSeenDto>>> GetMoviesByUser([FromQuery] string userId)
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
    [Route(nameof(UpdateMovieRating))]
    public async Task<ActionResult> UpdateMovieRating([FromQuery] string userId, [FromQuery] string movieId, [FromQuery] int rating)
    {
        try
        {
            await movieService.UpdateMovieRating(userId, movieId, rating);
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet]
    [Route(nameof(GetMovieRatingByUser))]
    public async Task<ActionResult<int>> GetMovieRatingByUser([FromQuery] string userId, [FromQuery] string movieId)
    {
        try
        {
            var rating = await movieService.GetMovieRatingByUser(userId, movieId);
            return Ok(rating);
        }
        catch (Exception)
        {
            return BadRequest("Could not fetch movie rating");
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
    public async Task<ActionResult<Movie>> CreateMovie([FromForm] CreateMovieDto movie, [FromQuery] string userId, [FromQuery] int? rating = null, [FromQuery] List<Genre>? genres = null)
    {
        try
        {
            return Ok(await movieService.CreateMovie(movie, userId, genres, rating));
        }
        catch (Exception)
        {
            return BadRequest("Could not create movie");
        }
    }
    
    [HttpGet]
    [Route(nameof(SearchMovies))]
    public async Task<ActionResult<List<Movie>>> SearchMovies([FromQuery] string query)
    {
        try
        {
            return Ok(await movieService.SearchMovies(query));
        }
        catch (Exception)
        {
            return BadRequest("Could not search movies");
        }
    }

    [HttpGet]
    [Route(nameof(GetMovieGenres))]
    public async Task<ActionResult<List<Genre>>> GetMovieGenres([FromQuery] string movieId)
    {
        try
        {
            return Ok(await movieService.GetMovieGenres(movieId));
        }
        catch (Exception)
        {
            return BadRequest("Could not fetch genres for movie");
        }
    }

    [HttpPatch]
    [Route(nameof(UpdateMovieGenres))]
    public async Task<ActionResult> UpdateMovieGenres([FromQuery] string movieId, [FromBody] List<Genre> genres)
    {
        try
        {
            await movieService.UpdateMovieGenres(movieId, genres);
            return Ok();
        }
        catch (Exception)
        {
            return BadRequest("Could not update genres for movie");
        }
    }

    [HttpPost]
    [Route(nameof(AddMovieToSeen))]
    public async Task<ActionResult<Movie>> AddMovieToSeen([FromQuery] string movieId, [FromQuery] string userId, [FromQuery] Boolean seen)
    {
        try
        {
            return Ok(await movieService.AddMovieToSeen(movieId, userId, seen));
        }
        catch (Exception)
        {
            return BadRequest("Could not add movie to seen");
        }
    }
    
}