using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace taskplan_calendar.Models
{
    /// <summary>
    /// Represents a calendar appointment/event for a user.
    /// Supports one-time and recurring appointments with optional end dates.
    /// </summary>
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Optional category/label for the appointment (FK to Category)
        /// </summary>
        public int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Indicates if this is a recurring appointment
        /// </summary>
        public bool IsRecurring { get; set; } = false;

        /// <summary>
        /// Recurrence pattern: "daily", "weekly", "monthly", "yearly"
        /// </summary>
        public string RecurrencePattern { get; set; }

        /// <summary>
        /// Optional end date for recurring appointments. NULL means forever.
        /// </summary>
        public DateTime? RecurrenceEndDate { get; set; }

        /// <summary>
        /// Days of week for weekly recurrence (comma-separated: 0-6, where 0 = Sunday)
        /// </summary>
        public string RecurrenceDaysOfWeek { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}
