using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs
{
    public class CreateEnrolmentDto
    {
        [Required] public int CategoryId { get; set; }
    }
}
