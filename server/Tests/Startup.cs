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
    private static PostgreSqlContainer? _postgreSqlContainer;
    private static readonly object _lockObject = new();

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddCors();
        services.AddControllers();
        services.AddScoped<IMovieService, MovieService>();
        
        // Ensure container is started only once
        if (_postgreSqlContainer == null)
        {
            lock (_lockObject)
            {
                if (_postgreSqlContainer == null)
                {
                    _postgreSqlContainer = new PostgreSqlBuilder().Build();
                    _postgreSqlContainer.StartAsync().GetAwaiter().GetResult();
                }
            }
        }

        // Reuse the same container for all tests
        services.AddScoped<MyDbContext>(factory =>
        {
            var connectionString = _postgreSqlContainer.GetConnectionString();
            var options = new DbContextOptionsBuilder<MyDbContext>()
                .UseNpgsql(connectionString)
                .Options;
            
            var ctx = new MyDbContext(options);
            ctx.Database.EnsureCreated();
            return ctx;
        });
    }

    // Helper method to reset database (clear all data)
    public static async Task ResetDatabaseAsync(MyDbContext dbContext)
    {
        // Delete in correct order to respect foreign keys
        dbContext.UsersMovies.RemoveRange(dbContext.UsersMovies);
        dbContext.Movies.RemoveRange(dbContext.Movies);
        dbContext.Users.RemoveRange(dbContext.Users);
        await dbContext.SaveChangesAsync();
    }
}