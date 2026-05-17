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

// HttpContext Accessor
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

app.UseAuthorization();

// ==========================================
// Default Route
// ==========================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ==========================================
// Run Application
// ==========================================

app.Run();