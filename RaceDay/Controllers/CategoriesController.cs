using Microsoft.AspNetCore.Mvc;
using RaceDayAPI.Attributes;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            var category = new Category
            {
                EventId = dto.EventId,
                Name = dto.Name,
                Description = dto.Description,
                MinAge = dto.MinAge,
                MaxAge = dto.MaxAge
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return StatusCode(201, category);
        }

        [HttpPut("{id}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateCategoryDto dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            category.Name = dto.Name;
            category.Description = dto.Description;
            category.MinAge = dto.MinAge;
            category.MaxAge = dto.MaxAge;

            await _context.SaveChangesAsync();
            return Ok(category);
        }

        [HttpDelete("{id}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
