using api.Controllers;
using api.Models;
using api.Services.Interfaces;
using efscaffold;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests;

public class MovieControllerTest
{
    private readonly Mock<IMovieService> _mockService;
    private readonly MovieController _controller;

    public MovieControllerTest()
    {
        _mockService = new Mock<IMovieService>();
        _controller = new MovieController(_mockService.Object);
    }

    [Fact]
    public async Task GetAllMovies_ShouldReturnOk_WhenServiceSucceeds()
    {
        // Arrange
        var movies = new List<Movie>
        {
            new Movie { Id = "1", Title = "Movie 1", Year = 2020, Starring = "Actor 1", Description = "Desc 1" },
            new Movie { Id = "2", Title = "Movie 2", Year = 2021, Starring = "Actor 2", Description = "Desc 2" }
        };
        _mockService.Setup(s => s.GetAllMovies()).ReturnsAsync(movies);

        // Act
        var result = await _controller.GetAllMovies();

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetAllMovies_ShouldReturnBadRequest_WhenServiceThrows()
    {
        // Arrange
        _mockService.Setup(s => s.GetAllMovies()).ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.GetAllMovies();

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Could not fetch all movies", badRequest.Value);
    }

    [Fact]
    public async Task GetMoviesByUser_ShouldReturnOk_WhenServiceSucceeds()
    {
        // Arrange
        var userId = "user1";
        var moviesWithSeen = new List<MovieWithSeenDto>
        {
            new MovieWithSeenDto("1", "Movie 1", 2020, "Desc 1", "Actor 1", null, false, null)
        };
        _mockService.Setup(s => s.GetMoviesByUser(userId)).ReturnsAsync(moviesWithSeen);

        // Act
        var result = await _controller.GetMoviesByUser(userId);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
    }


    [Fact]
    public async Task GetMoviesByUser_ShouldReturnBadRequest_WhenServiceThrows()
    {
        // Arrange
        var userId = "nonexistent-user";
        _mockService.Setup(s => s.GetMoviesByUser(userId)).ThrowsAsync(new Exception("User not found"));

        // Act
        var result = await _controller.GetMoviesByUser(userId);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Could not fetch movies by user", badRequest.Value);
    }

    [Fact]
    public async Task RemoveMovieFromUser_ShouldReturnOk_WhenServiceSucceeds()
    {
        // Arrange
        var userId = "user1";
        var movieId = "movie1";
        _mockService.Setup(s => s.RemoveMovieFromUser(userId, movieId)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RemoveMovieFromUser(userId, movieId);

        // Assert
        Assert.IsType<OkResult>(result.Result);
    }

    [Fact]
    public async Task RemoveMovieFromUser_ShouldReturnBadRequest_WhenServiceThrows()
    {
        // Arrange
        var userId = "nonexistent-user";
        var movieId = "movie1";
        _mockService.Setup(s => s.RemoveMovieFromUser(userId, movieId)).ThrowsAsync(new Exception("User not found"));

        // Act
        var result = await _controller.RemoveMovieFromUser(userId, movieId);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Could not remove movie from user", badRequest.Value);
    }

    [Fact]
    public async Task AddMovieToUser_ShouldReturnOk_WhenServiceSucceeds()
    {
        // Arrange
        var userId = "user1";
        var movieId = "movie1";
        _mockService.Setup(s => s.AddMovieToUser(userId, movieId)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AddMovieToUser(userId, movieId);

        // Assert
        Assert.IsType<OkResult>(result.Result);
    }

    [Fact]
    public async Task AddMovieToUser_ShouldReturnBadRequest_WhenServiceThrows()
    {
        // Arrange
        var userId = "nonexistent-user";
        var movieId = "movie1";
        _mockService.Setup(s => s.AddMovieToUser(userId, movieId)).ThrowsAsync(new Exception("User not found"));

        // Act
        var result = await _controller.AddMovieToUser(userId, movieId);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Could not add movie to user", badRequest.Value);
    }

    [Fact]
    public async Task EditMovie_ShouldReturnOk_WithUpdatedMovie_WhenServiceSucceeds()
    {
        // Arrange
        var movie = new Movie { Id = "movie1", Title = "Updated Title", Year = 2021, Starring = "Actor 2", Description = "Updated Desc" };
        _mockService.Setup(s => s.EditMovie(movie)).ReturnsAsync(movie);

        // Act
        var result = await _controller.EditMovie(movie);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(movie, okResult.Value);
    }

    [Fact]
    public async Task EditMovie_ShouldReturnBadRequest_WhenServiceThrows()
    {
        // Arrange
        var movie = new Movie { Id = "nonexistent", Title = "Title", Year = 2020, Starring = "Actor", Description = "Desc" };
        _mockService.Setup(s => s.EditMovie(movie)).ThrowsAsync(new Exception("Movie not found"));

        // Act
        var result = await _controller.EditMovie(movie);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Could not edit movie", badRequest.Value);
    }

   [Fact]
    public async Task CreateMovie_ShouldReturnOk_WithCreatedMovie_WhenServiceSucceeds()
    {
        // Arrange
        var userId = "user1";
        var rating = 0;
        var createMovieDTO = new CreateMovieDto("New Movie", 2023, "A great movie", "Actor 1", null);
        var createdMovie = new Movie { Id = Guid.NewGuid().ToString(), Title = "New Movie", Year = 2023, Starring = "Actor 1", Description = "A great movie" };
        _mockService.Setup(s => s.CreateMovie(createMovieDTO, userId, It.IsAny<int?>())).ReturnsAsync(createdMovie);

        // Act
        var result = await _controller.CreateMovie(createMovieDTO, userId, rating);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(createdMovie, okResult.Value);
    }

    [Fact]
    public async Task CreateMovie_ShouldReturnBadRequest_WhenServiceThrows()
    {
        // Arrange
        var invalidUserId = "nonexistent-user";
        var rating = 0;
        var createMovieDTO = new CreateMovieDto("New Movie", 2023, "A great movie", "Actor 1", null);
        _mockService.Setup(s => s.CreateMovie(createMovieDTO, invalidUserId, It.IsAny<int?>())).ThrowsAsync(new Exception("User not found"));

        // Act
        var result = await _controller.CreateMovie(createMovieDTO, invalidUserId, rating);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Could not create movie", badRequest.Value);
    }

    [Fact]
    public async Task UpdateMovieRating_ShouldReturnOk_WhenServiceSucceeds()
    {
        // Arrange
        var userId = "user1";
        var movieId = "movie1";
        var rating = 8;
        _mockService.Setup(s => s.UpdateMovieRating(userId, movieId, rating)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateMovieRating(userId, movieId, rating);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task UpdateMovieRating_ShouldReturnBadRequest_WhenServiceThrows()
    {
        // Arrange
        var userId = "user1";
        var movieId = "movie1";
        var rating = 11;
        _mockService.Setup(s => s.UpdateMovieRating(userId, movieId, rating))
            .ThrowsAsync(new ArgumentException("Rating must be between 1 and 10"));

        // Act
        var result = await _controller.UpdateMovieRating(userId, movieId, rating);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Rating must be between 1 and 10", badRequest.Value);
    }

    [Fact]
    public async Task GetMovieRatingByUser_ShouldReturnOk_WithRating_WhenServiceSucceeds()
    {
        // Arrange
        var userId = "user1";
        var movieId = "movie1";
        var rating = 7;
        _mockService.Setup(s => s.GetMovieRatingByUser(userId, movieId)).ReturnsAsync(rating);

        // Act
        var result = await _controller.GetMovieRatingByUser(userId, movieId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(rating, okResult.Value);
    }

    [Fact]
    public async Task GetMovieRatingByUser_ShouldReturnBadRequest_WhenServiceThrows()
    {
        // Arrange
        var userId = "user1";
        var movieId = "movie1";
        _mockService.Setup(s => s.GetMovieRatingByUser(userId, movieId))
            .ThrowsAsync(new Exception("Error"));

        // Act
        var result = await _controller.GetMovieRatingByUser(userId, movieId);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Could not fetch movie rating", badRequest.Value);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // AddMovieToSeen Controller Tests
    // ══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task AddMovieToSeen_ShouldReturnOk_WhenServiceSucceeds()
    {
        // Arrange
        var movieId = "movie1";
        var userId = "user1";
        var seen = true;
        var movie = new Movie { Id = movieId, Title = "Test Movie", Year = 2020, Starring = "Actor", Description = "Desc" };
        
        _mockService.Setup(s => s.AddMovieToSeen(movieId, userId, seen))
            .ReturnsAsync(movie);

        // Act
        var result = await _controller.AddMovieToSeen(movieId, userId, seen);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedMovie = Assert.IsType<Movie>(okResult.Value);
        Assert.Equal(movieId, returnedMovie.Id);
    }

    [Fact]
    public async Task AddMovieToSeen_ShouldReturnOk_WhenMarkingAsUnseen()
    {
        // Arrange
        var movieId = "movie1";
        var userId = "user1";
        var seen = false;
        var movie = new Movie { Id = movieId, Title = "Test Movie", Year = 2020, Starring = "Actor", Description = "Desc" };
        
        _mockService.Setup(s => s.AddMovieToSeen(movieId, userId, seen))
            .ReturnsAsync(movie);

        // Act
        var result = await _controller.AddMovieToSeen(movieId, userId, seen);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task AddMovieToSeen_ShouldReturnBadRequest_WhenServiceThrowsKeyNotFoundException()
    {
        // Arrange
        var movieId = "nonexistent-movie";
        var userId = "user1";
        var seen = true;
        
        _mockService.Setup(s => s.AddMovieToSeen(movieId, userId, seen))
            .ThrowsAsync(new KeyNotFoundException("Movie not found"));

        // Act
        var result = await _controller.AddMovieToSeen(movieId, userId, seen);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Could not add movie to seen", badRequest.Value);
    }

    [Fact]
    public async Task AddMovieToSeen_ShouldReturnBadRequest_WhenUserNotFound()
    {
        // Arrange
        var movieId = "movie1";
        var userId = "nonexistent-user";
        var seen = true;
        
        _mockService.Setup(s => s.AddMovieToSeen(movieId, userId, seen))
            .ThrowsAsync(new KeyNotFoundException("User not found"));

        // Act
        var result = await _controller.AddMovieToSeen(movieId, userId, seen);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Could not add movie to seen", badRequest.Value);
    }

    [Fact]
    public async Task AddMovieToSeen_ShouldReturnBadRequest_WhenUserMovieNotFound()
    {
        // Arrange
        var movieId = "movie1";
        var userId = "user1";
        var seen = true;
        
        _mockService.Setup(s => s.AddMovieToSeen(movieId, userId, seen))
            .ThrowsAsync(new KeyNotFoundException("User movie not found"));

        // Act
        var result = await _controller.AddMovieToSeen(movieId, userId, seen);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Could not add movie to seen", badRequest.Value);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // GetMoviesByUser with MovieWithSeenDto Tests
    // ══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetMoviesByUser_ShouldReturnMoviesWithSeenDto()
    {
        // Arrange
        var userId = "user1";
        var moviesWithSeen = new List<MovieWithSeenDto>
        {
            new MovieWithSeenDto("1", "Seen Movie", 2020, "Desc 1", "Actor 1", null, true, 8),
            new MovieWithSeenDto("2", "Unseen Movie", 2021, "Desc 2", "Actor 2", null, false, null)
        };
        
        _mockService.Setup(s => s.GetMoviesByUser(userId))
            .ReturnsAsync(moviesWithSeen);

        // Act
        var result = await _controller.GetMoviesByUser(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedMovies = Assert.IsType<List<MovieWithSeenDto>>(okResult.Value);
        Assert.Equal(2, returnedMovies.Count);
        
        var seenMovie = returnedMovies.First(m => m.Id == "1");
        Assert.True(seenMovie.Seen);
        Assert.Equal(8, seenMovie.Rating);
        
        var unseenMovie = returnedMovies.First(m => m.Id == "2");
        Assert.False(unseenMovie.Seen);
        Assert.Null(unseenMovie.Rating);
    }

}
