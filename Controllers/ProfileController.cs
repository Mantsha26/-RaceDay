using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Attributes;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [SessionAuthorize]
    public class ProfileController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ProfileController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var user = await _context.Users
                .Include(u => u.ParticipantProfile)
                .Include(u => u.Organiser)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            if (!string.IsNullOrEmpty(dto.FirstName)) user.FirstName = dto.FirstName;
            if (!string.IsNullOrEmpty(dto.LastName)) user.LastName = dto.LastName;
            if (!string.IsNullOrEmpty(dto.Phone)) user.Phone = dto.Phone;

            if (user.Role.Equals("Participant", StringComparison.OrdinalIgnoreCase))
            {
                var profile = await _context.ParticipantProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
                if (profile != null)
                {
                    if (!string.IsNullOrEmpty(dto.Address)) profile.Address = dto.Address;
                    if (!string.IsNullOrEmpty(dto.Gender)) profile.Gender = dto.Gender;
                    if (!string.IsNullOrEmpty(dto.EmergencyContact)) profile.EmergencyContact = dto.EmergencyContact;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Profile updated successfully." });
        }

        [HttpGet("enrolments")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var profile = await _context.ParticipantProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return NotFound(new { message = "Participant profile not found." });

            var enrolments = await _context.Enrolments
                .Include(e => e.Category)
                .ThenInclude(c => c!.Event)
                .Where(e => e.ProfileId == profile.ProfileId)
                .ToListAsync();

            return Ok(enrolments);
        }
    }
}
