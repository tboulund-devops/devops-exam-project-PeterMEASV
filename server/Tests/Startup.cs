using efscaffold;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using api.Services.Classes;
using api.Services.Interfaces;

namespace Tests;
public class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Add CORS and controllers (without calling Program.ConfigureServices)
        services.AddCors();
        services.AddControllers();
        services.AddScoped<IMovieService, MovieService>();
        
        // Setup test database with Testcontainers
        services.AddScoped<MyDbContext>(factory =>
        {
            var postgreSqlContainer = new PostgreSqlBuilder().Build();
            postgreSqlContainer.StartAsync().GetAwaiter().GetResult();
            var connectionString = postgreSqlContainer.GetConnectionString();
            var options = new DbContextOptionsBuilder<MyDbContext>()
                .UseNpgsql(connectionString)
                .Options;
            
            var ctx = new MyDbContext(options);
            ctx.Database.EnsureCreated();
            return ctx;
        });
    }
}