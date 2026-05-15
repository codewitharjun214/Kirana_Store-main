[HttpPost]
public async Task<IActionResult> Login(LoginModel model)
{
    if (!ModelState.IsValid)
        return View(model);

    var client = _factory.CreateClient("api");

    var loginDto = new
    {
        Username = model.Username,
        Password = model.Password
    };

    var response = await client.PostAsJsonAsync("Auth/Login", loginDto);

    if (response.IsSuccessStatusCode)
    {
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

        // FIXED REDIRECT
        return RedirectToAction("Index", "Home");
    }

    ModelState.AddModelError("", "Invalid username or password");
    return View(model);
}