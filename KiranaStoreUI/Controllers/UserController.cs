using KiranaStoreUI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace KiranaStoreUI.Controllers
{
    public class UserController : Controller
    {
        private readonly HttpClient _client;

        public UserController(IHttpClientFactory factory)
        {
            _client = factory.CreateClient("api");
        }

        // ======================================
        // LOGIN PAGE
        // ======================================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // ======================================
        // LOGIN POST
        // ======================================

        [HttpPost]
        public async Task<IActionResult> Login(Login loginDto)
        {
            try
            {
                var response =
                    await _client.PostAsJsonAsync(
                        "Auth/Login",
                        loginDto
                    );

                if (response.IsSuccessStatusCode)
                {
                    var result =
                        await response.Content
                        .ReadFromJsonAsync<LoginResponse>();

                    if (result != null)
                    {
                        HttpContext.Session.SetString(
                            "JWToken",
                            result.Token
                        );

                        HttpContext.Session.SetString(
                            "Username",
                            result.Username
                        );

                        return RedirectToAction(
                            "Dashboard",
                            "DashBoard"
                        );
                    }
                }

                ViewBag.Error =
                    "Invalid Username or Password";

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;

                return View();
            }
        }

        // ======================================
        // LOGOUT
        // ======================================

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }
}