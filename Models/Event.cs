using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    [Table("Events")]
    public class Event
    {
        [Key]
        [Column("event_id")]
        public int EventId { get; set; }

        [Column("organiser_id")]
        public int OrganiserId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("event_name")]
        public string EventName { get; set; } = string.Empty;

        [Column("event_date")]
        public DateTime EventDate { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("location")]
        public string Location { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("description")]
        public string? Description { get; set; }

        [MaxLength(255)]
        [Column("event_image_url")]
        public string? EventImageUrl { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("OrganiserId")]
        public Organiser? Organiser { get; set; }
        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}
