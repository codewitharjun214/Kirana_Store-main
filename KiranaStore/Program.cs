using DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

// DATABASE: choose provider based on environment and connection string
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    if (builder.Environment.IsProduction())
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        // For local development, if the connection string looks like SQL Server localhost
        // fall back to a lightweight SQLite DB so the project runs out-of-the-box.
        if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("Server=localhost") || connectionString.Contains("Trusted_Connection"))
        {
            options.UseSqlite("Data Source=kirana_dev.db");
        }
        else if (connectionString.Contains("Host="))
        {
            options.UseNpgsql(connectionString);
        }
        else
        {
            options.UseSqlServer(connectionString);
        }
    }
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// ENABLE SWAGGER IN PRODUCTION ALSO
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

// Apply EF Core migrations at startup (safe for most deployments). Wrap in try/catch.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetService<ILogger<Program>>();
        logger?.LogError(ex, "An error occurred while migrating or initializing the database.");
        // Rethrow in development so issues are obvious
        if (app.Environment.IsDevelopment()) throw;
    }
}

// ROOT ROUTE
app.MapGet("/", () => "Kirana Store Backend Running Successfully");

app.MapControllers();

app.Run();