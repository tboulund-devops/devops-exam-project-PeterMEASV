using api.Models;
using efscaffold;

namespace api.Services.Interfaces;

public interface IMovieService
{
    Task<List<Movie>> GetAllMovies();
    Task<List<MovieWithSeenDto>> GetMoviesByUser(string userId);
    Task RemoveMovieFromUser(string userId, string movieId);
    Task AddMovieToUser(string userId, string movieId);
    Task<Movie> EditMovie(Movie movie);
    Task<Movie> CreateMovie(CreateMovieDto movieDto, string userId, int? rating = null);
    Task UpdateMovieRating(string userId, string movieId, int rating);
    Task<int?> GetMovieRatingByUser(string userId, string movieId);
    Task<Movie?> AddMovieToSeen(string movieId, string userId, Boolean seen);


}