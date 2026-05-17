using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace KiranaStoreUI.Controllers
{
    public class UserController : Controller
    {
        private readonly HttpClient _httpClient;

        public UserController()
        {
            _httpClient = new HttpClient();

            // YOUR RENDER BACKEND URL
            _httpClient.BaseAddress =
                new Uri("https://kirana-store-main.onrender.com/");
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

                var json =
                    JsonConvert.SerializeObject(loginData);

                var content =
                    new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json"
                    );

                var response =
                    await _httpClient.PostAsync(
                        "api/Auth/Login",
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