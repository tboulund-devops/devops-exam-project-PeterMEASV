using api.Models;
using api.Services.Interfaces;
using efscaffold;
using Microsoft.EntityFrameworkCore;
using Moq;


namespace Tests;

public class MovieServiceTest(IMovieService movieService, MyDbContext dbContext)
{
    // Helper method to reset database
    private async Task ResetDatabaseAsync()
    {
        dbContext.UsersMovies.RemoveRange(dbContext.UsersMovies);
        dbContext.Movies.RemoveRange(dbContext.Movies);
        dbContext.Users.RemoveRange(dbContext.Users);
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAllMovies_ShouldReturnAllMovies()
    {
        // Reset database before test
        await ResetDatabaseAsync();

        // Arrange
        var movie1 = new Movie { Id = "1", Title = "Test Movie 1", Year = 2020, Starring = "Actor 1", Description = "Desc 1" };
        var movie2 = new Movie { Id = "2", Title = "Test Movie 2", Year = 2021, Starring = "Actor 2", Description = "Desc 2" };
        
        dbContext.Movies.Add(movie1);
        dbContext.Movies.Add(movie2);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await movieService.GetAllMovies();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, m => m.Id == "1" && m.Title == "Test Movie 1");
        Assert.Contains(result, m => m.Id == "2" && m.Title == "Test Movie 2");
    }

    [Fact]
    public async Task GetAllMovies_WithNoMovies_ShouldReturnEmptyList()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        

        // Act
        var result = await movieService.GetAllMovies();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    [Fact]
    public async Task GetMoviesByUser_ShouldReturnMoviesForValidUser()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var userId = "user1";
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var movie1 = new Movie { Id = "1", Title = "Movie 1", Year = 2020, Starring = "Actor 1", Description = "Desc 1"};
        var movie2 = new Movie { Id = "2", Title = "Movie 2", Year = 2021, Starring = "Actor 2", Description = "Desc 2"};
        
        dbContext.Users.Add(user);
        dbContext.Movies.Add(movie1);
        dbContext.Movies.Add(movie2);
        dbContext.UsersMovies.Add(new UsersMovie { UserId = userId, MovieId = "1" });
        dbContext.UsersMovies.Add(new UsersMovie { UserId = userId, MovieId = "2" });
        await dbContext.SaveChangesAsync();

