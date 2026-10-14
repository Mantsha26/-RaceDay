using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    [Table("ParticipantProfiles")]
    public class ParticipantProfile
    {
        [Key]
        [Column("profile_id")]
        public int ProfileId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("date_of_birth")]
        public DateTime DateOfBirth { get; set; }

        [MaxLength(20)]
        [Column("gender")]
        public string? Gender { get; set; }

        [MaxLength(150)]
        [Column("address")]
        public string? Address { get; set; }

        [MaxLength(100)]
        [Column("emergency_contact")]
        public string? EmergencyContact { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public User? User { get; set; }
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}