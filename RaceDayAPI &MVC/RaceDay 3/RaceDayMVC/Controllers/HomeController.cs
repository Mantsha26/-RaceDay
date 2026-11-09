using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;
namespace RaceDayMVC.Controllers;

public class HomeController(ApiClient api) : Controller
{
    public async Task<IActionResult> Index(string? search, string? type, decimal? maxDistance, DateTime? from)
    {
        if (HttpContext.Session.GetString("Role") == "Organiser")
            return RedirectToAction("Dashboard", "Organiser");

        List<RaceEvent> events;
        try
        {
            events = await api.Get<List<RaceEvent>>("api/events") ?? new();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or ApiException)
        {
            events = new();
            ViewBag.Error = "Races can't be loaded right now. Please try again in a moment.";
        }

        var start = from.HasValue && from.Value.Date > DateTime.Today ? from.Value.Date : DateTime.Today;
        var query = events.Where(e => e.EventDate.Date >= start);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(e => e.EventName.Contains(search, StringComparison.OrdinalIgnoreCase)
                                  || e.Location.Contains(search, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(e => string.Equals(e.EventType, type, StringComparison.OrdinalIgnoreCase));
        if (maxDistance is > 0)
            query = query.Where(e => e.DistanceKm <= maxDistance);

        ViewBag.Search = search;
        ViewBag.Type = type;
        ViewBag.MaxDistance = maxDistance;
        ViewBag.From = from?.ToString("yyyy-MM-dd");
        return View(query.OrderBy(e => e.EventDate).ToList());
    }

    // Public: guests can read about a race before deciding to join.
    public async Task<IActionResult> Details(int id)
    {
        var ev = await api.Get<RaceEvent>($"api/events/{id}");
        if (ev == null)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            ViewData["Message"] = "We couldn't find that race. It may have been removed.";
            return View("Error");
        }
        return View(ev);
    }

    public IActionResult Error(int? code)
    {
        if (code == 404)
        {
            ViewData["Message"] = "We couldn't find that page.";
        }
        else
        {
            ViewData["Message"] ??= "Something went wrong on our side. Please try again.";
        }
        return View();
    }
}
