using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Attributes;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers;

[ApiController]
[Route("api/results")]
public class ResultsController(RaceDayDbContext context) : ControllerBase
{
    [HttpPost]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Create([FromBody] CreateResultDto dto)
    {
        var userId = HttpContext.Session.GetInt32("UserId")!.Value;
        var enrolment = await context.Enrolments
            .Include(e => e.Category)
            .ThenInclude(c => c!.Event)
            .FirstOrDefaultAsync(e => e.EnrolmentId == dto.EnrolmentId);

        if (enrolment == null) return NotFound(new { message = "Enrolment not found." });
        if (enrolment.Category?.Event == null || enrolment.Category.Event.OrganiserId <= 0)
            return BadRequest(new { message = "The enrolment is not linked to a valid event." });

        var ownsEvent = await context.Organisers.AnyAsync(o =>
            o.UserId == userId && o.OrganiserId == enrolment.Category.Event.OrganiserId);
        if (!ownsEvent) return StatusCode(StatusCodes.Status403Forbidden,new{message="You do not have access to this resource."});
        if (enrolment.Category.Event.EventDate > DateTime.UtcNow)
            return Conflict(new { message = "Results can only be recorded after the event has finished." });
        if (await context.Results.AnyAsync(r => r.EnrolmentId == dto.EnrolmentId))
            return Conflict(new { message = "A result already exists for this enrolment. Update it instead." });

        var result = new Result
        {
            EnrolmentId = dto.EnrolmentId,
            Position = dto.Position,
            TimeSeconds = dto.TimeSeconds,
            Points = dto.Points
        };

        context.Results.Add(result);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = result.ResultId }, result);
    }

    [HttpGet("{id:int}")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await context.Results
            .Include(r => r.Enrolment).ThenInclude(e => e!.Category).ThenInclude(c => c!.Event)
            .FirstOrDefaultAsync(r => r.ResultId == id);
        if (result == null) return NotFound();
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!await context.Organisers.AnyAsync(o => o.UserId == userId && o.OrganiserId == result.Enrolment!.Category!.Event!.OrganiserId))
            return StatusCode(StatusCodes.Status403Forbidden,new{message="You do not have access to this resource."});
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateResultDto dto)
    {
        var result = await context.Results
            .Include(r => r.Enrolment).ThenInclude(e => e!.Category).ThenInclude(c => c!.Event)
            .FirstOrDefaultAsync(r => r.ResultId == id);
        if (result == null) return NotFound();

        var userId = HttpContext.Session.GetInt32("UserId");
        if (!await context.Organisers.AnyAsync(o =>
            o.UserId == userId && o.OrganiserId == result.Enrolment!.Category!.Event!.OrganiserId))
            return StatusCode(StatusCodes.Status403Forbidden,new{message="You do not have access to this resource."});

        result.Position = dto.Position;
        result.TimeSeconds = dto.TimeSeconds;
        result.Points = dto.Points;
        await context.SaveChangesAsync();
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [SessionAuthorize("Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await context.Results
            .Include(r => r.Enrolment).ThenInclude(e => e!.Category).ThenInclude(c => c!.Event)
            .FirstOrDefaultAsync(r => r.ResultId == id);
        if (result == null) return NotFound();

        var userId = HttpContext.Session.GetInt32("UserId");
        if (!await context.Organisers.AnyAsync(o =>
            o.UserId == userId && o.OrganiserId == result.Enrolment!.Category!.Event!.OrganiserId))
            return StatusCode(StatusCodes.Status403Forbidden,new{message="You do not have access to this resource."});

        context.Results.Remove(result);
        await context.SaveChangesAsync();
        return NoContent();
    }
}
