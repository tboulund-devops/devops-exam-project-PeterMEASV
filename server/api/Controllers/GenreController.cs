using api.Services.Interfaces;
using efscaffold;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("[controller]")]
public class GenreController(IGenreService genreService) : ControllerBase
{
    [HttpPost]
    [Route(nameof(CreateGenre))]
    public async Task<ActionResult<Genre>> CreateGenre(string genreName)
    {
        try
        {
            return Ok(await genreService.CreateGenre(genreName));
        }
        catch (Exception)
        {
            return BadRequest("Could not create genre");
        }
    }

    [HttpGet]
    [Route(nameof(GetAllGenres))]
    public async Task<ActionResult<List<Genre>>> GetAllGenres()
    {
        try
        {
            return Ok(await genreService.GetAllGenres());
            
        }
        catch (Exception)
        {
            return BadRequest("Could not fetch genres");
        }
    }

    [HttpDelete]
    [Route(nameof(DeleteGenre))]
    public async Task<ActionResult> DeleteGenre(string genreId)
    {
        try
        {
            await genreService.DeleteGenre(genreId);
            return Ok();
        }
        catch (Exception)
        {
            return BadRequest("Could not delete genre");
        }
        
    }
    
}