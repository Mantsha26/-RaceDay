using Microsoft.AspNetCore.Mvc;
using RaceDayAPI.Attributes;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/results")]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> Create([FromBody] CreateResultDto dto)
        {
            var result = new Result
            {
                EnrolmentId = dto.EnrolmentId,
                Position = dto.Position,
                TimeSeconds = dto.TimeSeconds,
                Points = dto.Points
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();
            return StatusCode(201, result);
        }

        [HttpPut("{id}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateResultDto dto)
        {
            var result = await _context.Results.FindAsync(id);
            if (result == null) return NotFound();

            result.Position = dto.Position;
            result.TimeSeconds = dto.TimeSeconds;
            result.Points = dto.Points;

            await _context.SaveChangesAsync();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _context.Results.FindAsync(id);
            if (result == null) return NotFound();

            _context.Results.Remove(result);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
