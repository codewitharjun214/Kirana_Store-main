var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var apiBase = builder.Configuration["ApiBaseUrl"];

if (string.IsNullOrWhiteSpace(apiBase))
{
    apiBase = builder.Environment.IsDevelopment()
        ? "http://localhost:5000/api/"
        : "https://kirana-store-main.onrender.com/api/";
}

builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri(apiBase);
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();