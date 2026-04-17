using api.Services.Interfaces;
using efscaffold;

namespace Tests;

public class GenreServiceTest(IGenreService genreService, MyDbContext dbContext)
{
    private async Task ResetDatabaseAsync()
    {
        dbContext.MoviesGenres.RemoveRange(dbContext.MoviesGenres);
        dbContext.Genres.RemoveRange(dbContext.Genres);
        await dbContext.SaveChangesAsync();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // CreateGenre Tests
    // ══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task CreateGenre_ShouldCreateGenreSuccessfully()
    {
        // Arrange
        await ResetDatabaseAsync();
        var genreName = "Action";

        // Act
        var result = await genreService.CreateGenre(genreName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(genreName, result.Name);
        Assert.NotNull(result.Id);
        Assert.Single(dbContext.Genres);
    }

    [Fact]
    public async Task CreateGenre_ShouldGenerateUniqueId()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        var genre1 = await genreService.CreateGenre("Action");
        var genre2 = await genreService.CreateGenre("Drama");

        // Assert
        Assert.NotEqual(genre1.Id, genre2.Id);
    }

    [Fact]
    public async Task CreateGenre_WithDuplicateName_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        await ResetDatabaseAsync();
        await genreService.CreateGenre("Action");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => genreService.CreateGenre("Action"));
        Assert.Equal("Genre already exists", exception.Message);
    }

    [Fact]
    public async Task CreateGenre_ShouldPersistToDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();
        var genreName = "Comedy";

        // Act
        var result = await genreService.CreateGenre(genreName);

        // Assert
        var persisted = dbContext.Genres.FirstOrDefault(g => g.Id == result.Id);
        Assert.NotNull(persisted);
        Assert.Equal(genreName, persisted.Name);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // GetAllGenres Tests
    // ══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetAllGenres_ShouldReturnAllGenres()
    {
        // Arrange
        await ResetDatabaseAsync();
        dbContext.Genres.Add(new Genre { Id = "1", Name = "Action" });
        dbContext.Genres.Add(new Genre { Id = "2", Name = "Drama" });
        dbContext.Genres.Add(new Genre { Id = "3", Name = "Comedy" });
        await dbContext.SaveChangesAsync();

        // Act
        var result = await genreService.GetAllGenres();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Contains(result, g => g.Name == "Action");
        Assert.Contains(result, g => g.Name == "Drama");
        Assert.Contains(result, g => g.Name == "Comedy");
    }

    [Fact]
    public async Task GetAllGenres_WithNoGenres_ShouldReturnEmptyList()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        var result = await genreService.GetAllGenres();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // DeleteGenre Tests
    // ══════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task DeleteGenre_ShouldDeleteGenreSuccessfully()
    {
        // Arrange
        await ResetDatabaseAsync();
        var genre = new Genre { Id = "genre1", Name = "Action" };
        dbContext.Genres.Add(genre);
        await dbContext.SaveChangesAsync();

        // Act
        await genreService.DeleteGenre("genre1");

        // Assert
        Assert.Empty(dbContext.Genres);
    }

    [Fact]
    public async Task DeleteGenre_WithInvalidId_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => genreService.DeleteGenre("nonexistent-id"));
        Assert.Equal("Genre not found", exception.Message);
    }

    [Fact]
    public async Task DeleteGenre_ShouldNotAffectOtherGenres()
    {
        // Arrange
        await ResetDatabaseAsync();
        dbContext.Genres.Add(new Genre { Id = "genre1", Name = "Action" });
        dbContext.Genres.Add(new Genre { Id = "genre2", Name = "Drama" });
        await dbContext.SaveChangesAsync();

        // Act
        await genreService.DeleteGenre("genre1");

        // Assert
        Assert.Single(dbContext.Genres);
        Assert.Equal("Drama", dbContext.Genres.First().Name);
    }
}
