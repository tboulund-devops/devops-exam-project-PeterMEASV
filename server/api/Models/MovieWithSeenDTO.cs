using System.ComponentModel.DataAnnotations;

namespace api.Models;

public record CreateMovieDTO([Required]string Title, [Required]int Year, string? Description, string? Starring, IFormFile? Photo);

public record MovieWithSeenDto(
    string Id,
    string Title,
    int Year,
    string? Description,
    string? Starring,
    string? Photo,
    bool? Seen,
    int? Rating
);