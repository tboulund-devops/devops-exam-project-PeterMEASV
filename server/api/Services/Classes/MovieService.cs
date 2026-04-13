using api.Models;
using api.Services.Interfaces;
using efscaffold;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Classes;

public class MovieService(MyDbContext context) : IMovieService
{
    public Task<List<Movie>> GetAllMovies()
    {
        return context.Movies.ToListAsync();
    }

    public async Task<List<Movie>> GetMoviesByUser(string userId)
    {
        User user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        var userMovies = await context.UsersMovies.Where(um => um.UserId == userId).Select(um => um.MovieId).ToListAsync();
        var movies = await context.Movies.Where(m => userMovies.Contains(m.Id)).ToListAsync();
        return movies;
    }

    public async void RemoveMovieFromUser(string userId, string movieId)
    {
        User user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        UsersMovie? userMovie = await context.UsersMovies.FirstOrDefaultAsync(um => um.UserId == userId && um.MovieId == movieId);
        if (userMovie != null)
        {
            context.UsersMovies.Remove(userMovie);
            context.SaveChanges();
        }
        else
        {
            throw new Exception("User movie not found");
        }
    }

    public async void AddMovieToUser(string userId, string movieId)
    {
        User user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }
        
        Movie movie = context.Movies.FirstOrDefault(m => m.Id == movieId);
        if (movie == null)
        {
            throw new Exception("Movie not found");
        }
        
        if (user == null || movie == null)
        {
            throw new Exception("User or movie not found");
        }
        
        context.UsersMovies.Add(new UsersMovie {UserId = userId, MovieId = movieId});
        context.SaveChanges();
    }

    public Task<Movie> EditMovie(Movie movie)
    {
        if (movie.Id == null)
        {
            throw new ArgumentNullException(nameof(movie.Title), "Movie id cannot be null");
        }
        
        Movie? existingMovie = context.Movies.FirstOrDefault(m => m.Id == movie.Id);

        if (existingMovie == null)
        {
            throw new Exception("Movie not found"); 
        }
        
        existingMovie.Title = movie.Title;
        existingMovie.Year = movie.Year;
        existingMovie.Starring = movie.Starring;
        existingMovie.Description = movie.Description;
        
        context.Update(movie);
        context.SaveChanges();
        return Task.FromResult(movie);
    }

    public async Task<Movie> CreateMovie(CreateMovieDTO movieDTO, string userId)
    {
        
        User user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new Exception("User not found");
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
        context.SaveChanges();
        return movie;
    }
}