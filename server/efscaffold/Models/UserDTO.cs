using System.ComponentModel.DataAnnotations;

namespace api.Models;

public record CreateUserDTO([Required] [EmailAddress] string email,[Required] [MinLength(8)] string password, string? name);