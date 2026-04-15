using api.Models;
using api.Services.Interfaces;
using efscaffold;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Classes;

public class MovieService(MyDbContext context, IStorageService storageService) : IMovieService
{
    const string ErrorMessage = "User not found";
    public Task<List<Movie>> GetAllMovies()
    {
        return context.Movies.ToListAsync();
    }

    public async Task<List<Movie>> GetMoviesByUser(string userId)
    {
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new KeyNotFoundException(ErrorMessage);
        }
        
        var userMovies = await context.UsersMovies.Where(um => um.UserId == userId).Select(um => um.MovieId).ToListAsync();
        var movies = await context.Movies.Where(m => userMovies.Contains(m.Id)).ToListAsync();
        return movies;
    }

    public async Task RemoveMovieFromUser(string userId, string movieId)
    {
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new KeyNotFoundException(ErrorMessage);
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
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new KeyNotFoundException(ErrorMessage);
        }
        
        Movie? movie = await context.Movies.FirstOrDefaultAsync(m => m.Id == movieId);
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
    
    public async Task UpdateMovieRating(string userId, string movieId, int rating)
    {
        if (rating < 1 || rating > 10)
        {
            throw new ArgumentException("Rating must be between 1 and 10");
        }
        
        UsersMovie? userMovie = await context.UsersMovies
            .FirstOrDefaultAsync(um => um.UserId == userId && um.MovieId == movieId);

        if (userMovie == null)
        {
            throw new KeyNotFoundException("User movie not found");
        }

        userMovie.Rating = rating;
        context.Update(userMovie);
        await context.SaveChangesAsync();
    }

    public Task<int?> GetMovieRatingByUser(string userId, string movieId)
    {
        UsersMovie? userMovie = context.UsersMovies.FirstOrDefault(um => um.UserId == userId && um.MovieId == movieId);
        if (userMovie == null)
        {
            throw new KeyNotFoundException("User movie not found");
        }
        
        return Task.FromResult(userMovie.Rating);
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

    public async Task<Movie> CreateMovie(CreateMovieDto movieDto, string userId, int? rating = null)
    {
        
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new KeyNotFoundException(ErrorMessage);
        }

        if (rating.HasValue && (rating < 1 || rating > 10))
        {
            throw new ArgumentException("Rating must be between 1 and 10");
        }

        string url;
        if (movieDto.Photo != null)
        {
             url = await storageService.UploadPhotoAsync(movieDto.Photo);
        }
        else
        {
            url = "https://storage.googleapis.com/devops-m2c-posters/default-placeholder.png";
        }

        Movie movie = new Movie()
        {
            Id = Guid.NewGuid().ToString(),
            Description = movieDto.Description,
            Title = movieDto.Title,
            Year = movieDto.Year,
            Starring = movieDto.Starring,
            Photo = url
        };
        context.Movies.Add(movie);
        context.UsersMovies.Add(new UsersMovie {UserId = userId, MovieId = movie.Id, Rating = rating});
        await context.SaveChangesAsync();
        return movie;
    }
}