using api.Services.Classes;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Tests;

public class StorageServiceTest
{
    private readonly Mock<StorageClient> _mockClient;
    private const string BucketName = "test-bucket";

    public StorageServiceTest()
    {
        _mockClient = new Mock<StorageClient>();
    }

    [Fact]
    public async Task UploadPhotoAsync_ShouldReturnCorrectUrl()
    {
        // Arrange
        var service = new StorageService(_mockClient.Object, BucketName);
        var mockFile = CreateMockFile("poster.jpg", "image/jpeg");

        _mockClient
            .Setup(c => c.UploadObjectAsync(
                BucketName, It.IsAny<string>(), "image/jpeg",
                It.IsAny<Stream>(), null, default, null))
            .ReturnsAsync(new Google.Apis.Storage.v1.Data.Object());

        // Act
        var result = await service.UploadPhotoAsync(mockFile.Object);

        // Assert
        Assert.StartsWith($"https://storage.googleapis.com/{BucketName}/", result);
    }

    [Fact]
    public async Task UploadPhotoAsync_ShouldUseProvidedFileName()
    {
        // Arrange
        var service = new StorageService(_mockClient.Object, BucketName);
        var mockFile = CreateMockFile("original.jpg", "image/jpeg");
        var customName = "custom-name.jpg";

        _mockClient
            .Setup(c => c.UploadObjectAsync(
                BucketName, customName, "image/jpeg",
                It.IsAny<Stream>(), null, default, null))
            .ReturnsAsync(new Google.Apis.Storage.v1.Data.Object());

        // Act
        var result = await service.UploadPhotoAsync(mockFile.Object, customName);

        // Assert
        Assert.Equal($"https://storage.googleapis.com/{BucketName}/{customName}", result);
    }

    [Fact]
    public async Task UploadPhotoAsync_WithNoFileName_ShouldPreserveExtension()
    {
        // Arrange
        var service = new StorageService(_mockClient.Object, BucketName);
        var mockFile = CreateMockFile("poster.png", "image/png");

        _mockClient
            .Setup(c => c.UploadObjectAsync(
                BucketName, It.IsAny<string>(), "image/png",
                It.IsAny<Stream>(), null, default, null))
            .ReturnsAsync(new Google.Apis.Storage.v1.Data.Object());

        // Act
        var result = await service.UploadPhotoAsync(mockFile.Object);

        // Assert
        Assert.EndsWith(".png", result);
    }

    [Fact]
    public async Task UploadPhotoAsync_WithNoFileName_ShouldGenerateUniqueNames()
    {
        // Arrange
        var service = new StorageService(_mockClient.Object, BucketName);
        var mockFile = CreateMockFile("poster.jpg", "image/jpeg");

        _mockClient
            .Setup(c => c.UploadObjectAsync(
                BucketName, It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Stream>(), null, default, null))
            .ReturnsAsync(new Google.Apis.Storage.v1.Data.Object());

        // Act
        var result1 = await service.UploadPhotoAsync(mockFile.Object);
        var result2 = await service.UploadPhotoAsync(mockFile.Object);

        // Assert
        Assert.NotEqual(result1, result2);
    }

    [Fact]
    public async Task UploadPhotoAsync_ShouldCallClientWithCorrectBucket()
    {
        // Arrange
        var service = new StorageService(_mockClient.Object, BucketName);
        var mockFile = CreateMockFile("poster.jpg", "image/jpeg");

        _mockClient
            .Setup(c => c.UploadObjectAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Stream>(), null, default, null))
            .ReturnsAsync(new Google.Apis.Storage.v1.Data.Object());

        // Act
        await service.UploadPhotoAsync(mockFile.Object);

        // Assert
        _mockClient.Verify(c => c.UploadObjectAsync(
            BucketName, It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<Stream>(), null, default, null), Times.Once);
    }

    [Fact]
    public async Task UploadPhotoAsync_WhenClientThrows_ShouldPropagate()
    {
        // Arrange
        var service = new StorageService(_mockClient.Object, BucketName);
        var mockFile = CreateMockFile("poster.jpg", "image/jpeg");

        _mockClient
            .Setup(c => c.UploadObjectAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Stream>(), null, default, null))
            .ThrowsAsync(new Exception("GCS unavailable"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => service.UploadPhotoAsync(mockFile.Object));
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Mock<IFormFile> CreateMockFile(string fileName, string contentType)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.ContentType).Returns(contentType);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
        return mockFile;
    }
}
