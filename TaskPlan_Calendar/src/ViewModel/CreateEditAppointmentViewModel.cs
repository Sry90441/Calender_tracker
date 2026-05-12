using System;
using System.ComponentModel.DataAnnotations;

namespace taskplan_calendar.ViewModel
{
    public class CreateEditAppointmentViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(255)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public int? CategoryId { get; set; }

        public bool IsRecurring { get; set; } = false;

        [StringLength(50)]
        public string RecurrencePattern { get; set; }

        public DateTime? RecurrenceEndDate { get; set; }

        [StringLength(100)]
        public string RecurrenceDaysOfWeek { get; set; }
    }
}
