var builder = WebApplication.CreateBuilder(args);

// ==========================================
// Add MVC Services
// ==========================================
builder.Services.AddControllersWithViews();

// ==========================================
// Backend API URL
// ==========================================
var apiBase = builder.Configuration["ApiBaseUrl"];

if (string.IsNullOrWhiteSpace(apiBase))
{
    apiBase = builder.Environment.IsDevelopment()
        ? "http://localhost:5000/api/"
        : "https://kirana-store-main.onrender.com/api/";
}

// ==========================================
// HTTP Client
// ==========================================
builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri(apiBase);
});

// ==========================================
// Session
// ==========================================
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ==========================================
// HttpContext
// ==========================================
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ==========================================
// Error Handling
// ==========================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// ==========================================
// Middleware
// ==========================================

// Render handles HTTPS
// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

// ==========================================
// Default MVC Route
// ==========================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Login}/{id?}");

// ==========================================
// Root URL Redirect
// ==========================================
app.MapGet("/", () =>
{
    return Results.Redirect("/User/Login");
});

// ==========================================
// Run App
// ==========================================
app.Run();