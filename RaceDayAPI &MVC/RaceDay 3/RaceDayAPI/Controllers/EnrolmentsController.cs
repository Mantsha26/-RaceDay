using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Attributes;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/enrolments")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> Enrol([FromBody] CreateEnrolmentDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var profile = await _context.ParticipantProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return StatusCode(403, new { message = "Participant profile required." });

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == dto.CategoryId);
            if (category?.Event == null)
                return NotFound(new { message = "Category not found." });
            if (category.Event.EventDate < DateTime.UtcNow.Date)
                return Conflict(new { message = "This event has already taken place." });

            var today = DateTime.UtcNow.Date;
            var age = today.Year - profile.DateOfBirth.Year;
            if (profile.DateOfBirth.Date > today.AddYears(-age)) age--;
            if (age < category.MinAge || age > category.MaxAge)
                return Conflict(new { message = $"This category is for ages {category.MinAge} to {category.MaxAge}. Choose a category that matches your age." });

            var exists = await _context.Enrolments.AnyAsync(e => e.ProfileId == profile.ProfileId && e.CategoryId == dto.CategoryId);
            if (exists) return Conflict(new { message = "Already enrolled in this category." });

            var alreadyInEvent = await _context.Enrolments.AnyAsync(e =>
                e.ProfileId == profile.ProfileId && e.Category!.EventId == category.EventId);
            if (alreadyInEvent) return Conflict(new { message = "You are already enrolled in this event. Cancel that enrolment to switch category." });

            var enrolment = new Enrolment
            {
                ProfileId = profile.ProfileId,
                CategoryId = dto.CategoryId
            };

            _context.Enrolments.Add(enrolment);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Unique (profile_id, category_id) index hit by a concurrent request.
                return Conflict(new { message = "Already enrolled in this category." });
            }
            return StatusCode(201, new { enrolment.EnrolmentId, enrolment.ProfileId, enrolment.CategoryId, enrolment.EnrolmentDate, enrolment.Status });
        }

        [HttpDelete("{id}")]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> Cancel(int id)
        {
            var enrolment = await _context.Enrolments.FindAsync(id);
            if (enrolment == null) return NotFound();
            if(!await _context.ParticipantProfiles.AnyAsync(p=>p.ProfileId==enrolment.ProfileId&&p.UserId==HttpContext.Session.GetInt32("UserId")))return StatusCode(StatusCodes.Status403Forbidden,new{message="You do not have access to this resource."});

            _context.Enrolments.Remove(enrolment);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}