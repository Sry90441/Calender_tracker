using System;
using System.ComponentModel.DataAnnotations;

namespace taskplan_calendar.ViewModel
{
    /// <summary>
    /// ViewModel for displaying calendar event information.
    /// Read-only, used for API responses and views.
    /// </summary>
    public class CalendarEventViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public bool IsAllDay { get; set; }

        public string ColorHex { get; set; }

        public int? CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string RecurrencePattern { get; set; }

        public DateTime? RecurrenceEndDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Helper property for duration display.
        /// </summary>
        public TimeSpan Duration => EndDateTime - StartDateTime;

        /// <summary>
        /// Helper property for formatted date/time display.
        /// </summary>
        public string FormattedStartTime => IsAllDay 
            ? StartDateTime.ToString("MMM dd, yyyy") 
            : StartDateTime.ToString("MMM dd, yyyy h:mm tt");

        public string FormattedEndTime => IsAllDay
            ? EndDateTime.AddDays(-1).ToString("MMM dd, yyyy")
            : EndDateTime.ToString("MMM dd, yyyy h:mm tt");

        /// <summary>
        /// Indicates if this is a recurring event.
        /// </summary>
        public bool IsRecurring => !string.IsNullOrEmpty(RecurrencePattern) && RecurrencePattern != "None";
    }

    /// <summary>
    /// ViewModel for creating or editing calendar events.
    /// Contains only editable fields.
    /// </summary>
    public class CreateEditCalendarEventViewModel
    {
        [Required(ErrorMessage = "Event title is required")]
        [StringLength(200, MinimumLength = 1, 
            ErrorMessage = "Title must be between 1 and 200 characters")]
        public string Title { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Start date/time is required")]
        [DataType(DataType.DateTime)]
        public DateTime StartDateTime { get; set; }

        [Required(ErrorMessage = "End date/time is required")]
        [DataType(DataType.DateTime)]
        public DateTime EndDateTime { get; set; }

        [Display(Name = "All Day Event")]
        public bool IsAllDay { get; set; }

        [RegularExpression(@"^#[0-9A-F]{6}$", 
            ErrorMessage = "Color must be a valid hex code (e.g., #FF5733)")]
        [Display(Name = "Event Color (Optional)")]
        public string ColorHex { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Display(Name = "Recurrence Pattern")]
        public string RecurrencePattern { get; set; } = "None";

        [DataType(DataType.Date)]
        [Display(Name = "Recurrence End Date (Optional)")]
        public DateTime? RecurrenceEndDate { get; set; }

        [Display(Name = "Weekly Days (0-127 bitmask)")]
        public byte? WeeklyRecurrenceDays { get; set; }

        [Range(1, 31, ErrorMessage = "Day must be between 1 and 31")]
        [Display(Name = "Monthly Day")]
        public int? MonthlyRecurrenceDay { get; set; }

        public string ActionLabel => "Create Event";
    }

    /// <summary>
    /// ViewModel for displaying calendar event occurrences (instances of recurring events).
    /// </summary>
    public class CalendarEventOccurrenceViewModel
    {
        public int Id { get; set; }

        public int CalendarEventId { get; set; }

        public DateTime OccurrenceDate { get; set; }

        public bool IsModified { get; set; }

        public string ModifiedTitle { get; set; }

        public string ModifiedDescription { get; set; }

        public DateTime? ModifiedStartDateTime { get; set; }

        public DateTime? ModifiedEndDateTime { get; set; }

        public bool IsCanceled { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Status display (modified/canceled/normal).
        /// </summary>
        public string Status
        {
            get
            {
                if (IsCanceled) return "Canceled";
                if (IsModified) return "Modified";
                return "Normal";
            }
        }
    }

    /// <summary>
    /// ViewModel for updating a specific occurrence.
    /// Allows partial updates of individual instances.
    /// </summary>
    public class UpdateCalendarEventOccurrenceViewModel
    {
        [StringLength(200, MinimumLength = 1)]
        public string ModifiedTitle { get; set; }

        [StringLength(1000)]
        public string ModifiedDescription { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? ModifiedStartDateTime { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? ModifiedEndDateTime { get; set; }
    }

    /// <summary>
    /// ViewModel for calendar view data (daily, weekly, monthly).
    /// Aggregates events for UI rendering.
    /// </summary>
    public class CalendarViewViewModel
    {
        public string ViewType { get; set; } // "daily", "weekly", "monthly"

        public DateTime ViewDate { get; set; }

        public System.Collections.Generic.List<CalendarEventViewModel> Events { get; set; }
            = new System.Collections.Generic.List<CalendarEventViewModel>();

        public DateTime? PreviousViewDate { get; set; }

        public DateTime? NextViewDate { get; set; }

        public int TotalEventCount { get; set; }

        /// <summary>
        /// Gets the date range for current view.
        /// </summary>
        public (DateTime start, DateTime end) GetDateRange()
        {
            return ViewType switch
            {
                "daily" => (ViewDate.Date, ViewDate.Date.AddDays(1)),
                "weekly" => (GetWeekStart(ViewDate), GetWeekStart(ViewDate).AddDays(7)),
                "monthly" => (new DateTime(ViewDate.Year, ViewDate.Month, 1), 
                              new DateTime(ViewDate.Year, ViewDate.Month, 1).AddMonths(1)),
                _ => (ViewDate.Date, ViewDate.Date.AddDays(1))
            };
        }

        /// <summary>
        /// Gets the label for current view.
        /// </summary>
        public string GetViewLabel()
        {
            return ViewType switch
            {
                "daily" => ViewDate.ToString("dddd, MMMM dd, yyyy"),
                "weekly" => $"Week of {GetWeekStart(ViewDate):MMMM dd, yyyy}",
                "monthly" => ViewDate.ToString("MMMM yyyy"),
                _ => ViewDate.ToString("g")
            };
        }

        private DateTime GetWeekStart(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Sunday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }
    }

    /// <summary>
    /// ViewModel for quick event creation (from time slot selection).
    /// Minimal form for rapid entry.
    /// </summary>
    public class QuickCreateEventViewModel
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime SlotStartTime { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime SlotEndTime { get; set; }

        public int? CategoryId { get; set; }

        public string Description { get; set; }
    }
}
