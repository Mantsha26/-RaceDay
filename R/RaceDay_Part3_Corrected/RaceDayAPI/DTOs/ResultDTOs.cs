using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs
{
    public class CreateResultDto
    {
        [Required] public int EnrolmentId { get; set; }
        public int? Position { get; set; }
        public decimal? TimeSeconds { get; set; }
        public int Points { get; set; }
    }
}