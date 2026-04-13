using api;
using api.Services.Classes;
using api.Services.Interfaces;
using efscaffold;
using Microsoft.EntityFrameworkCore;

public class Program
{
    public static void Main()
    {
        var builder = WebApplication.CreateBuilder();

        ConfigureServices(builder.Services, builder.Configuration);
        
        var app = builder.Build();

        app.UseCors(config => config.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        app.MapControllers();
        app.UseOpenApi();
        app.UseSwaggerUi();
        app.GenerateApiClientsFromOpenApi("/../../client/src/generated-ts-client.ts");

        app.Run();
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
    }
}