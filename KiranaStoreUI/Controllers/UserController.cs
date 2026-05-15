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

        // LOGIN PAGE
        public IActionResult Login()
        {
            return View();
        }

        // LOGIN POST
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var client = _factory.CreateClient("api");

                var response = await client.PostAsJsonAsync(
                    "api/Auth/Login",
                    new
                    {
                        Username = model.Username,
                        Password = model.Password
                    });

                var responseText =
                    await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("",
                        $"Login Failed : {responseText}");

                    return View(model);
                }

                var json = JsonDocument.Parse(responseText);

                var token =
                    json.RootElement.GetProperty("token").GetString();

                var role =
                    json.RootElement.GetProperty("role").GetString();

                var username =
                    json.RootElement.GetProperty("username").GetString();

                HttpContext.Session.SetString("JWToken", token);
                HttpContext.Session.SetString("Username", username);
                HttpContext.Session.SetString("Role", role);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, role)
                };

                var identity =
                    new ClaimsIdentity(claims, "Cookies");

                var principal =
                    new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    "Cookies",
                    principal);

                return RedirectToAction(
                    "Index",
                    "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                return View(model);
            }
        }

        // LOGOUT
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();

            await HttpContext.SignOutAsync("Cookies");

            return RedirectToAction("Login");
        }
    }
}