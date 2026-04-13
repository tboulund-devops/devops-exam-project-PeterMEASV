namespace api.Models;

public record CreateMovieDTO(string Title, int Year, string? Description, string? Starring);