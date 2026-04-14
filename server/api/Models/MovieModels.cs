using System.ComponentModel.DataAnnotations;

namespace api.Models;

public record CreateMovieDto([Required]string Title, [Required]int Year, string? Description, string? Starring, IFormFile? Photo);