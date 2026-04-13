using System.ComponentModel.DataAnnotations;

namespace api.Models;

public record LoginDTO(
    [Required]
    [EmailAddress]
    string Email,
    [Required]
    string Password
);

public record LoginResponseDTO(
    string id,
    string email,
    string token,
    string message
);