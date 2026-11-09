using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Attributes;
using RaceDayAPI.Data;
using RaceDayAPI.Services;

namespace RaceDayAPI.Controllers;

[ApiController, Route("api/uploads"), SessionAuthorize]
public class UploadsController(RaceDayDbContext db, IBlobStorageService blobs) : ControllerBase
{
    private const long MaxBytes = 5_000_000;
    private static readonly string[] Types = ["image/jpeg", "image/png", "image/webp"];
    private static readonly string[] Extensions = [".jpg", ".jpeg", ".png", ".webp"];

    private static bool Valid(IFormFile? file) =>
        file is { Length: > 0 and <= MaxBytes }
        && Types.Contains(file.ContentType.ToLowerInvariant())
        && Extensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant());

    // The declared content type is client-controlled, so check the real file signature too.
    private static async Task<bool> LooksLikeImage(IFormFile file)
    {
        var h = new byte[12];
        await using var s = file.OpenReadStream();
        if (await s.ReadAsync(h.AsMemory(0, 12)) < 12) return false;
        bool jpeg = h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF;
        bool png = h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47;
        bool webp = h[0] == 'R' && h[1] == 'I' && h[2] == 'F' && h[3] == 'F' && h[8] == 'W' && h[9] == 'E' && h[10] == 'B' && h[11] == 'P';
        return jpeg || png || webp;
    }

    private async Task<(string? url, IActionResult? error)> Store(IFormFile file, string container)
    {
        try { return (await blobs.UploadFileAsync(file, container), null); }
        catch (InvalidOperationException ex)
        {
            return (null, StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message }));
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            return (null, StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { message = "The AzureStorage:ConnectionString value is not a valid Azure Storage connection string." }));
        }
        catch (RequestFailedException ex)
        {
            return (null, StatusCode(StatusCodes.Status502BadGateway,
                new { message = $"Azure Blob Storage rejected the upload ({ex.ErrorCode ?? ex.Status.ToString()})." }));
        }
    }

    [HttpPost("events/{id:int}/banner"), SessionAuthorize("Organiser")]
    [RequestSizeLimit(6_000_000)]
    public async Task<IActionResult> Banner(int id, IFormFile file)
    {
        if (!Valid(file) || !await LooksLikeImage(file))
            return BadRequest(new { message = "Use a JPG, PNG or WebP image under 5 MB." });

        var ev = await db.Events.FindAsync(id);
        if (ev == null) return NotFound(new { message = "Event not found." });

        var userId = HttpContext.Session.GetInt32("UserId");
        if (!await db.Organisers.AnyAsync(o => o.OrganiserId == ev.OrganiserId && o.UserId == userId))
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not own this event." });

        var (url, error) = await Store(file, "event-banners");
        if (error != null) return error;

        var oldUrl = ev.EventImageUrl;
        ev.EventImageUrl = url;
        await db.SaveChangesAsync();
        await blobs.DeleteAsync(oldUrl);
        return Ok(new { url });
    }

    [HttpPost("profile/picture"), SessionAuthorize("Participant")]
    [RequestSizeLimit(6_000_000)]
    public async Task<IActionResult> Picture(IFormFile file)
    {
        if (!Valid(file) || !await LooksLikeImage(file))
            return BadRequest(new { message = "Use a JPG, PNG or WebP image under 5 MB." });

        var userId = HttpContext.Session.GetInt32("UserId");
        var profile = await db.ParticipantProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null) return NotFound(new { message = "Participant profile not found." });

        var (url, error) = await Store(file, "profile-pictures");
        if (error != null) return error;

        var oldUrl = profile.ProfilePictureUrl;
        profile.ProfilePictureUrl = url;
        await db.SaveChangesAsync();
        await blobs.DeleteAsync(oldUrl);
        return Ok(new { url });
    }
}
