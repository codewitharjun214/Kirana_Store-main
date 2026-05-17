using KiranaStoreUI.Models;

var builder = WebApplication.CreateBuilder(args);

// ======================================
// MVC
// ======================================

builder.Services.AddControllersWithViews();

// ======================================
// SESSION
// ======================================

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ======================================
// HTTP CLIENT
// ======================================

var apiBaseUrl = Environment.GetEnvironmentVariable("API_BASE_URL")
    ?? builder.Configuration["ApiBaseUrl"]
    ?? "http://localhost:5013/api/";

builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// ======================================
// BUILD
// ======================================

var app = builder.Build();

// ======================================
// MIDDLEWARE
// ======================================

// Render handles HTTPS
// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

// ======================================
// ROUTES
// ======================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Login}/{id?}"
);

// ======================================
// ROOT REDIRECT
// ======================================

app.MapGet("/", () =>
{
    return Results.Redirect("/User/Login");
});

app.Run();