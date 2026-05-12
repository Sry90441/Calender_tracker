using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using taskplan_calendar.Models;

namespace taskplan_calendar.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Todo> Todos { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CalendarEvent> CalendarEvents { get; set; }
        public DbSet<CalendarEventOccurrence> CalendarEventOccurrences { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure CalendarEvent relationships
            modelBuilder.Entity<CalendarEvent>()
                .HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure CalendarEventOccurrence relationships
            modelBuilder.Entity<CalendarEventOccurrence>()
                .HasOne(o => o.CalendarEvent)
                .WithMany()
                .HasForeignKey(o => o.CalendarEventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index for performance optimization
            modelBuilder.Entity<CalendarEvent>()
                .HasIndex(e => new { e.UserId, e.StartDateTime })
                .HasName("IX_CalendarEvents_UserId_StartDateTime");

            modelBuilder.Entity<CalendarEventOccurrence>()
                .HasIndex(o => new { o.CalendarEventId, o.OccurrenceDate })
                .HasName("IX_CalendarEventOccurrences_EventId_Date");
        }
    }
}
