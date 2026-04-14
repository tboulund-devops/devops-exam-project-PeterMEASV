using api.Services.Interfaces;
using Google.Cloud.Storage.V1;

namespace api.Services.Classes;

public class StorageService(StorageClient client, string bucketName) : IStorageService
{
    public async Task<string> UploadPhotoAsync(IFormFile photo, string? fileName = null)
    {
        if (photo == null)
        {
            return "https://storage.googleapis.com/default-placeholder.png";
        }

        var objectName = fileName ?? $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";

        using var stream = photo.OpenReadStream();
        await client.UploadObjectAsync(bucketName, objectName, photo.ContentType, stream);

        return $"https://storage.googleapis.com/{bucketName}/{objectName}";
    }
}
