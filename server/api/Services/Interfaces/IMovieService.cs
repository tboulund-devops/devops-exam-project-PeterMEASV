using api.Models;
using efscaffold;

namespace api.Services.Interfaces;

public interface IMovieService
{
    Task<List<Movie>> GetAllMovies();
    Task<List<Movie>> GetMoviesByUser(string userId);
    Task RemoveMovieFromUser(string userId, string movieId);
    Task AddMovieToUser(string userId, string movieId);
    Task<Movie> EditMovie(Movie movie);
    Task<Movie> CreateMovie(CreateMovieDto movieDTO, string userID);

}