using api.Controllers;
using api.Services.Interfaces;
using efscaffold;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests;

public class GenreControllerTest
{
    private readonly Mock<IGenreService> _mockService;
    private readonly GenreController _controller;

    public GenreControllerTest()
    {
        _mockService = new Mock<IGenreService>();
        _controller = new GenreController(_mockService.Object);
    }
    
    [Fact]
    public async Task CreateGenre_ShouldReturnOk_WithCreatedGenre_WhenServiceSucceeds()
    {
        // Arrange
        var genreName = "Action";
        var genre = new Genre { Id = "genre1", Name = genreName };
        _mockService.Setup(s => s.CreateGenre(genreName)).ReturnsAsync(genre);

        // Act
        var result = await _controller.CreateGenre(genreName);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(genre, okResult.Value);
    }

    [Fact]
    public async Task CreateGenre_ShouldReturnBadRequest_WhenServiceThrows()
    {
        // Arrange
        var genreName = "Action";
        _mockService.Setup(s => s.CreateGenre(genreName))
            .ThrowsAsync(new KeyNotFoundException("Genre already exists"));

        // Act
        var result = await _controller.CreateGenre(genreName);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Could not create genre", badRequest.Value);
    }

    [Fact]
    public async Task GetAllGenres_ShouldReturnOk_WithGenreList_WhenServiceSucceeds()
    {
        // Arrange
        var genres = new List<Genre>
        {
            new Genre { Id = "1", Name = "Action" },
            new Genre { Id = "2", Name = "Drama" },
        };
        _mockService.Setup(s => s.GetAllGenres()).ReturnsAsync(genres);

        // Act
        var result = await _controller.GetAllGenres();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedGenres = Assert.IsType<List<Genre>>(okResult.Value);
        Assert.Equal(2, returnedGenres.Count);
    }

    [Fact]
    public async Task GetAllGenres_ShouldReturnOk_WithEmptyList_WhenNoGenresExist()
    {
        // Arrange
        _mockService.Setup(s => s.GetAllGenres()).ReturnsAsync(new List<Genre>());

        // Act
        var result = await _controller.GetAllGenres();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedGenres = Assert.IsType<List<Genre>>(okResult.Value);
        Assert.Empty(returnedGenres);
    }

    [Fact]
    public async Task GetAllGenres_ShouldReturnBadRequest_WhenServiceThrows()
    {
        // Arrange
        _mockService.Setup(s => s.GetAllGenres())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAllGenres();

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Could not fetch genres", badRequest.Value);
    }

    [Fact]
    public async Task DeleteGenre_ShouldReturnOk_WhenServiceSucceeds()
    {
        // Arrange
        var genreId = "genre1";
        _mockService.Setup(s => s.DeleteGenre(genreId)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteGenre(genreId);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeleteGenre_ShouldReturnBadRequest_WhenServiceThrows()
    {
        // Arrange
        var genreId = "nonexistent-id";
        _mockService.Setup(s => s.DeleteGenre(genreId))
            .ThrowsAsync(new KeyNotFoundException("Genre not found"));

        // Act
        var result = await _controller.DeleteGenre(genreId);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Could not delete genre", badRequest.Value);
    }
}
