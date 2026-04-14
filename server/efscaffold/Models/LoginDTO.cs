using System.ComponentModel.DataAnnotations;

namespace api.Models;

public record LoginDto(
    [Required]
    [EmailAddress]
    string Email,
    [Required]
    string Password
);

public record LoginResponseDto(
    string Id,
    string Email,
    string Token,
    string Message
);