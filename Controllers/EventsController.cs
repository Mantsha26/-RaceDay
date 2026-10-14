using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Attributes;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var events = await _context.Events.Include(e => e.Organiser).ToListAsync();
            return Ok(events);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ev = await _context.Events.Include(e => e.Categories).FirstOrDefaultAsync(e => e.EventId == id);
            if (ev == null) return NotFound();
            return Ok(ev);
        }

        [HttpPost]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> Create([FromBody] CreateEventDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserId == userId);
            if (organiser == null) return StatusCode(403, new { message = "Organiser profile missing." });

            var ev = new Event
            {
                OrganiserId = organiser.OrganiserId,
                EventName = dto.EventName,
                EventDate = dto.EventDate,
                Location = dto.Location,
                Description = dto.Description,
                EventImageUrl = dto.EventImageUrl
            };

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = ev.EventId }, ev);
        }

        [HttpPut("{id}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateEventDto dto)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            ev.EventName = dto.EventName;
            ev.EventDate = dto.EventDate;
            ev.Location = dto.Location;
            ev.Description = dto.Description;
            ev.EventImageUrl = dto.EventImageUrl;

            await _context.SaveChangesAsync();
            return Ok(ev);
        }

        [HttpDelete("{id}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{eventId}/categories")]
        public async Task<IActionResult> GetCategories(int eventId)
        {
            if (!await _context.Events.AnyAsync(e => e.EventId == eventId))
                return NotFound(new { message = "Event not found." });

            var categories = await _context.Categories.Where(c => c.EventId == eventId).ToListAsync();
            return Ok(categories);
        }

        [HttpGet("{eventId}/results")]
        public async Task<IActionResult> GetResults(int eventId)
        {
            var results = await _context.Results
                .Include(r => r.Enrolment)
                .ThenInclude(e => e!.Category)
                .Where(r => r.Enrolment!.Category!.EventId == eventId)
                .ToListAsync();

            return Ok(results);
        }
    }
}
