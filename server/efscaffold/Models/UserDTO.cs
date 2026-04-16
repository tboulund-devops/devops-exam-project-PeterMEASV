using System.ComponentModel.DataAnnotations;

namespace efscaffold.Models;

public record CreateUserDto([Required] string userId, [Required] [EmailAddress] string Email,[Required] [MinLength(8)] string Password, string? Name);