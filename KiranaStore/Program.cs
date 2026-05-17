using DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BLL.Services;

var builder = WebApplication.CreateBuilder(args);

// =======================================
// SERVICES
// =======================================

builder.Services.AddControllers();

// DATABASE
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString =
        builder.Configuration.GetConnectionString("DefaultConnection");

    if (builder.Environment.IsProduction())
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        if (string.IsNullOrWhiteSpace(connectionString)
            || connectionString.Contains("Server=localhost")
            || connectionString.Contains("Trusted_Connection"))
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

// =======================================
// DEPENDENCY INJECTION
// =======================================

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<SaleService>();
builder.Services.AddScoped<UserService>();

// =======================================
// SWAGGER
// =======================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =======================================
// CORS
// =======================================

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

// =======================================
// MIDDLEWARE
// =======================================

app.UseSwagger();
app.UseSwaggerUI();

// Render handles HTTPS
// app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

// =======================================
// AUTO MIGRATION
// =======================================

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
        var logger =
            services.GetService<ILogger<Program>>();

        logger?.LogError(
            ex,
            "Database migration error"
        );

        if (app.Environment.IsDevelopment())
            throw;
    }
}

// =======================================
// ROOT ROUTE
// =======================================

app.MapGet("/", () =>
    "Kirana Store Backend Running Successfully");

// =======================================
// CONTROLLERS
// =======================================

app.MapControllers();

app.Run();