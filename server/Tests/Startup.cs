using efscaffold;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using api.Services.Classes;
using api.Services.Interfaces;

namespace Tests;

public class Startup
{
    private static string? _dbPath;
    private static readonly object _lockObject = new();

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors();
        services.AddControllers();
        services.AddScoped<IMovieService, MovieService>();
        
        // Use a temporary SQLite database file (persists across test instances)
        if (_dbPath == null)
        {
            lock (_lockObject)
            {
                if (_dbPath == null)
                {
                    var tempPath = Path.Combine(Path.GetTempPath(), "stryker-tests");
                    Directory.CreateDirectory(tempPath);
                    _dbPath = Path.Combine(tempPath, $"test-{Guid.NewGuid()}.db");
                }
            }
        }

        services.AddScoped<MyDbContext>(factory =>
        {
            var options = new DbContextOptionsBuilder<MyDbContext>()
                .UseSqlite($"Data Source={_dbPath}")
                .Options;
            
            var ctx = new MyDbContext(options);
            ctx.Database.EnsureCreated();
            return ctx;
        });
    }
}