using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;
namespace RaceDayMVC.Controllers;

public class OrganiserController(ApiClient api) : RoleController("Organiser")
{
    private static readonly string[] EventTypes = ["Road", "Trail", "Track"];

    private async Task<List<RaceEvent>> MyEvents() =>
        await api.Get<List<RaceEvent>>("api/organiser/events") ?? new();

    // Only events owned by this organiser are returned by the API, so this doubles as an ownership check.
    private async Task<RaceEvent?> MyEvent(int id) =>
        (await MyEvents()).FirstOrDefault(e => e.EventId == id);

    private IActionResult EventNotFound()
    {
        Fail("We couldn't find that event in your account.");
        return RedirectToAction(nameof(Events));
    }

    public async Task<IActionResult> Dashboard() => View(await MyEvents());

    public async Task<IActionResult> Events() => View(await MyEvents());

    // ---------- Events ----------

    [HttpGet]
    public async Task<IActionResult> EditEvent(int id = 0)
    {
        if (id == 0) return View(new RaceEvent { EventDate = DateTime.Today.AddDays(14) });
        var ev = await MyEvent(id);
        return ev == null ? EventNotFound() : View(ev);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEvent(RaceEvent model, IFormFile? banner)
    {
        var isNew = model.EventId == 0;
        if (string.IsNullOrWhiteSpace(model.EventName))
            ModelState.AddModelError(nameof(model.EventName), "Enter an event name.");
        if (string.IsNullOrWhiteSpace(model.Location))
            ModelState.AddModelError(nameof(model.Location), "Enter a location.");
        if (model.DistanceKm < 0.1m || model.DistanceKm > 1000m)
            ModelState.AddModelError(nameof(model.DistanceKm), "Distance must be between 0.1 and 1000 km.");
        if (!EventTypes.Contains(model.EventType))
            ModelState.AddModelError(nameof(model.EventType), "Choose Road, Trail or Track.");
        if (isNew && model.EventDate.Date < DateTime.Today)
            ModelState.AddModelError(nameof(model.EventDate), "A new event needs a date of today or later.");
        if (!ModelState.IsValid) return View(model);

        var (ok, data) = await api.Send(isNew ? "POST" : "PUT", isNew ? "api/events" : $"api/events/{model.EventId}", model);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, ApiClient.Message(data, "The event could not be saved. Please try again."));
            return View(model);
        }

        var id = isNew ? data.GetProperty("eventId").GetInt32() : model.EventId;
        if (banner != null && banner.Length > 0)
        {
            var (uploaded, uploadMsg) = await api.Upload($"api/uploads/events/{id}/banner", banner);
            if (!uploaded)
            {
                Fail("Event saved, but the banner was not uploaded: " + uploadMsg);
                return RedirectToAction(nameof(EditEvent), new { id });
            }
        }
        Notice("Event saved.");
        return RedirectToAction(nameof(Events));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var (ok, data) = await api.Send("DELETE", $"api/events/{id}");
        if (ok) Notice("Event deleted.");
        else Fail(ApiClient.Message(data, "We couldn't delete that event. Remove its categories and enrolments first, then try again."));
        return RedirectToAction(nameof(Events));
    }

    // ---------- Categories ----------

    public async Task<IActionResult> Categories(int id, int? editId)
    {
        var ev = await MyEvent(id);
        if (ev == null) return EventNotFound();

        var categories = await api.Get<List<RaceCategory>>($"api/events/{id}/categories") ?? new();
        var form = editId.HasValue
            ? categories.FirstOrDefault(c => c.CategoryId == editId.Value) ?? new RaceCategory { EventId = id }
            : new RaceCategory { EventId = id };
        return View(new CategoriesPage { Event = ev, Categories = categories, Form = form });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCategory(RaceCategory category)
    {
        if (string.IsNullOrWhiteSpace(category.Name) || category.Name.Length > 50)
            Fail("Enter a category name of up to 50 characters.");
        else if (category.MinAge < 0 || category.MaxAge < category.MinAge || category.MaxAge > 120)
            Fail("Ages must be between 0 and 120, and the maximum can't be lower than the minimum.");
        else if (category.Description?.Length > 255)
            Fail("Keep the description under 255 characters.");
        else
        {
            var isNew = category.CategoryId == 0;
            var (ok, data) = await api.Send(isNew ? "POST" : "PUT", isNew ? "api/categories" : $"api/categories/{category.CategoryId}", category);
            if (ok) Notice(isNew ? "Category added." : "Category updated.");
            else Fail(ApiClient.Message(data, "We couldn't save that category. Please try again."));
        }
        return RedirectToAction(nameof(Categories), new { id = category.EventId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id, int eventId)
    {
        var (ok, data) = await api.Send("DELETE", $"api/categories/{id}");
        if (ok) Notice("Category deleted.");
        else Fail(ApiClient.Message(data, "We couldn't delete that category. It may already have enrolments."));
        return RedirectToAction(nameof(Categories), new { id = eventId });
    }

    // ---------- Enrolments ----------

    public async Task<IActionResult> Enrolments(int id)
    {
        var ev = await MyEvent(id);
        if (ev == null) return EventNotFound();
        var list = await api.Get<List<RaceEnrolment>>($"api/organiser/events/{id}/enrolments") ?? new();
        return View(new EnrolmentsPage { Event = ev, Enrolments = list.OrderBy(e => e.ParticipantName).ToList() });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateEnrolmentStatus(int eventId, int enrolmentId, string status)
    {
        var (ok, data) = await api.Send("PUT", $"api/organiser/enrolments/{enrolmentId}/status", new { status });
        if (ok) Notice($"Enrolment marked {status}.");
        else Fail(ApiClient.Message(data, "We couldn't update that enrolment. Please try again."));
        return RedirectToAction(nameof(Enrolments), new { id = eventId });
    }

    // ---------- Results ----------

    public async Task<IActionResult> Results(int id)
    {
        var ev = await MyEvent(id);
        if (ev == null) return EventNotFound();
        var list = await api.Get<List<RaceEnrolment>>($"api/organiser/events/{id}/enrolments") ?? new();
        return View(new ResultsPage
        {
            Event = ev,
            Enrolments = list.OrderBy(e => e.Position ?? int.MaxValue).ThenBy(e => e.ParticipantName).ToList(),
            Open = ev.EventDate.Date <= DateTime.Today
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveResult(int eventId, int enrolmentId, int? resultId, int position, string? time)
    {
        var seconds = RaceFormat.ParseTime(time);
        if (position < 1 || seconds is null)
        {
            Fail("Enter a finishing position of 1 or more and a time like 1:23:45 (hours:minutes:seconds).");
            return RedirectToAction(nameof(Results), new { id = eventId });
        }

        var body = new { enrolmentId, position, timeSeconds = seconds, points = 0 };
        var (ok, data) = await api.Send(resultId.HasValue ? "PUT" : "POST",
            resultId.HasValue ? $"api/results/{resultId}" : "api/results", body);

        if (ok) Notice("Result published.");
        else Fail(ApiClient.Message(data, "We couldn't publish that result. Please try again."));
        return RedirectToAction(nameof(Results), new { id = eventId });
    }
}
