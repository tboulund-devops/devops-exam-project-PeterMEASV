namespace api.Services.Interfaces;

public interface IStorageService
{
    Task<string> UploadPhotoAsync(IFormFile photo, string? fileName = null);
}
