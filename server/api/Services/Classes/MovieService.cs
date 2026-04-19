using api.Models;
using api.Services.Interfaces;
using efscaffold;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Classes;

public class MovieService(MyDbContext context, IStorageService storageService) : IMovieService
{
    const string ErrorMessage = "User not found";
    const string MovieNotFound = "Movie not found";
    const string UserMovieNotFound = "User movie not found";
    public Task<List<Movie>> GetAllMovies()
    {
        return context.Movies.ToListAsync();
    }

    public async Task<List<MovieWithSeenDto>> GetMoviesByUser(string userId)
    {
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new KeyNotFoundException(ErrorMessage);
        }
        
        var userMoviesWithStatus = await context.UsersMovies
            .Where(um => um.UserId == userId)
            .Join(context.Movies,
                um => um.MovieId,
                m => m.Id,
                (um, m) => new MovieWithSeenDto(
                    m.Id,
                    m.Title,
                    m.Year,
                    m.Description,
                    m.Starring,
                    m.Photo,
                    um.Seen,
                    um.Rating
                ))
            .ToListAsync();
        
        return userMoviesWithStatus;
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
            throw new KeyNotFoundException(UserMovieNotFound);
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
            throw new KeyNotFoundException(MovieNotFound);
        }
        
        if (user == null || movie == null)
        {
            throw new KeyNotFoundException("User or movie not found");
        }
        
        context.UsersMovies.Add(new UsersMovie {UserId = userId, MovieId = movieId});
        await context.SaveChangesAsync();
    }
    
    public async Task UpdateMovieRating(string userId, string movieId, int rating, string? comment)
    {
        if (rating < 1 || rating > 10)
        {
            throw new ArgumentException("Rating must be between 1 and 10");
        }
        
        UsersMovie? userMovie = await context.UsersMovies
            .FirstOrDefaultAsync(um => um.UserId == userId && um.MovieId == movieId);

        if (userMovie == null)
        {
            throw new KeyNotFoundException(UserMovieNotFound);
        }

        userMovie.Rating = rating;
        userMovie.Comment = comment;
        context.Update(userMovie);
        await context.SaveChangesAsync();
    }

    public Task<ratingDto?> GetMovieRatingByUser(string userId, string movieId)
    {
        UsersMovie? userMovie = context.UsersMovies.FirstOrDefault(um => um.UserId == userId && um.MovieId == movieId);
        if (userMovie == null)
        {
            throw new KeyNotFoundException(UserMovieNotFound);
        }

        return Task.FromResult<ratingDto?>(new ratingDto(userMovie.Rating, userMovie.Comment));
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
            throw new KeyNotFoundException(MovieNotFound); 
        }
        
        existingMovie.Title = movie.Title;
        existingMovie.Year = movie.Year;
        existingMovie.Starring = movie.Starring;
        existingMovie.Description = movie.Description;
        
        context.Update(existingMovie);
        context.SaveChanges();
        return Task.FromResult(existingMovie);
    }

    public async Task<Movie> CreateMovie(CreateMovieDto movieDto, string userId, List<Genre> genres, int? rating = null)
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
        
        foreach (Genre genre in genres ?? [])
        {
            context.MoviesGenres.Add(new MoviesGenre { MovieId = movie.Id, GenreId = genre.Id });
        }
        
        context.UsersMovies.Add(new UsersMovie {UserId = userId, MovieId = movie.Id, Rating = rating, Seen = false});
        await context.SaveChangesAsync();
        return movie;
    }

    public async Task<Movie?> AddMovieToSeen(string movieId, string userId, Boolean seen)
    {
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new KeyNotFoundException(ErrorMessage);
        }
        
        Movie? movie = await context.Movies.FirstOrDefaultAsync(m => m.Id == movieId);
        if (movie == null)
        {
            throw new KeyNotFoundException(MovieNotFound);
        }
        
        UsersMovie? userMovie = await context.UsersMovies
            .FirstOrDefaultAsync(um => um.UserId == userId && um.MovieId == movieId);

        if (userMovie == null)
        {
            throw new KeyNotFoundException(UserMovieNotFound);
        }

        userMovie.Seen = seen;
        context.Update(userMovie);
        await context.SaveChangesAsync();
        
        return movie;
    }

    public Task<List<Movie>> SearchMovies(string query)
    {
        var q = query.Trim().ToLower();
        return context.Movies
            .Where(m => m.Title.ToLower().Contains(q))
            .Take(10)
            .ToListAsync();
    }

    public Task<List<Genre>> GetMovieGenres(string movieId)
    {
        var genreIds = context.MoviesGenres
            .Where(mg => mg.MovieId == movieId)
            .Select(mg => mg.GenreId)
            .ToHashSet();

        return context.Genres
            .Where(g => genreIds.Contains(g.Id))
            .ToListAsync();
    }

    public async Task UpdateMovieGenres(string movieId, List<Genre> genres)
    {
        var existing = context.MoviesGenres.Where(mg => mg.MovieId == movieId);
        context.MoviesGenres.RemoveRange(existing);

        foreach (var genre in genres)
        {
            context.MoviesGenres.Add(new MoviesGenre { MovieId = movieId, GenreId = genre.Id });
        }

        await context.SaveChangesAsync();
    }
}