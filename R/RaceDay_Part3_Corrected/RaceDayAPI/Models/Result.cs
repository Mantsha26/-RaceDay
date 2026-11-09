using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    [Table("Results")]
    public class Result
    {
        [Key]
        [Column("result_id")]
        public int ResultId { get; set; }

        [Column("enrolment_id")]
        public int EnrolmentId { get; set; }

        [Column("position")]
        public int? Position { get; set; }

        [Column("time_seconds", TypeName = "decimal(8,2)")]
        public decimal? TimeSeconds { get; set; }

        [Column("points")]
        public int Points { get; set; } = 0;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("EnrolmentId")]
        public Enrolment? Enrolment { get; set; }
    }
}
