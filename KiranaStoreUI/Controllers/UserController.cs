using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace KiranaStoreUI.Controllers
{
    public class UserController : Controller
    {
        private readonly HttpClient _httpClient;

        public UserController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("api");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            try
            {
                var loginData = new
                {
                    Username = username,
                    Password = password
                };

                var json = JsonSerializer.Serialize(loginData);

                var content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(
                    "Auth/Login",
                    content
                );

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(
                        "Index",
                        "Dashboard"
                    );
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
    }
}