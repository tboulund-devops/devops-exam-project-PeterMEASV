using efscaffold;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using api.Services.Classes;
using api.Services.Interfaces;
using api.Security;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Tests;

public class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddCors();
        services.AddControllers();
        
        // Register the password hasher
        services.AddScoped<IPasswordHasher<User>, KonciousArgon2idPasswordHasher>();
        
        // Register the mocked StorageClient
        var mockStorageClient = new Mock<StorageClient>();
        mockStorageClient
            .Setup(c => c.UploadObjectAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Stream>(), null, default, null))
            .ReturnsAsync(new Google.Apis.Storage.v1.Data.Object());
        
        services.AddScoped(_ => mockStorageClient.Object);
        services.AddScoped<IStorageService>(provider => 
            new StorageService(provider.GetRequiredService<StorageClient>(), "test-bucket"));
        
        services.AddScoped<IMovieService, MovieService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPasswordHasher<User>, KonciousArgon2idPasswordHasher>();
        
        // Use a fresh in-memory database per test scope to avoid conflicts
        services.AddScoped<MyDbContext>(factory =>
        {
            var options = new DbContextOptionsBuilder<MyDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            var ctx = new MyDbContext(options);
            ctx.Database.EnsureCreated();
            return ctx;
        });
    }
}