using api.Services.Interfaces;
using efscaffold;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Classes;

public class GenreService(MyDbContext context) : IGenreService
{
    public Task<Genre> CreateGenre(string genreName)
    {
        if (context.Genres.Any(g => g.Name == genreName))
        {
            throw new KeyNotFoundException("Genre already exists");
        }

        Genre genre = new Genre()
        {
            Id = Guid.NewGuid().ToString(),
            Name = genreName
        };
        context.Genres.Add(genre);
        context.SaveChanges();
        return Task.FromResult(genre);
    }

    public Task<List<Genre>> GetAllGenres()
    {
        return context.Genres.ToListAsync();
    }

    public Task DeleteGenre(string genreId)
    {
        Genre? genre = context.Genres.FirstOrDefault(g => g.Id == genreId);
        if (genre == null)
        {
            throw new KeyNotFoundException("Genre not found");
        }
        
        context.Genres.Remove(genre);
        context.SaveChanges();
        return Task.CompletedTask;
    }
}