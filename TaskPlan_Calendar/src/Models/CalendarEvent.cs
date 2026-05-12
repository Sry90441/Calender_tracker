using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace taskplan_calendar.Models
{
    /// <summary>
    /// Enum for recurring event patterns.
    /// </summary>
    public enum RecurrencePattern
    {
        None = 0,
        Daily = 1,
        Weekly = 2,
        Monthly = 3,
        Yearly = 4
    }

    /// <summary>
    /// Represents a calendar event with optional recurrence.
    /// Events are linked to categories for organization and can be time-slotted or all-day.
    /// </summary>
    public class CalendarEvent
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Event title is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
        public string Title { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Start date/time is required")]
        public DateTime StartDateTime { get; set; }

        [Required(ErrorMessage = "End date/time is required")]
        public DateTime EndDateTime { get; set; }

        /// <summary>
        /// If true, event ignores time and spans entire days.
        /// StartDateTime and EndDateTime will be normalized to midnight.
        /// </summary>
        public bool IsAllDay { get; set; } = false;

        /// <summary>
        /// Color override for this specific event (optional, uses Category color if null).
        /// Format: #RRGGBB (e.g., #FF5733)
        /// </summary>
        [RegularExpression(@"^#[0-9A-F]{6}$", ErrorMessage = "Color must be a valid hex code (e.g., #FF5733)")]
        public string ColorHex { get; set; }

        /// <summary>
        /// Foreign key to Category for organization and theming.
        /// </summary>
        public int? CategoryId { get; set; }
        public Category Category { get; set; }

        public string UserId { get; set; }

        /// <summary>
        /// Indicates if this event repeats.
        /// </summary>
        public RecurrencePattern RecurrencePattern { get; set; } = RecurrencePattern.None;

        /// <summary>
        /// If RecurrencePattern is not None, this defines the recurrence end date.
        /// If null, event repeats indefinitely.
        /// </summary>
        public DateTime? RecurrenceEndDate { get; set; }

        /// <summary>
        /// Days of week for weekly recurrence (bitmask: 0=Sun, 1=Mon, ..., 6=Sat).
        /// Only used when RecurrencePattern == Weekly.
        /// </summary>
        public byte? WeeklyRecurrenceDays { get; set; }

        /// <summary>
        /// For monthly recurrence, the day of month (1-31).
        /// Only used when RecurrencePattern == Monthly.
        /// </summary>
        public int? MonthlyRecurrenceDay { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Represents a specific occurrence of a recurring event.
    /// Allows individual modifications to specific instances.
    /// </summary>
    public class CalendarEventOccurrence
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CalendarEventId { get; set; }

        [ForeignKey("CalendarEventId")]
        public CalendarEvent CalendarEvent { get; set; }

        /// <summary>
        /// The date this occurrence falls on.
        /// </summary>
        [Required]
        public DateTime OccurrenceDate { get; set; }

        /// <summary>
        /// If true, this occurrence has been modified from the parent event.
        /// </summary>
        public bool IsModified { get; set; } = false;

        /// <summary>
        /// Modified title for this occurrence (null = use parent event's title).
        /// </summary>
        public string ModifiedTitle { get; set; }

        /// <summary>
        /// Modified description for this occurrence (null = use parent event's description).
        /// </summary>
        public string ModifiedDescription { get; set; }

        /// <summary>
        /// Modified start time for this occurrence (null = use calculated time from parent).
        /// </summary>
        public DateTime? ModifiedStartDateTime { get; set; }

        /// <summary>
        /// Modified end time for this occurrence (null = use calculated time from parent).
        /// </summary>
        public DateTime? ModifiedEndDateTime { get; set; }

        /// <summary>
        /// If true, this occurrence is marked as canceled/skipped.
        /// </summary>
        public bool IsCanceled { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
