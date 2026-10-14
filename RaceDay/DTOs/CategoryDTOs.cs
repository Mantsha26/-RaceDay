using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs
{
    public class CreateCategoryDto
    {
        [Required] public int EventId { get; set; }
        [Required] public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required] public int MinAge { get; set; }
        [Required] public int MaxAge { get; set; }
    }
}
