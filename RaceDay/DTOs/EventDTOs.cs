using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs
{
    public class CreateEventDto
    {
        [Required] public string EventName { get; set; } = string.Empty;
        [Required] public DateTime EventDate { get; set; }
        [Required] public string Location { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? EventImageUrl { get; set; }
    }
}