using System;
using System.Collections.Generic;
using taskplan_calendar.Models;

namespace taskplan_calendar.ViewModel
{
    public class CalendarViewModel
    {
        public string ViewType { get; set; } = "monthly";
        public DateTime ViewDate { get; set; } = DateTime.UtcNow;
        public List<AppointmentViewModel> Appointments { get; set; } = new List<AppointmentViewModel>();
    }

    public class AppointmentViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string CategoryName { get; set; }
        public string CategoryColor { get; set; }
        public bool IsRecurring { get; set; }
        public string RecurrencePattern { get; set; }
        public DateTime? RecurrenceEndDate { get; set; }
    }
}
