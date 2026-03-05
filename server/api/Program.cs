using api;
using efscaffold;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var appOptions = builder.Services.AddAppOptions(builder.Configuration);

builder.Services.AddCors();
builder.Services.AddDbContext<MyDbContext>(conf =>
{
    conf.UseNpgsql(appOptions.DBConnectionString);
});
builder.Services.AddControllers();
builder.Services.AddOpenApiDocument();


var app = builder.Build();

app.UseCors(config => config.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.MapControllers();
app.UseOpenApi();
app.UseSwaggerUi();
app.GenerateApiClientsFromOpenApi("/../../client/src/generated-ts-client.ts");

app.Run();
