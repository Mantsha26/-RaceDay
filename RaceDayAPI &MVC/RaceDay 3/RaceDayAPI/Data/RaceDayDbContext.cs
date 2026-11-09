using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Models;

namespace RaceDayAPI.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Organiser> Organisers { get; set; }
        public DbSet<ParticipantProfile> ParticipantProfiles { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Event>()
                .Property(e => e.DistanceKm)
                .HasColumnType("decimal(18,2)");

            // User has at most one organiser profile and one participant profile.
            modelBuilder.Entity<Organiser>()
                .HasOne(o => o.User)
                .WithOne(u => u.Organiser)
                .HasForeignKey<Organiser>(o => o.UserId);

            modelBuilder.Entity<ParticipantProfile>()
                .HasOne(p => p.User)
                .WithOne(u => u.ParticipantProfile)
                .HasForeignKey<ParticipantProfile>(p => p.UserId);

            // Organiser owns many events; each event belongs to one organiser.
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organiser)
                .WithMany(o => o.Events)
                .HasForeignKey(e => e.OrganiserId);

            // Event has many categories; category has many enrolments.
            modelBuilder.Entity<Category>()
                .HasOne(c => c.Event)
                .WithMany(e => e.Categories)
                .HasForeignKey(c => c.EventId);

            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Profile)
                .WithMany(p => p.Enrolments)
                .HasForeignKey(e => e.ProfileId)
                // Users -> ParticipantProfiles -> Enrolments and
                // Users -> Organisers -> Events -> Categories -> Enrolments would be two
                // cascade paths, which SQL Server rejects (error 1785). Only the
                // Category path cascades.
                .OnDelete(DeleteBehavior.Restrict);

            // A participant can enrol in a category only once.
            modelBuilder.Entity<Enrolment>()
                .HasIndex(e => new { e.ProfileId, e.CategoryId })
                .IsUnique();

            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Enrolments)
                .HasForeignKey(e => e.CategoryId);

            // One result per enrolment, matching the unique migration index.
            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Result)
                .WithOne(r => r.Enrolment)
                .HasForeignKey<Result>(r => r.EnrolmentId);
        }
    }
}
