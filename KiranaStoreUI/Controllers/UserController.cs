using KiranaStoreUI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace KiranaStoreUI.Controllers
{
    public class UserController : Controller
    {
        private readonly IHttpClientFactory _factory;

        public UserController(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        private HttpClient CreateClientWithToken()
        {
            var client = _factory.CreateClient("api");

            var token = HttpContext.Session.GetString("JWToken");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        // ================= LOGIN GET =================
        public IActionResult Login()
        {
            return View();
        }

        // ================= LOGIN POST =================
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var client = _factory.CreateClient("api");

                var loginDto = new
                {
                    Username = model.Username,
                    Password = model.Password
                };

                var response = await client.PostAsJsonAsync("api/Auth/Login", loginDto);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();

                    ModelState.AddModelError("", $"Login failed: {error}");

                    return View(model);
                }

                var json = await response.Content.ReadAsStringAsync();

                var doc = JsonDocument.Parse(json);

                var token = doc.RootElement.GetProperty("token").GetString();
                var role = doc.RootElement.GetProperty("role").GetString();
                var username = doc.RootElement.GetProperty("username").GetString();

                HttpContext.Session.SetString("JWToken", token);
                HttpContext.Session.SetString("Username", username);
                HttpContext.Session.SetString("Role", role);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, role)
                };

                var identity = new ClaimsIdentity(claims, "Cookies");

                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("Cookies", principal);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                return View(model);
            }
        }

        // ================= REGISTER GET =================
        public IActionResult Register()
        {
            return View();
        }

        // ================= REGISTER POST =================
        [HttpPost]
        public async Task<IActionResult> Register(User model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var client = _factory.CreateClient("api");

                var registerDto = new
                {
                    FullName = model.FullName,
                    Username = model.Username,
                    Password = model.Password,
                    Phone = model.Phone,
                    Role = model.Role
                };

                var response =
                    await client.PostAsJsonAsync("api/Auth/Register", registerDto);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Login");
                }

                var error = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError("", $"Registration failed: {error}");

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                return View(model);
            }
        }

        // ================= LOGOUT =================
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();

            await HttpContext.SignOutAsync("Cookies");

            return RedirectToAction("Login");
        }
    }
}