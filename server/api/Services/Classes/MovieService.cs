using api.Models;
using api.Services.Interfaces;
using efscaffold;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Classes;

public class MovieService(MyDbContext context) : IMovieService
{
    const string _errorMessage = "User not found";
    public Task<List<Movie>> GetAllMovies()
    {
        return context.Movies.ToListAsync();
    }

    public async Task<List<Movie>> GetMoviesByUser(string userId)
    {
        User user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new KeyNotFoundException(_errorMessage);
        }
        
        var userMovies = await context.UsersMovies.Where(um => um.UserId == userId).Select(um => um.MovieId).ToListAsync();
        var movies = await context.Movies.Where(m => userMovies.Contains(m.Id)).ToListAsync();
        return movies;
    }

    public async Task RemoveMovieFromUser(string userId, string movieId)
    {
        User user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new KeyNotFoundException(_errorMessage);
        }
        
        
        
            UsersMovie? userMovie = await context.UsersMovies.FirstOrDefaultAsync(um => um.UserId == userId && um.MovieId == movieId);

        if (userMovie != null)
        {
            context.UsersMovies.Remove(userMovie);
            await context.SaveChangesAsync();
        }
        else
        {
            throw new KeyNotFoundException("User movie not found");
        }
    }

    public async Task AddMovieToUser(string userId, string movieId)
    {
        User user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new KeyNotFoundException(_errorMessage);
        }
        
        Movie movie = await context.Movies.FirstOrDefaultAsync(m => m.Id == movieId);
        if (movie == null)
        {
            throw new KeyNotFoundException("Movie not found");
        }
        
        if (user == null || movie == null)
        {
            throw new KeyNotFoundException("User or movie not found");
        }
        
        context.UsersMovies.Add(new UsersMovie {UserId = userId, MovieId = movieId});
        await context.SaveChangesAsync();
    }

    public Task<Movie> EditMovie(Movie movie)
    {
        if (movie == null)
            throw new ArgumentNullException(nameof(movie), "Movie cannot be null");

        if (string.IsNullOrEmpty(movie.Id))
            throw new ArgumentException("Movie id cannot be null", nameof(movie));
        
        Movie? existingMovie = context.Movies.FirstOrDefault(m => m.Id == movie.Id);

        if (existingMovie == null)
        {
            throw new KeyNotFoundException("Movie not found"); 
        }
        
        existingMovie.Title = movie.Title;
        existingMovie.Year = movie.Year;
        existingMovie.Starring = movie.Starring;
        existingMovie.Description = movie.Description;
        
        context.Update(existingMovie);
        context.SaveChanges();
        return Task.FromResult(existingMovie);
    }

    public async Task<Movie> CreateMovie(CreateMovieDto movieDTO, string userID)
    {
        
        User user = await context.Users.FirstOrDefaultAsync(u => u.Id == userID);
        if (user == null)
        {
            throw new KeyNotFoundException(_errorMessage);
        }

        Movie movie = new Movie()
        {
            Id = Guid.NewGuid().ToString(),
            Description = movieDTO.Description,
            Title = movieDTO.Title,
            Year = movieDTO.Year,
            Starring = movieDTO.Starring,
        };
        context.Movies.Add(movie);
        await context.SaveChangesAsync();
        return movie;
    }
}