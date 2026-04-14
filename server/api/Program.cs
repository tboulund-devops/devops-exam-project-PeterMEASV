using api;
using api.Security;
using api.Services.Classes;
using api.Services.Interfaces;
using efscaffold;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api;
public class Program
{
    public static async Task Main()
    {
        var builder = WebApplication.CreateBuilder();

        ConfigureServices(builder.Services, builder.Configuration);
        
        var app = builder.Build();

        app.UseCors(config => config.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.UseOpenApi();
        app.UseSwaggerUi();
        await app.GenerateApiClientsFromOpenApi("/../../client/src/generated-ts-client.ts");

        await app.RunAsync();
    }

    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var appOptions = services.AddAppOptions(configuration);

        services.AddCors();
        services.AddDbContext<MyDbContext>(conf =>
        {
            conf.UseNpgsql(appOptions.DBConnectionString);
        });
        services.AddControllers();
        services.AddOpenApiDocument();
        services.AddScoped<IMovieService, MovieService>();
        services.AddSingleton<IStorageService>(sp =>
        {
            var env = sp.GetRequiredService<IWebHostEnvironment>();
            var keyPath = Path.IsPathRooted(appOptions.GoogleApiKey)
                ? appOptions.GoogleApiKey
                : Path.Combine(env.ContentRootPath, appOptions.GoogleApiKey);
            var json = File.ReadAllText(keyPath);
#pragma warning disable CS0618
            var credential = GoogleCredential.FromJson(json);
#pragma warning restore CS0618
            var client = StorageClient.Create(credential);
            return new StorageService(client, appOptions.GoogleBucketName);
        });
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, JwtService>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = JwtService.ValidationParameters(configuration);
            });
        services.AddAuthorization();
    }
}