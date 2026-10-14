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

            var exists = await _context.Enrolments.AnyAsync(e => e.ProfileId == profile.ProfileId && e.CategoryId == dto.CategoryId);
            if (exists) return Conflict(new { message = "Already enrolled in this category." });

            var enrolment = new Enrolment
            {
                ProfileId = profile.ProfileId,
                CategoryId = dto.CategoryId
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();
            return StatusCode(201, enrolment);
        }

        [HttpDelete("{id}")]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> Cancel(int id)
        {
            var enrolment = await _context.Enrolments.FindAsync(id);
            if (enrolment == null) return NotFound();

            _context.Enrolments.Remove(enrolment);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}