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
        var movies = new List<Movie>
        {
            new Movie { Id = "1", Title = "Movie 1", Year = 2020, Starring = "Actor 1", Description = "Desc 1" }
        };
        _mockService.Setup(s => s.GetMoviesByUser(userId)).ReturnsAsync(movies);

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
        var createMovieDTO = new CreateMovieDto("New Movie", 2023, "A great movie", "Actor 1", null);
        var createdMovie = new Movie { Id = Guid.NewGuid().ToString(), Title = "New Movie", Year = 2023, Starring = "Actor 1", Description = "A great movie" };
        _mockService.Setup(s => s.CreateMovie(createMovieDTO, userId)).ReturnsAsync(createdMovie);

        // Act
        var result = await _controller.CreateMovie(createMovieDTO, userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(createdMovie, okResult.Value);
    }

    [Fact]
    public async Task CreateMovie_ShouldReturnBadRequest_WhenServiceThrows()
    {
        // Arrange
        var invalidUserId = "nonexistent-user";
        var createMovieDTO = new CreateMovieDto("New Movie", 2023, "A great movie", "Actor 1", null);
        _mockService.Setup(s => s.CreateMovie(createMovieDTO, invalidUserId)).ThrowsAsync(new Exception("User not found"));

        // Act
        var result = await _controller.CreateMovie(createMovieDTO, invalidUserId);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Could not create movie", badRequest.Value);
    }
}
