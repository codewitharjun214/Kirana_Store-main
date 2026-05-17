using DAL.Data;
using Microsoft.EntityFrameworkCore;
using BLL.Services;

using DAL.Repository.Interface;
using DAL.Repository.Interfaces;
using DAL.Repository.Implimentation;
using DAL.Repository.Implementation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IPurchaseItemRepository, PurchaseItemRepository>();
builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();
builder.Services.AddScoped<ISaleItemRepository, SaleItemRepository>();
builder.Services.AddScoped<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<IStockRepository, StockRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<SaleService>();

// =======================================
// JWT AUTHENTICATION
// =======================================

var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") 
    ?? builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("JWT_KEY environment variable or Jwt:Key in configuration must be set");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

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

app.UseAuthentication();
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

// =======================================
// DATABASE INITIALIZATION
// =======================================

try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Database migration error: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}

app.Run();