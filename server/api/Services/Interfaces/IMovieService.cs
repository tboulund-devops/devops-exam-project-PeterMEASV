using api.Models;
using efscaffold;

namespace api.Services.Interfaces;

public interface IMovieService
{
    Task<List<Movie>> GetAllMovies();
    Task<List<Movie>> GetMoviesByUser(string userId);
    void RemoveMovieFromUser(string userId, string movieId);
    void AddMovieToUser(string userId, string movieId);
    Task<Movie> EditMovie(Movie movie);
    Task<Movie> CreateMovie(CreateMovieDTO movieDTO, string userID);

}