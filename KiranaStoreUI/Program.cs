var builder = WebApplication.CreateBuilder(args);

// ==========================================
// Add services to the container
// ==========================================

builder.Services.AddControllersWithViews();

// Backend API Connection
builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri("https://kirana-store-main.onrender.com/api/");
});

// Session Configuration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Access HttpContext
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ==========================================
// Configure HTTP Request Pipeline
// ==========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// Enable Session
app.UseSession();

// Authorization
app.UseAuthorization();

// ==========================================
// Default Route
// ==========================================

// When opening:
// https://kirana-store-main-1.onrender.com
// it will automatically open login page

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

// ==========================================
// Run Application
// ==========================================

app.Run();