using DAL.Data;
using Microsoft.EntityFrameworkCore;
using BLL.Services;

using DAL.Repository.Interface;
using DAL.Repository.Implimentation;

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

builder.Services.AddScoped<IAuthRepository, AuthRepository>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<SaleService>();

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
// ROOT ROUTE
// =======================================

app.MapGet("/", () =>
    "Kirana Store Backend Running Successfully");

// =======================================
// CONTROLLERS
// =======================================

app.MapControllers();

app.Run();