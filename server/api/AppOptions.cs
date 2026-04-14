using System.ComponentModel.DataAnnotations;

namespace api;

public class AppOptions
{
    [Required]
    [Length(1, 1000)]
    public string DBConnectionString { get; set; } = string.Empty;
    [Required]
    [Length(1, 1000)]
    public string GoogleApiKey { get; set; } = string.Empty;
    [Required]
    [Length(1, 1000)]
    public string GoogleBucketName { get; set; } = string.Empty;
}