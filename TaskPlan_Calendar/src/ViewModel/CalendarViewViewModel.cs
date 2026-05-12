using System;

namespace taskplan_calendar.ViewModel
{
    public class CalendarViewViewModel
    {
        public string ViewType { get; set; } = "monthly";
        public DateTime ViewDate { get; set; } = DateTime.UtcNow;
    }
}
