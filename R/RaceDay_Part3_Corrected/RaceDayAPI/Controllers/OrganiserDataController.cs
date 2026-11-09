using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Attributes;
using RaceDayAPI.Data;
namespace RaceDayAPI.Controllers;
[ApiController,Route("api/organiser"),SessionAuthorize("Organiser")]
public class OrganiserDataController(RaceDayDbContext db):ControllerBase{
[HttpGet("events")]public async Task<IActionResult> Events(){var id=HttpContext.Session.GetInt32("UserId");return Ok(await db.Events.Where(e=>e.Organiser!.UserId==id).Select(e=>new{e.EventId,e.EventName,e.EventDate,e.Location,e.Description,e.DistanceKm,e.EventType,e.EventImageUrl,EnrolmentCount=e.Categories.SelectMany(c=>c.Enrolments).Count()}).ToListAsync());}
[HttpGet("events/{id:int}/enrolments")]public async Task<IActionResult> Enrolments(int id){if(!await db.Events.AnyAsync(e=>e.EventId==id&&e.Organiser!.UserId==HttpContext.Session.GetInt32("UserId")))return StatusCode(StatusCodes.Status403Forbidden,new{message="You do not have access to this resource."});return Ok(await db.Enrolments.Where(en=>en.Category!.EventId==id).Select(en=>new{en.EnrolmentId,en.Status,CategoryName=en.Category!.Name,ParticipantName=en.Profile!.User!.FirstName+" "+en.Profile.User.LastName,en.EnrolmentDate,ResultId=en.Result!=null?en.Result.ResultId:(int?)null,Position=en.Result!=null?en.Result.Position:null,TimeSeconds=en.Result!=null?en.Result.TimeSeconds:null}).ToListAsync());}

public class EnrolmentStatusDto{public string Status{get;set;}=string.Empty;}

[HttpPut("enrolments/{id:int}/status")]
public async Task<IActionResult> SetStatus(int id,[FromBody] EnrolmentStatusDto dto)
{
    var allowed=new[]{"Pending","Confirmed"};
    var status=allowed.FirstOrDefault(s=>string.Equals(s,dto.Status?.Trim(),StringComparison.OrdinalIgnoreCase));
    if(status==null)return BadRequest(new{message="Status must be Pending or Confirmed."});
    var userId=HttpContext.Session.GetInt32("UserId");
    var enrolment=await db.Enrolments.Include(e=>e.Category).ThenInclude(c=>c!.Event).ThenInclude(ev=>ev!.Organiser).FirstOrDefaultAsync(e=>e.EnrolmentId==id);
    if(enrolment==null)return NotFound(new{message="Enrolment not found."});
    if(enrolment.Category?.Event?.Organiser?.UserId!=userId)return StatusCode(StatusCodes.Status403Forbidden,new{message="You do not have access to this resource."});
    enrolment.Status=status;
    await db.SaveChangesAsync();
    return Ok(new{enrolment.EnrolmentId,enrolment.Status});
}
}