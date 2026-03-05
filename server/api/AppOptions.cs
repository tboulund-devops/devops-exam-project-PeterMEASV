using System.ComponentModel.DataAnnotations;

namespace api;

public class AppOptions
{
    [Required]
    [Length(1, 1000)]
    public string DBConnectionString { get; set; } = string.Empty;
}