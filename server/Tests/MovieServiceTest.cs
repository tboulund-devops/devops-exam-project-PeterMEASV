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
        var createMovieDto = new CreateMovieDto("New Movie", 2023, "A great movie", "Actor 1, Actor 2", null);
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await movieService.CreateMovie(createMovieDto, userId, new List<Genre>());

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.Equal("New Movie", result.Title);
        Assert.Equal(2023, result.Year);
        Assert.Equal("A great movie", result.Description);
        Assert.Equal("Actor 1, Actor 2", result.Starring);
        
        var dbMovie = await dbContext.Movies.FirstOrDefaultAsync(m => m.Id == result.Id);
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
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => movieService.CreateMovie(createMovieDTO, invalidUserId, new List<Genre>()));
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
        var result = await movieService.CreateMovie(createMovieDTO, userId, new List<Genre>());

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
        var result = await movieService.CreateMovie(createMovieDTO, userId, new List<Genre>());

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
        var result1 = await movieService.CreateMovie(createMovieDTO1, userId, new List<Genre>());
        var result2 = await movieService.CreateMovie(createMovieDTO2, userId, new List<Genre>());

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
        var result = await movieService.CreateMovie(createMovieDTO, userId,new List<Genre>(), rating);

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
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => movieService.CreateMovie(createMovieDTO, userId,new List<Genre>(), invalidRating));
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
        var result = await movieService.CreateMovie(createMovieDTO, userId, new List<Genre>());

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
        await movieService.UpdateMovieRating(userId, movieId, newRating, null);

        // Assert
        var userMovie = await dbContext.UsersMovies.FirstOrDefaultAsync(um => um.UserId == userId && um.MovieId == movieId);
        Assert.NotNull(userMovie);
        Assert.Equal(newRating, userMovie.Rating);
    }

    [Fact]
    public async Task UpdateMovieRating_WithComment_ShouldSaveComment()
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
        dbContext.UsersMovies.Add(new UsersMovie { UserId = userId, MovieId = movieId, Rating = 5 });
        await dbContext.SaveChangesAsync();

        // Act
        await movieService.UpdateMovieRating(userId, movieId, 8, "Really enjoyed this one");

        // Assert
        var userMovie = await dbContext.UsersMovies.FirstOrDefaultAsync(um => um.UserId == userId && um.MovieId == movieId);
        Assert.NotNull(userMovie);
        Assert.Equal(8, userMovie.Rating);
        Assert.Equal("Really enjoyed this one", userMovie.Comment);
    }

    [Fact]
    public async Task UpdateMovieRating_WithNullComment_ShouldClearComment()
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
        dbContext.UsersMovies.Add(new UsersMovie { UserId = userId, MovieId = movieId, Rating = 5, Comment = "Old comment" });
        await dbContext.SaveChangesAsync();

        // Act
        await movieService.UpdateMovieRating(userId, movieId, 7, null);

        // Assert
        var userMovie = await dbContext.UsersMovies.FirstOrDefaultAsync(um => um.UserId == userId && um.MovieId == movieId);
        Assert.NotNull(userMovie);
        Assert.Null(userMovie.Comment);
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
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => movieService.UpdateMovieRating(userId, movieId, invalidRating, null));
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
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => movieService.UpdateMovieRating(userId, movieId, rating, null));
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
        var tooHighRating = 11;
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var movie = new Movie { Id = movieId, Title = "Test Movie", Year = 2020, Starring = "Actor", Description = "Desc" };

        dbContext.Users.Add(user);
        dbContext.Movies.Add(movie);
        dbContext.UsersMovies.Add(new UsersMovie { UserId = userId, MovieId = movieId });
        await dbContext.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => movieService.UpdateMovieRating(userId, movieId, tooHighRating, null));
        Assert.Equal("Rating must be between 1 and 10", exception.Message);
    }

    [Fact]
    public async Task GetMovieRatingByUser_ShouldReturnCorrectRating()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var userId = "user1";
        var movieId = "movie1";
        var expectedRating = 7;
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var movie = new Movie { Id = movieId, Title = "Test Movie", Year = 2020, Starring = "Actor", Description = "Desc" };

        dbContext.Users.Add(user);
        dbContext.Movies.Add(movie);
        dbContext.UsersMovies.Add(new UsersMovie { UserId = userId, MovieId = movieId, Rating = expectedRating });
        await dbContext.SaveChangesAsync();

        // Act
        var result = await movieService.GetMovieRatingByUser(userId, movieId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedRating, result.rating);
    }

    [Fact]
    public async Task GetMovieRatingByUser_WithComment_ShouldReturnComment()
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
        dbContext.UsersMovies.Add(new UsersMovie { UserId = userId, MovieId = movieId, Rating = 9, Comment = "Masterpiece" });
        await dbContext.SaveChangesAsync();

        // Act
        var result = await movieService.GetMovieRatingByUser(userId, movieId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(9, result.rating);
        Assert.Equal("Masterpiece", result.comment);
    }

    [Fact]
    public async Task GetMovieRatingByUser_WithNullRating_ShouldReturnDtoWithNullRating()
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
        dbContext.UsersMovies.Add(new UsersMovie { UserId = userId, MovieId = movieId, Rating = null });
        await dbContext.SaveChangesAsync();

        // Act
        var result = await movieService.GetMovieRatingByUser(userId, movieId);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.rating);
    }

    [Fact]
    public async Task GetMovieRatingByUser_WithNonExistentUserMovie_ShouldThrowKeyNotFoundException()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var userId = "user1";
        var movieId = "movie1";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => movieService.GetMovieRatingByUser(userId, movieId));
        Assert.Equal("User movie not found", exception.Message);
    }

    [Fact]
    public async Task GetMoviesByUser_WithSeenAndRating_ShouldReturnCorrectData()
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
        dbContext.UsersMovies.Add(new UsersMovie { UserId = userId, MovieId = "1", Seen = true, Rating = 9 });
        dbContext.UsersMovies.Add(new UsersMovie { UserId = userId, MovieId = "2", Seen = false, Rating = null });
        await dbContext.SaveChangesAsync();

        // Act
        var result = await movieService.GetMoviesByUser(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        
        var seenMovie = result.First(m => m.Id == "1");
        Assert.True(seenMovie.Seen);
        Assert.Equal(9, seenMovie.Rating);
        
        var unseenMovie = result.First(m => m.Id == "2");
        Assert.False(unseenMovie.Seen);
        Assert.Null(unseenMovie.Rating);
    }

    [Fact]
    public async Task AddMovieToSeen_ShouldMarkMovieAsSeen()
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
        dbContext.UsersMovies.Add(new UsersMovie { UserId = userId, MovieId = movieId, Seen = false });
        await dbContext.SaveChangesAsync();

        // Act
        var result = await movieService.AddMovieToSeen(movieId, userId, true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(movieId, result.Id);
        
        var userMovie = await dbContext.UsersMovies.FirstOrDefaultAsync(um => um.UserId == userId && um.MovieId == movieId);
        Assert.NotNull(userMovie);
        Assert.True(userMovie.Seen);
    }

    [Fact]
    public async Task AddMovieToSeen_ShouldMarkMovieAsUnseen()
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
        dbContext.UsersMovies.Add(new UsersMovie { UserId = userId, MovieId = movieId, Seen = true });
        await dbContext.SaveChangesAsync();

        // Act
        var result = await movieService.AddMovieToSeen(movieId, userId, false);

        // Assert
        Assert.NotNull(result);
        
        var userMovie = await dbContext.UsersMovies.FirstOrDefaultAsync(um => um.UserId == userId && um.MovieId == movieId);
        Assert.NotNull(userMovie);
        Assert.False(userMovie.Seen);
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
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => movieService.AddMovieToSeen(invalidMovieId, userId, true));
        Assert.Equal("Movie not found", exception.Message);
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
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => movieService.AddMovieToSeen(movieId, invalidUserId, true));
        Assert.Equal("User not found", exception.Message);
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
        // Not adding to UsersMovies table
        await dbContext.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => movieService.AddMovieToSeen(movieId, userId, true));
        Assert.Equal("User movie not found", exception.Message);
    }

    [Fact]
    public async Task CreateMovie_WithValidRating_ShouldCreateWithCorrectRating()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var userId = "user1";
        var rating = 10;
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var createMovieDTO = new CreateMovieDto("New Movie", 2023, "A great movie", "Actor 1", null);
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await movieService.CreateMovie(createMovieDTO, userId, new List<Genre>(), rating);

        // Assert
        Assert.NotNull(result);
        var userMovie = await dbContext.UsersMovies.FirstOrDefaultAsync(um => um.UserId == userId && um.MovieId == result.Id);
        Assert.NotNull(userMovie);
        Assert.Equal(10, userMovie.Rating);
        Assert.False(userMovie.Seen);
    }

    [Fact]
    public async Task CreateMovie_WithRatingBelowMinimum_ShouldThrowArgumentException()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var userId = "user1";
        var invalidRating = 0;
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var createMovieDTO = new CreateMovieDto("New Movie", 2023, "A great movie", "Actor 1", null);
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => movieService.CreateMovie(createMovieDTO, userId, new List<Genre>(), invalidRating));
        Assert.Equal("Rating must be between 1 and 10", exception.Message);
    }

    [Fact]
    public async Task UpdateMovieRating_FromNullToValidRating_ShouldUpdate()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var userId = "user1";
        var movieId = "movie1";
        var newRating = 6;
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var movie = new Movie { Id = movieId, Title = "Test Movie", Year = 2020, Starring = "Actor", Description = "Desc" };
        
        dbContext.Users.Add(user);
        dbContext.Movies.Add(movie);
        dbContext.UsersMovies.Add(new UsersMovie { UserId = userId, MovieId = movieId, Rating = null });
        await dbContext.SaveChangesAsync();

        // Act
        await movieService.UpdateMovieRating(userId, movieId, newRating, null);

        // Assert
        var userMovie = await dbContext.UsersMovies.FirstOrDefaultAsync(um => um.UserId == userId && um.MovieId == movieId);
        Assert.NotNull(userMovie);
        Assert.Equal(6, userMovie.Rating);
    }

    [Fact]
    public async Task GetMoviesByUser_ShouldIncludeRatingInDto()
    {
        // Reset database before test
        await ResetDatabaseAsync();
        // Arrange
        var userId = "user1";
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var movie = new Movie { Id = "1", Title = "Rated Movie", Year = 2020, Starring = "Actor 1", Description = "Desc" };
        
        dbContext.Users.Add(user);
        dbContext.Movies.Add(movie);
        dbContext.UsersMovies.Add(new UsersMovie { UserId = userId, MovieId = "1", Seen = true, Rating = 8 });
        await dbContext.SaveChangesAsync();

        // Act
        var result = await movieService.GetMoviesByUser(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        var movieDto = result.First();
        Assert.Equal(8, movieDto.Rating);
        Assert.True(movieDto.Seen);
        Assert.Equal("Rated Movie", movieDto.Title);
    }

    [Fact]
    public async Task SearchMovies_ShouldReturnMatchingMovies_WhenQueryMatchesTitle()
    {
        // Arrange
        await ResetDatabaseAsync();
        dbContext.Movies.Add(new Movie { Id = "1", Title = "Inception", Year = 2010, Description = "Desc", Starring = "Actor" });
        dbContext.Movies.Add(new Movie { Id = "2", Title = "Interstellar", Year = 2014, Description = "Desc", Starring = "Actor" });
        dbContext.Movies.Add(new Movie { Id = "3", Title = "The Dark Knight", Year = 2008, Description = "Desc", Starring = "Actor" });
        await dbContext.SaveChangesAsync();

        // Act
        var result = await movieService.SearchMovies("in");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, m => m.Title == "Inception");
        Assert.Contains(result, m => m.Title == "Interstellar");
    }

    [Fact]
    public async Task SearchMovies_ShouldBeCaseInsensitive()
    {
        // Arrange
        await ResetDatabaseAsync();
        dbContext.Movies.Add(new Movie { Id = "1", Title = "Inception", Year = 2010, Description = "Desc", Starring = "Actor" });
        await dbContext.SaveChangesAsync();

        // Act
        var result = await movieService.SearchMovies("INCEPTION");

        // Assert
        Assert.Single(result);
        Assert.Equal("Inception", result[0].Title);
    }

    [Fact]
    public async Task SearchMovies_ShouldReturnEmptyList_WhenNoMatchesFound()
    {
        // Arrange
        await ResetDatabaseAsync();
        dbContext.Movies.Add(new Movie { Id = "1", Title = "Inception", Year = 2010, Description = "Desc", Starring = "Actor" });
        await dbContext.SaveChangesAsync();

        // Act
        var result = await movieService.SearchMovies("zzznomatch");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchMovies_ShouldReturnAtMostTenResults()
    {
        // Arrange
        await ResetDatabaseAsync();
        for (int i = 1; i <= 15; i++)
            dbContext.Movies.Add(new Movie { Id = $"{i}", Title = $"Movie {i}", Year = 2000 + i, Description = "Desc", Starring = "Actor" });
        await dbContext.SaveChangesAsync();

        // Act
        var result = await movieService.SearchMovies("movie");

        // Assert
        Assert.Equal(10, result.Count);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // GetMovieGenres / UpdateMovieGenres Service Tests
    // ══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetMovieGenres_ShouldReturnGenresForMovie()
    {
        // Arrange
        await ResetDatabaseAsync();
        var genre1 = new Genre { Id = "g1", Name = "Action" };
        var genre2 = new Genre { Id = "g2", Name = "Drama" };
        var movie = new Movie { Id = "m1", Title = "Test Movie", Year = 2020, Description = "Desc", Starring = "Actor" };

        dbContext.Genres.AddRange(genre1, genre2);
        dbContext.Movies.Add(movie);
        dbContext.MoviesGenres.Add(new MoviesGenre { MovieId = "m1", GenreId = "g1" });
        dbContext.MoviesGenres.Add(new MoviesGenre { MovieId = "m1", GenreId = "g2" });
        await dbContext.SaveChangesAsync();

        // Act
        var result = await movieService.GetMovieGenres("m1");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, g => g.Name == "Action");
        Assert.Contains(result, g => g.Name == "Drama");
    }

    [Fact]
    public async Task GetMovieGenres_WithNoGenres_ShouldReturnEmptyList()
    {
        // Arrange
        await ResetDatabaseAsync();
        var movie = new Movie { Id = "m1", Title = "Test Movie", Year = 2020, Description = "Desc", Starring = "Actor" };
        dbContext.Movies.Add(movie);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await movieService.GetMovieGenres("m1");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateMovieGenres_ShouldReplaceExistingGenres()
    {
        // Arrange
        await ResetDatabaseAsync();
        var genre1 = new Genre { Id = "g1", Name = "Action" };
        var genre2 = new Genre { Id = "g2", Name = "Drama" };
        var genre3 = new Genre { Id = "g3", Name = "Comedy" };
        var movie = new Movie { Id = "m1", Title = "Test Movie", Year = 2020, Description = "Desc", Starring = "Actor" };

        dbContext.Genres.AddRange(genre1, genre2, genre3);
        dbContext.Movies.Add(movie);
        dbContext.MoviesGenres.Add(new MoviesGenre { MovieId = "m1", GenreId = "g1" });
        dbContext.MoviesGenres.Add(new MoviesGenre { MovieId = "m1", GenreId = "g2" });
        await dbContext.SaveChangesAsync();

        // Act — replace genres with just Comedy
        await movieService.UpdateMovieGenres("m1", new List<Genre> { genre3 });

        // Assert
        var remaining = dbContext.MoviesGenres.Where(mg => mg.MovieId == "m1").ToList();
        Assert.Single(remaining);
        Assert.Equal("g3", remaining[0].GenreId);
    }

    [Fact]
    public async Task UpdateMovieGenres_WithEmptyList_ShouldRemoveAllGenres()
    {
        // Arrange
        await ResetDatabaseAsync();
        var genre1 = new Genre { Id = "g1", Name = "Action" };
        var movie = new Movie { Id = "m1", Title = "Test Movie", Year = 2020, Description = "Desc", Starring = "Actor" };

        dbContext.Genres.Add(genre1);
        dbContext.Movies.Add(movie);
        dbContext.MoviesGenres.Add(new MoviesGenre { MovieId = "m1", GenreId = "g1" });
        await dbContext.SaveChangesAsync();

        // Act
        await movieService.UpdateMovieGenres("m1", new List<Genre>());

        // Assert
        var remaining = dbContext.MoviesGenres.Where(mg => mg.MovieId == "m1").ToList();
        Assert.Empty(remaining);
    }
}
