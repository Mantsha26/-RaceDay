using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;
namespace RaceDayMVC.Controllers;

public class AccountController(ApiClient api) : Controller
{
    private const string Unreachable = "RaceDay can't reach its server right now. Please try again in a moment.";

    [HttpGet]
    public IActionResult Login()
    {
        if (HttpContext.Session.GetString("ApiToken") != null) return RedirectToAction("Index", "Home");
        return View(new LoginForm());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginForm form)
    {
        if (!ModelState.IsValid) return View(form);
        try
        {
            var (ok, data) = await api.Send("POST", "api/auth/login", form);
            if (ok && data.TryGetProperty("token", out var token))
            {
                HttpContext.Session.Clear();
                HttpContext.Session.SetString("ApiToken", token.GetString() ?? string.Empty);
                HttpContext.Session.SetString("Role", data.GetProperty("role").GetString() ?? string.Empty);
                HttpContext.Session.SetInt32("UserId", data.GetProperty("userId").GetInt32());
                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError(string.Empty, ApiClient.Message(data, "Login failed. Check your email and password."));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or ApiException)
        {
            ModelState.AddModelError(string.Empty, Unreachable);
        }
        return View(form);
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterForm());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterForm form)
    {
        if (string.Equals(form.Role, "Participant", StringComparison.OrdinalIgnoreCase))
        {
            if (form.DateOfBirth is null)
                ModelState.AddModelError(nameof(form.DateOfBirth), "Date of birth is required for participants.");
            else if (form.DateOfBirth.Value.Date >= DateTime.Today)
                ModelState.AddModelError(nameof(form.DateOfBirth), "Enter a date of birth in the past.");
        }
        if (!ModelState.IsValid) return View(form);
        try
        {
            var (ok, data) = await api.Send("POST", "api/auth/register", form);
            if (ok)
            {
                TempData["Notice"] = "Account created. Log in to get started.";
                return RedirectToAction(nameof(Login));
            }
            ModelState.AddModelError(string.Empty, ApiClient.Message(data, "Registration failed. Check your details and try again."));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or ApiException)
        {
            ModelState.AddModelError(string.Empty, Unreachable);
        }
        return View(form);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["Notice"] = "You have been logged out.";
        return RedirectToAction("Index", "Home");
    }
}