        // Act
        var result = await movieService.GetMoviesByUser(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, m => m.Id == "1");
        Assert.Contains(result, m => m.Id == "2");
    }

    [Fact]
    public async Task GetMoviesByUser_WithInvalidUserId_ShouldThrowException()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var invalidUserId = "nonexistent-user";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => movieService.GetMoviesByUser(invalidUserId));
        Assert.Equal("User not found", exception.Message);
    }

    [Fact]
    public async Task GetMoviesByUser_WithNoMovies_ShouldReturnEmptyList()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var userId = "user1";
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await movieService.GetMoviesByUser(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task AddMovieToUser_ShouldAddMovieSuccessfully()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var userId = "user1";
        var movieId = "movie1";
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var movie = new Movie { Id = movieId, Title = "Test Movie", Year = 2020, Starring = "Actor", Description = "Desc" };
        
        dbContext.Users.Add(user);
        dbContext.Movies.Add(movie);
        await dbContext.SaveChangesAsync();

        // Act
        await movieService.AddMovieToUser(userId, movieId);

        // Assert
        var userMovie = await dbContext.UsersMovies.FirstOrDefaultAsync(um => um.UserId == userId && um.MovieId == movieId);
        Assert.NotNull(userMovie);
        Assert.Equal(userId, userMovie.UserId);
        Assert.Equal(movieId, userMovie.MovieId);
    }

    [Fact]
    public async Task AddMovieToUser_WithInvalidUserId_ShouldThrowException()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var invalidUserId = "nonexistent-user";
        var movieId = "movie1";
        var movie = new Movie { Id = movieId, Title = "Test Movie", Year = 2020, Starring = "Actor", Description = "Desc" };
        
        dbContext.Movies.Add(movie);
        await dbContext.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => movieService.AddMovieToUser(invalidUserId, movieId));
        Assert.Equal("User not found", exception.Message);
    }

    [Fact]
    public async Task AddMovieToUser_WithInvalidMovieId_ShouldThrowException()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var userId = "user1";
        var invalidMovieId = "nonexistent-movie";
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        
        dbContext.Users.Add(user);
        dbContext.SaveChanges();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => movieService.AddMovieToUser(userId, invalidMovieId));
        Assert.Equal("Movie not found", exception.Message);
    }

    [Fact]
    public async Task RemoveMovieFromUser_ShouldRemoveMovieSuccessfully()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var userId = "user1";
        var movieId = "movie1";
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var movie = new Movie { Id = movieId, Title = "Test Movie", Year = 2020, Starring = "Actor", Description = "Desc"};
        
        dbContext.Users.Add(user);
        dbContext.Movies.Add(movie);
        dbContext.UsersMovies.Add(new UsersMovie { UserId = userId, MovieId = movieId });
        await dbContext.SaveChangesAsync();

        // Act
        await movieService.RemoveMovieFromUser(userId, movieId);

        // Assert
        var userMovie = await dbContext.UsersMovies.FirstOrDefaultAsync(um => um.UserId == userId && um.MovieId == movieId);
        Assert.Null(userMovie);
    }

    [Fact]
    public async Task RemoveMovieFromUser_WithInvalidUserId_ShouldThrowException()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var invalidUserId = "nonexistent-user";
        var movieId = "movie1";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => movieService.RemoveMovieFromUser(invalidUserId, movieId));
        Assert.NotNull(exception);
        Assert.Equal("User not found", exception.Message);
    }

    [Fact]
    public async Task RemoveMovieFromUser_WithNonExistentAssociation_ShouldThrowException()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var userId = "user1";
        var movieId = "movie1";
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => movieService.RemoveMovieFromUser(userId, movieId));
        Assert.Equal("User movie not found", exception.Message);
    }

    [Fact]
    public async Task EditMovie_ShouldUpdateMovieSuccessfully()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var movieId = "movie1";
        var originalMovie = new Movie { Id = movieId, Title = "Original Title", Year = 2020, Starring = "Actor 1", Description = "Original Desc" };
        
        dbContext.Movies.Add(originalMovie);
        await dbContext.SaveChangesAsync();

        var updatedMovie = new Movie { Id = movieId, Title = "Updated Title", Year = 2021, Starring = "Actor 2", Description = "Updated Desc"};

        // Act
        var result = await movieService.EditMovie(updatedMovie);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(movieId, result.Id);
        Assert.Equal("Updated Title", result.Title);
        Assert.Equal(2021, result.Year);
        Assert.Equal("Actor 2", result.Starring);
        Assert.Equal("Updated Desc", result.Description);
        
        var dbMovie = await dbContext.Movies.FirstOrDefaultAsync(m => m.Id == movieId);
        Assert.NotNull(dbMovie);
        Assert.Equal("Updated Title", dbMovie.Title);
    }

    [Fact]
    public async Task EditMovie_WithNullId_ShouldThrowArgumentNullException()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var movieWithNullId = new Movie { Id = null, Title = "Title", Year = 2020, Starring = "Actor", Description = "Desc"};

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => movieService.EditMovie(movieWithNullId));
        Assert.Contains("Movie id cannot be null", exception.Message);
    }

    [Fact]
    public async Task EditMovie_WithNonExistentMovie_ShouldThrowException()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var nonExistentMovie = new Movie { Id = "nonexistent", Title = "Title", Year = 2020, Starring = "Actor", Description = "Desc"};

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => movieService.EditMovie(nonExistentMovie));
        Assert.Equal("Movie not found", exception.Message);
    }

    [Fact]
    public async Task CreateMovie_ShouldCreateMovieSuccessfully()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var userId = "user1";
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var createMovieDTO = new CreateMovieDto("New Movie", 2023, "A great movie", "Actor 1, Actor 2", null);
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await movieService.CreateMovie(createMovieDTO, userId);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.Equal("New Movie", result.Title);
        Assert.Equal(2023, result.Year);
        Assert.Equal("A great movie", result.Description);
        Assert.Equal("Actor 1, Actor 2", result.Starring);
        
        var dbMovie = dbContext.Movies.FirstOrDefault(m => m.Id == result.Id);
        Assert.NotNull(dbMovie);
        Assert.Equal("New Movie", dbMovie.Title);
    }

    [Fact]
    public async Task CreateMovie_WithInvalidUserId_ShouldThrowException()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var invalidUserId = "nonexistent-user";
        var createMovieDTO = new CreateMovieDto("New Movie", 2023, "A great movie", "Actor 1", null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => movieService.CreateMovie(createMovieDTO, invalidUserId));
        Assert.Equal("User not found", exception.Message);
    }

    [Fact]
    public async Task CreateMovie_WithNullDescription_ShouldCreateMovieSuccessfully()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var userId = "user1";
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var createMovieDTO = new CreateMovieDto("Movie Title", 2023, null, "Actor", null);
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await movieService.CreateMovie(createMovieDTO, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Movie Title", result.Title);
        Assert.Null(result.Description);
    }

    [Fact]
    public async Task CreateMovie_WithNullStarring_ShouldCreateMovieSuccessfully()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var userId = "user1";
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var createMovieDTO = new CreateMovieDto("Movie Title", 2023, "Description", null, null);
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await movieService.CreateMovie(createMovieDTO, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Movie Title", result.Title);
        Assert.Null(result.Starring);
    }

    [Fact]
    public async Task CreateMovie_ShouldGenerateUniqueIds()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var userId = "user1";
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var createMovieDTO1 = new CreateMovieDto("Movie 1", 2023, "Desc 1", "Actor 1", null);
        var createMovieDTO2 = new CreateMovieDto("Movie 2", 2023, "Desc 2", "Actor 2", null);
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Act
        var result1 = await movieService.CreateMovie(createMovieDTO1, userId);
        var result2 = await movieService.CreateMovie(createMovieDTO2, userId);

        // Assert
        Assert.NotNull(result1.Id);
        Assert.NotNull(result2.Id);
        Assert.NotEqual(result1.Id, result2.Id);
    }

    [Fact]
    public async Task CreateMovie_WithRating_ShouldSetRatingInUsersMovies()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var userId = "user1";
        var rating = 8;
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var createMovieDTO = new CreateMovieDto("New Movie", 2023, "A great movie", "Actor 1", null);
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await movieService.CreateMovie(createMovieDTO, userId, rating);

        // Assert
        Assert.NotNull(result);
        var userMovie = await dbContext.UsersMovies.FirstOrDefaultAsync(um => um.UserId == userId && um.MovieId == result.Id);
        Assert.NotNull(userMovie);
        Assert.Equal(rating, userMovie.Rating);
    }

    [Fact]
    public async Task CreateMovie_WithInvalidRating_ShouldThrowArgumentException()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var userId = "user1";
        var invalidRating = 11;
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var createMovieDTO = new CreateMovieDto("New Movie", 2023, "A great movie", "Actor 1", null);
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => movieService.CreateMovie(createMovieDTO, userId, invalidRating));
        Assert.Equal("Rating must be between 1 and 10", exception.Message);
    }

    [Fact]
    public async Task CreateMovie_WithNullRating_ShouldCreateMovieWithoutRating()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var userId = "user1";
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var createMovieDTO = new CreateMovieDto("New Movie", 2023, "A great movie", "Actor 1", null);
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await movieService.CreateMovie(createMovieDTO, userId, null);

        // Assert
        Assert.NotNull(result);
        var userMovie = await dbContext.UsersMovies.FirstOrDefaultAsync(um => um.UserId == userId && um.MovieId == result.Id);
        Assert.NotNull(userMovie);
        Assert.Null(userMovie.Rating);
    }

    [Fact]
    public async Task UpdateMovieRating_ShouldUpdateRatingSuccessfully()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var userId = "user1";
        var movieId = "movie1";
        var newRating = 9;
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var movie = new Movie { Id = movieId, Title = "Test Movie", Year = 2020, Starring = "Actor", Description = "Desc" };
        
        dbContext.Users.Add(user);
        dbContext.Movies.Add(movie);
        dbContext.UsersMovies.Add(new UsersMovie { UserId = userId, MovieId = movieId, Rating = 5 });
        await dbContext.SaveChangesAsync();

        // Act
        await movieService.UpdateMovieRating(userId, movieId, newRating);

        // Assert
        var userMovie = await dbContext.UsersMovies.FirstOrDefaultAsync(um => um.UserId == userId && um.MovieId == movieId);
        Assert.NotNull(userMovie);
        Assert.Equal(newRating, userMovie.Rating);
    }

    [Fact]
    public async Task UpdateMovieRating_WithInvalidRating_ShouldThrowArgumentException()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var userId = "user1";
        var movieId = "movie1";
        var invalidRating = 0;
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var movie = new Movie { Id = movieId, Title = "Test Movie", Year = 2020, Starring = "Actor", Description = "Desc" };
        
        dbContext.Users.Add(user);
        dbContext.Movies.Add(movie);
        dbContext.UsersMovies.Add(new UsersMovie { UserId = userId, MovieId = movieId });
        await dbContext.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => movieService.UpdateMovieRating(userId, movieId, invalidRating));
        Assert.Equal("Rating must be between 1 and 10", exception.Message);
    }

    [Fact]
    public async Task UpdateMovieRating_WithNonExistentUserMovie_ShouldThrowKeyNotFoundException()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var userId = "user1";
        var movieId = "movie1";
        var rating = 8;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => movieService.UpdateMovieRating(userId, movieId, rating));
        Assert.Equal("User movie not found", exception.Message);
    }

    [Fact]
    public async Task UpdateMovieRating_WithRatingTooHigh_ShouldThrowArgumentException()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var userId = "user1";
        var movieId = "movie1";
        var invalidRating = 15;
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var movie = new Movie { Id = movieId, Title = "Test Movie", Year = 2020, Starring = "Actor", Description = "Desc" };
        
        dbContext.Users.Add(user);
        dbContext.Movies.Add(movie);
        dbContext.UsersMovies.Add(new UsersMovie { UserId = userId, MovieId = movieId });
        await dbContext.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => movieService.UpdateMovieRating(userId, movieId, invalidRating));
        Assert.Equal("Rating must be between 1 and 10", exception.Message);
    }

    [Fact]
    public async Task GetMovieRatingByUser_ShouldGetMovieRatingSuccesfully()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        
        // Arrange
        var userId = "user1";
        var movieId = "movie1";
        var Rating = 7;
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var movie = new Movie { Id = movieId, Title = "Test Movie", Year = 2020, Starring = "Actor", Description = "Desc" };
        
        dbContext.Users.Add(user);
        dbContext.Movies.Add(movie);
        dbContext.UsersMovies.Add(new UsersMovie { UserId = userId, MovieId = movieId, Rating = 7});
        await dbContext.SaveChangesAsync();

        // Act & Assert
        var result = await movieService.GetMovieRatingByUser(userId, movieId);
        Assert.Equal(Rating, result);
    }
    
    [Fact]
    public async Task GetMovieRatingByUser_InvalidMovieShouldFail()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var userId = "user1";
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var movie = new Movie { Id = "1", Title = "Movie 1", Year = 2020, Starring = "Actor 1", Description = "Desc 1" };
        
        dbContext.Users.Add(user);
        dbContext.Movies.Add(movie);
        await dbContext.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => movieService.GetMovieRatingByUser(userId, "2"));
    }

    // ══════════════════════════════════════════════════════════════════════════
    // AddMovieToSeen Tests
    // ══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task AddMovieToSeen_ShouldSetSeenToTrue_WhenValidInput()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        
        // Arrange
        var userId = "user1";
        var movieId = "movie1";
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var movie = new Movie { Id = movieId, Title = "Test Movie", Year = 2020, Starring = "Actor", Description = "Desc" };
        var userMovie = new UsersMovie { UserId = userId, MovieId = movieId, Seen = false };
        
        dbContext.Users.Add(user);
        dbContext.Movies.Add(movie);
        dbContext.UsersMovies.Add(userMovie);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await movieService.AddMovieToSeen(movieId, userId, true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(movieId, result.Id);
        
        var updatedUserMovie = await dbContext.UsersMovies
            .FirstOrDefaultAsync(um => um.UserId == userId && um.MovieId == movieId);
        Assert.NotNull(updatedUserMovie);
        Assert.True(updatedUserMovie.Seen);
    }

    [Fact]
    public async Task AddMovieToSeen_ShouldSetSeenToFalse_WhenMarkingAsUnseen()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        
        // Arrange
        var userId = "user1";
        var movieId = "movie1";
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var movie = new Movie { Id = movieId, Title = "Test Movie", Year = 2020, Starring = "Actor", Description = "Desc" };
        var userMovie = new UsersMovie { UserId = userId, MovieId = movieId, Seen = true };
        
        dbContext.Users.Add(user);
        dbContext.Movies.Add(movie);
        dbContext.UsersMovies.Add(userMovie);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await movieService.AddMovieToSeen(movieId, userId, false);

        // Assert
        Assert.NotNull(result);
        var updatedUserMovie = await dbContext.UsersMovies
            .FirstOrDefaultAsync(um => um.UserId == userId && um.MovieId == movieId);
        Assert.NotNull(updatedUserMovie);
        Assert.False(updatedUserMovie.Seen);
    }

    [Fact]
    public async Task AddMovieToSeen_WithInvalidUserId_ShouldThrowKeyNotFoundException()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        
        // Arrange
        var invalidUserId = "nonexistent-user";
        var movieId = "movie1";
        var movie = new Movie { Id = movieId, Title = "Test Movie", Year = 2020, Starring = "Actor", Description = "Desc" };
        
        dbContext.Movies.Add(movie);
        await dbContext.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            movieService.AddMovieToSeen(movieId, invalidUserId, true));
        Assert.Equal("User not found", exception.Message);
    }

    [Fact]
    public async Task AddMovieToSeen_WithInvalidMovieId_ShouldThrowKeyNotFoundException()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        
        // Arrange
        var userId = "user1";
        var invalidMovieId = "nonexistent-movie";
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            movieService.AddMovieToSeen(invalidMovieId, userId, true));
        Assert.Equal("Movie not found", exception.Message);
    }

    [Fact]
    public async Task AddMovieToSeen_WithNonExistentUserMovie_ShouldThrowKeyNotFoundException()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        
        // Arrange
        var userId = "user1";
        var movieId = "movie1";
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var movie = new Movie { Id = movieId, Title = "Test Movie", Year = 2020, Starring = "Actor", Description = "Desc" };
        
        dbContext.Users.Add(user);
        dbContext.Movies.Add(movie);
        await dbContext.SaveChangesAsync();
        // Note: Not adding UsersMovie relationship

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            movieService.AddMovieToSeen(movieId, userId, true));
        Assert.Equal("User movie not found", exception.Message);
    }

    [Fact]
    public async Task AddMovieToSeen_ShouldToggleSeenStatus_WhenCalledMultipleTimes()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        
        // Arrange
        var userId = "user1";
        var movieId = "movie1";
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var movie = new Movie { Id = movieId, Title = "Test Movie", Year = 2020, Starring = "Actor", Description = "Desc" };
        var userMovie = new UsersMovie { UserId = userId, MovieId = movieId, Seen = false };
        
        dbContext.Users.Add(user);
        dbContext.Movies.Add(movie);
        dbContext.UsersMovies.Add(userMovie);
        await dbContext.SaveChangesAsync();

        // Act - Set to true
        await movieService.AddMovieToSeen(movieId, userId, true);
        dbContext.ChangeTracker.Clear(); // Clear the EF cache
        var firstCheck = await dbContext.UsersMovies
            .AsNoTracking() // Don't track this query
            .FirstOrDefaultAsync(um => um.UserId == userId && um.MovieId == movieId);
        
        // Act - Set to false
        await movieService.AddMovieToSeen(movieId, userId, false);
        dbContext.ChangeTracker.Clear(); // Clear the EF cache
        var secondCheck = await dbContext.UsersMovies
            .AsNoTracking() // Don't track this query
            .FirstOrDefaultAsync(um => um.UserId == userId && um.MovieId == movieId);
        
        // Act - Set to true again
        await movieService.AddMovieToSeen(movieId, userId, true);
        dbContext.ChangeTracker.Clear(); // Clear the EF cache
        var thirdCheck = await dbContext.UsersMovies
            .AsNoTracking() // Don't track this query
            .FirstOrDefaultAsync(um => um.UserId == userId && um.MovieId == movieId);

        // Assert
        Assert.True(firstCheck?.Seen);
        Assert.False(secondCheck?.Seen);
        Assert.True(thirdCheck?.Seen);
    }


    // ══════════════════════════════════════════════════════════════════════════
    // GetMoviesByUser with MovieWithSeenDto Tests
    // ══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetMoviesByUser_ShouldReturnMoviesWithSeenStatus()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        
        // Arrange
        var userId = "user1";
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var movie1 = new Movie { Id = "1", Title = "Seen Movie", Year = 2020, Starring = "Actor 1", Description = "Desc 1" };
        var movie2 = new Movie { Id = "2", Title = "Unseen Movie", Year = 2021, Starring = "Actor 2", Description = "Desc 2" };
        
        dbContext.Users.Add(user);
        dbContext.Movies.Add(movie1);
        dbContext.Movies.Add(movie2);
        dbContext.UsersMovies.Add(new UsersMovie { UserId = userId, MovieId = "1", Seen = true, Rating = 8 });
        dbContext.UsersMovies.Add(new UsersMovie { UserId = userId, MovieId = "2", Seen = false, Rating = null });
        await dbContext.SaveChangesAsync();

        // Act
        var result = await movieService.GetMoviesByUser(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        
        var seenMovie = result.FirstOrDefault(m => m.Id == "1");
        Assert.NotNull(seenMovie);
        Assert.True(seenMovie.Seen);
        Assert.Equal(8, seenMovie.Rating);
        Assert.Equal("Seen Movie", seenMovie.Title);
        
        var unseenMovie = result.FirstOrDefault(m => m.Id == "2");
        Assert.NotNull(unseenMovie);
        Assert.False(unseenMovie.Seen);
        Assert.Null(unseenMovie.Rating);
        Assert.Equal("Unseen Movie", unseenMovie.Title);
    }

    [Fact]
    public async Task GetMoviesByUser_ShouldReturnOnlySeenMovies_WhenFiltered()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        
        // Arrange
        var userId = "user1";
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var movie1 = new Movie { Id = "1", Title = "Movie 1", Year = 2020, Starring = "Actor 1", Description = "Desc 1" };
        var movie2 = new Movie { Id = "2", Title = "Movie 2", Year = 2021, Starring = "Actor 2", Description = "Desc 2" };
        var movie3 = new Movie { Id = "3", Title = "Movie 3", Year = 2022, Starring = "Actor 3", Description = "Desc 3" };
        
        dbContext.Users.Add(user);
        dbContext.Movies.AddRange(movie1, movie2, movie3);
        dbContext.UsersMovies.Add(new UsersMovie { UserId = userId, MovieId = "1", Seen = true });
        dbContext.UsersMovies.Add(new UsersMovie { UserId = userId, MovieId = "2", Seen = false });
        dbContext.UsersMovies.Add(new UsersMovie { UserId = userId, MovieId = "3", Seen = true });
        await dbContext.SaveChangesAsync();

        // Act
        var allMovies = await movieService.GetMoviesByUser(userId);
        var seenMovies = allMovies.Where(m => m.Seen == true).ToList();

        // Assert
        Assert.Equal(3, allMovies.Count);
        Assert.Equal(2, seenMovies.Count);
        Assert.Contains(seenMovies, m => m.Id == "1");
        Assert.Contains(seenMovies, m => m.Id == "3");
        Assert.DoesNotContain(seenMovies, m => m.Id == "2");
    }

    [Fact]
    public async Task GetMoviesByUser_ShouldReturnMoviesWithNullSeen_AsDefaultFalse()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        
        // Arrange
        var userId = "user1";
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var movie = new Movie { Id = "1", Title = "Movie 1", Year = 2020, Starring = "Actor 1", Description = "Desc 1" };
        
        dbContext.Users.Add(user);
        dbContext.Movies.Add(movie);
        dbContext.UsersMovies.Add(new UsersMovie { UserId = userId, MovieId = "1", Seen = null });
        await dbContext.SaveChangesAsync();

        // Act
        var result = await movieService.GetMoviesByUser(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        var movieDto = result.First();
        Assert.Null(movieDto.Seen); // Should preserve null value
    }

    [Fact]
    public async Task CreateMovie_ShouldSetSeenToFalse_ByDefault()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        
        // Arrange
        var userId = "user1";
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var createMovieDto = new CreateMovieDto("New Movie", 2023, "A new movie", "Actor", null);

        // Act
        var createdMovie = await movieService.CreateMovie(createMovieDto, userId, 7);

        // Assert
        var userMovie = await dbContext.UsersMovies
            .FirstOrDefaultAsync(um => um.UserId == userId && um.MovieId == createdMovie.Id);
        Assert.NotNull(userMovie);
        Assert.False(userMovie.Seen);
        Assert.Equal(7, userMovie.Rating);
    }
}
