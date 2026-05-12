using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using taskplan_calendar.Models;

namespace taskplan_calendar.Services
{
    /// <summary>
    /// Service interface for calendar event management.
    /// Handles CRUD operations, recurrence logic, and event occurrence generation.
    /// </summary>
    public interface ICalendarService
    {
        /// <summary>
        /// Retrieves calendar events for a user within a date range.
        /// Does NOT expand recurring events to occurrences.
        /// </summary>
        Task<IEnumerable<CalendarEvent>> GetUserEventsAsync(string userId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Retrieves a specific calendar event by ID (user must own it).
        /// </summary>
        Task<CalendarEvent> GetEventByIdAsync(int id, string userId);

        /// <summary>
        /// Creates a new calendar event for the user.
        /// If recurring, generates initial occurrences.
        /// </summary>
        Task<CalendarEvent> CreateEventAsync(CalendarEvent calendarEvent, string userId);

        /// <summary>
        /// Updates an existing calendar event.
        /// If recurrence settings changed, regenerates occurrences.
        /// </summary>
        Task<CalendarEvent> UpdateEventAsync(int id, CalendarEvent updatedEvent, string userId);

        /// <summary>
        /// Deletes a calendar event and all related occurrences.
        /// </summary>
        Task<bool> DeleteEventAsync(int id, string userId);

        /// <summary>
        /// Gets all occurrences (individual instances) of a recurring event within a date range.
        /// For non-recurring events, returns synthetic occurrence or empty.
        /// </summary>
        Task<IEnumerable<CalendarEventOccurrence>> GetEventOccurrencesAsync(
            int eventId, 
            DateTime startDate, 
            DateTime endDate, 
            string userId);

        /// <summary>
        /// Modifies a specific occurrence of a recurring event.
        /// Can change title, description, times, or mark as canceled.
        /// </summary>
        Task<CalendarEventOccurrence> UpdateOccurrenceAsync(
            int occurrenceId,
            CalendarEventOccurrence updates,
            string userId);

        /// <summary>
        /// Cancels a specific occurrence of a recurring event.
        /// </summary>
        Task<bool> CancelOccurrenceAsync(int occurrenceId, string userId);

        /// <summary>
        /// Validates if start time is before end time.
        /// Returns null if valid, error message if invalid.
        /// </summary>
        string ValidateEventTimes(DateTime startDateTime, DateTime endDateTime);

        /// <summary>
        /// Gets the total event count for a user (excluding occurrences).
        /// </summary>
        Task<int> GetUserEventCountAsync(string userId);

        /// <summary>
        /// Gets events for a specific day (all-day and timed events).
        /// </summary>
        Task<IEnumerable<CalendarEvent>> GetDayEventsAsync(string userId, DateTime date);
    }

    /// <summary>
    /// Service interface for handling recurring event logic.
    /// </summary>
    public interface IRecurrenceService
    {
        /// <summary>
        /// Generates occurrences for a recurring event within a date range.
        /// </summary>
        IEnumerable<CalendarEventOccurrence> GenerateOccurrences(
            CalendarEvent calendarEvent,
            DateTime startDate,
            DateTime endDate);

        /// <summary>
        /// Gets the next occurrence date after a given date.
        /// </summary>
        DateTime? GetNextOccurrenceDate(CalendarEvent calendarEvent, DateTime afterDate);

        /// <summary>
        /// Validates recurrence pattern and settings.
        /// Returns null if valid, error message if invalid.
        /// </summary>
        string ValidateRecurrencePattern(
            RecurrencePattern pattern,
            DateTime? endDate,
            byte? weeklyDays,
            int? monthlyDay);
    }
}
