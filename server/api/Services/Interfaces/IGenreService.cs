using efscaffold;

namespace api.Services.Interfaces;

public interface IGenreService
{
    Task<Genre> CreateGenre(string genreName);
    Task<List<Genre>> GetAllGenres();
    Task DeleteGenre(string genreId);
}