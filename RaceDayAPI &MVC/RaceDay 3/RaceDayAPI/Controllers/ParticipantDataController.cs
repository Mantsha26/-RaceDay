using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Attributes;
using RaceDayAPI.Data;
namespace RaceDayAPI.Controllers;
[ApiController,Route("api/participant"),SessionAuthorize("Participant")]
public class ParticipantDataController(RaceDayDbContext db):ControllerBase{
[HttpGet("history")]public async Task<IActionResult> History(){var id=HttpContext.Session.GetInt32("UserId");return Ok(await db.Enrolments.Where(en=>en.Profile!.UserId==id).Select(en=>new{en.EnrolmentId,en.Status,CategoryName=en.Category!.Name,EventId=en.Category.EventId,EventName=en.Category.Event!.EventName,EventDate=en.Category.Event.EventDate,Position=en.Result!=null?en.Result.Position:null,TimeSeconds=en.Result!=null?en.Result.TimeSeconds:null,Finishers=db.Results.Count(r=>r.Enrolment!.Category!.EventId==en.Category.EventId&&r.Position!=null)}).ToListAsync());}
}