
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    [Table("Enrolments")]
    public class Enrolment
    {
        [Key]
        [Column("enrolment_id")]
        public int EnrolmentId { get; set; }

        [Column("profile_id")]
        public int ProfileId { get; set; }

        [Column("category_id")]
        public int CategoryId { get; set; }

        [Column("enrolment_date")]
        public DateTime EnrolmentDate { get; set; }
            = DateTime.UtcNow;

        [MaxLength(20)]
        [Column("status")]
        public string Status { get; set; } = "Pending";

        // Participant profile relationship
        [ForeignKey(nameof(ProfileId))]
        public ParticipantProfile? Profile { get; set; }

        // Category relationship
        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        // Result relationship
        public Result? Result { get; set; }
    }
}

