using api.Models;
using api.Services.Interfaces;
using efscaffold;
using Microsoft.EntityFrameworkCore;


namespace Tests;

public class MovieServiceTest(IMovieService movieService, MyDbContext dbContext, ITestOutputHelper outputHelper)
{
    [Fact]
    public async Task GetAllMovies_ShouldReturnAllMovies()
    {
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
        // Arrange - empty database

        // Act
        var result = await movieService.GetAllMovies();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetMoviesByUser_ShouldReturnMoviesForValidUser()
    {
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
        // Arrange
        var invalidUserId = "nonexistent-user";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => movieService.GetMoviesByUser(invalidUserId));
        Assert.Equal("User not found", exception.Message);
    }

    [Fact]
    public async Task GetMoviesByUser_WithNoMovies_ShouldReturnEmptyList()
    {
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
        // Arrange
        var invalidUserId = "nonexistent-user";
        var movieId = "movie1";
        var movie = new Movie { Id = movieId, Title = "Test Movie", Year = 2020, Starring = "Actor", Description = "Desc" };
        
        dbContext.Movies.Add(movie);
        await dbContext.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => movieService.AddMovieToUser(invalidUserId, movieId));
        Assert.Equal("User not found", exception.Message);
    }

    [Fact]
    public async Task AddMovieToUser_WithInvalidMovieId_ShouldThrowException()
    {
        // Arrange
        var userId = "user1";
        var invalidMovieId = "nonexistent-movie";
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        
        dbContext.Users.Add(user);
        dbContext.SaveChanges();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => movieService.AddMovieToUser(userId, invalidMovieId));
        Assert.Equal("Movie not found", exception.Message);
    }

    [Fact]
    public async Task RemoveMovieFromUser_ShouldRemoveMovieSuccessfully()
    {
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
        // Arrange
        var invalidUserId = "nonexistent-user";
        var movieId = "movie1";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => movieService.RemoveMovieFromUser(invalidUserId, movieId));
        Assert.Equal("User not found", exception.Message);
    }

    [Fact]
    public async Task RemoveMovieFromUser_WithNonExistentAssociation_ShouldThrowException()
    {
        // Arrange
        var userId = "user1";
        var movieId = "movie1";
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => movieService.RemoveMovieFromUser(userId, movieId));
        Assert.Equal("User movie not found", exception.Message);
    }

    [Fact]
    public async Task EditMovie_ShouldUpdateMovieSuccessfully()
    {
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
        // Arrange
        var movieWithNullId = new Movie { Id = null, Title = "Title", Year = 2020, Starring = "Actor", Description = "Desc"};

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => movieService.EditMovie(movieWithNullId));
        Assert.Contains("Movie id cannot be null", exception.Message);
    }

    [Fact]
    public async Task EditMovie_WithNonExistentMovie_ShouldThrowException()
    {
        // Arrange
        var nonExistentMovie = new Movie { Id = "nonexistent", Title = "Title", Year = 2020, Starring = "Actor", Description = "Desc"};

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => movieService.EditMovie(nonExistentMovie));
        Assert.Equal("Movie not found", exception.Message);
    }

    [Fact]
    public async Task CreateMovie_ShouldCreateMovieSuccessfully()
    {
        // Arrange
        var userId = "user1";
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var createMovieDTO = new CreateMovieDTO("New Movie", 2023, "A great movie", "Actor 1, Actor 2");
        
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
        // Arrange
        var invalidUserId = "nonexistent-user";
        var createMovieDTO = new CreateMovieDTO("New Movie", 2023, "A great movie", "Actor 1");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => movieService.CreateMovie(createMovieDTO, invalidUserId));
        Assert.Equal("User not found", exception.Message);
    }

    [Fact]
    public async Task CreateMovie_WithNullDescription_ShouldCreateMovieSuccessfully()
    {
        // Arrange
        var userId = "user1";
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var createMovieDTO = new CreateMovieDTO("Movie Title", 2023, null, "Actor");
        
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
        // Arrange
        var userId = "user1";
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var createMovieDTO = new CreateMovieDTO("Movie Title", 2023, "Description", null);
        
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
        // Arrange
        var userId = "user1";
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "pwd" };
        var createMovieDTO1 = new CreateMovieDTO("Movie 1", 2023, "Desc 1", "Actor 1");
        var createMovieDTO2 = new CreateMovieDTO("Movie 2", 2023, "Desc 2", "Actor 2");
        
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
}
