using api;
using api.Security;
using api.Services.Classes;
using api.Services.Interfaces;
using efscaffold;
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
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, JwtService>();
    }
}