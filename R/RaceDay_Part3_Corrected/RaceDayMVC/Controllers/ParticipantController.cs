using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;
namespace RaceDayMVC.Controllers;

public class ParticipantController(ApiClient api) : RoleController("Participant")
{
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(int categoryId, int eventId)
    {
        var (ok, data) = await api.Send("POST", "api/enrolments", new { categoryId });
        if (ok) Notice("You're in! Your enrolment is recorded and shows as Pending until the organiser confirms it.");
        else Fail(ApiClient.Message(data, "We couldn't enrol you. Please try again."));
        return RedirectToAction("Details", "Home", new { id = eventId });
    }

    public async Task<IActionResult> MyEvents() =>
        View((await api.Get<List<RaceEnrolment>>("api/participant/history") ?? new())
            .OrderBy(e => e.EventDate).ToList());

    public async Task<IActionResult> MyResults() =>
        View((await api.Get<List<RaceEnrolment>>("api/participant/history") ?? new())
            .Where(e => e.Position != null)
            .OrderByDescending(e => e.EventDate).ToList());

    [HttpGet]
    public async Task<IActionResult> Profile() => View(await api.Get<ProfileForm>("api/profile") ?? new());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileForm form, IFormFile? picture)
    {
        if (string.IsNullOrWhiteSpace(form.FirstName) || string.IsNullOrWhiteSpace(form.LastName))
        {
            ModelState.AddModelError(string.Empty, "First and last name are required.");
            form.ProfilePictureUrl = (await api.Get<ProfileForm>("api/profile"))?.ProfilePictureUrl;
            return View(form);
        }

        var (ok, data) = await api.Send("PUT", "api/profile", form);
        if (!ok)
        {
            Fail(ApiClient.Message(data, "We couldn't save your profile. Please try again."));
            return RedirectToAction(nameof(Profile));
        }

        if (picture != null && picture.Length > 0)
        {
            var (uploaded, message) = await api.Upload("api/uploads/profile/picture", picture);
            if (!uploaded)
            {
                Fail("Profile saved, but the photo was not uploaded: " + message);
                return RedirectToAction(nameof(Profile));
            }
        }

        Notice("Profile saved.");
        return RedirectToAction(nameof(Profile));
    }
}
