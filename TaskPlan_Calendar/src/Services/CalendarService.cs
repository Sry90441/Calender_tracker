using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using taskplan_calendar.Data;
using taskplan_calendar.Models;

namespace taskplan_calendar.Services
{
    /// <summary>
    /// Implementation of calendar event management service.
    /// Handles CRUD operations, authorization, and recurrence logic integration.
    /// </summary>
    public class CalendarService : ICalendarService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRecurrenceService _recurrenceService;

        public CalendarService(ApplicationDbContext context, IRecurrenceService recurrenceService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _recurrenceService = recurrenceService ?? throw new ArgumentNullException(nameof(recurrenceService));
        }

        public async Task<IEnumerable<CalendarEvent>> GetUserEventsAsync(string userId, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            if (startDate > endDate)
                throw new ArgumentException("Start date must be before end date");

            return await _context.CalendarEvents
                .Where(e => e.UserId == userId &&
                           e.StartDateTime.Date <= endDate.Date &&
                           e.EndDateTime.Date >= startDate.Date)
                .Include(e => e.Category)
                .OrderBy(e => e.StartDateTime)
                .ToListAsync();
        }

        public async Task<CalendarEvent> GetEventByIdAsync(int id, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            var calendarEvent = await _context.CalendarEvents
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (calendarEvent == null)
                throw new KeyNotFoundException($"Calendar event with ID {id} not found for the current user");

            return calendarEvent;
        }

        public async Task<CalendarEvent> CreateEventAsync(CalendarEvent calendarEvent, string userId)
        {
            if (calendarEvent == null)
                throw new ArgumentNullException(nameof(calendarEvent));

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            ValidateEventData(calendarEvent);

            calendarEvent.UserId = userId;
            calendarEvent.CreatedAt = DateTime.UtcNow;
            calendarEvent.UpdatedAt = DateTime.UtcNow;

            // Normalize all-day events
            if (calendarEvent.IsAllDay)
            {
                calendarEvent.StartDateTime = calendarEvent.StartDateTime.Date;
                calendarEvent.EndDateTime = calendarEvent.EndDateTime.Date.AddDays(1);
            }

            _context.CalendarEvents.Add(calendarEvent);
            await _context.SaveChangesAsync();

            // Generate initial occurrences if recurring
            if (calendarEvent.RecurrencePattern != RecurrencePattern.None)
            {
                await GenerateOccurrencesAsync(calendarEvent);
            }

            return calendarEvent;
        }

        public async Task<CalendarEvent> UpdateEventAsync(int id, CalendarEvent updatedEvent, string userId)
        {
            if (updatedEvent == null)
                throw new ArgumentNullException(nameof(updatedEvent));

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            ValidateEventData(updatedEvent);

            var existingEvent = await GetEventByIdAsync(id, userId);

            // Update properties
            existingEvent.Title = updatedEvent.Title;
            existingEvent.Description = updatedEvent.Description;
            existingEvent.StartDateTime = updatedEvent.StartDateTime;
            existingEvent.EndDateTime = updatedEvent.EndDateTime;
            existingEvent.IsAllDay = updatedEvent.IsAllDay;
            existingEvent.ColorHex = updatedEvent.ColorHex;
            existingEvent.CategoryId = updatedEvent.CategoryId;
            existingEvent.RecurrencePattern = updatedEvent.RecurrencePattern;
            existingEvent.RecurrenceEndDate = updatedEvent.RecurrenceEndDate;
            existingEvent.WeeklyRecurrenceDays = updatedEvent.WeeklyRecurrenceDays;
            existingEvent.MonthlyRecurrenceDay = updatedEvent.MonthlyRecurrenceDay;
            existingEvent.UpdatedAt = DateTime.UtcNow;

            // Normalize all-day events
            if (existingEvent.IsAllDay)
            {
                existingEvent.StartDateTime = existingEvent.StartDateTime.Date;
                existingEvent.EndDateTime = existingEvent.EndDateTime.Date.AddDays(1);
            }

            _context.CalendarEvents.Update(existingEvent);
            await _context.SaveChangesAsync();

            // Regenerate occurrences if recurrence pattern changed
            if (existingEvent.RecurrencePattern != RecurrencePattern.None)
            {
                // Delete old occurrences
                var oldOccurrences = await _context.CalendarEventOccurrences
                    .Where(o => o.CalendarEventId == id)
                    .ToListAsync();
                _context.CalendarEventOccurrences.RemoveRange(oldOccurrences);
                await _context.SaveChangesAsync();

                // Generate new occurrences
                await GenerateOccurrencesAsync(existingEvent);
            }

            return existingEvent;
        }

        public async Task<bool> DeleteEventAsync(int id, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            var calendarEvent = await GetEventByIdAsync(id, userId);

            // Cascade delete occurrences (handled by DbContext with DeleteBehavior.Cascade)
            _context.CalendarEvents.Remove(calendarEvent);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<CalendarEventOccurrence>> GetEventOccurrencesAsync(
            int eventId,
            DateTime startDate,
            DateTime endDate,
            string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            if (startDate > endDate)
                throw new ArgumentException("Start date must be before end date");

            // Verify user owns this event
            var calendarEvent = await GetEventByIdAsync(eventId, userId);

            if (calendarEvent.RecurrencePattern == RecurrencePattern.None)
                return Enumerable.Empty<CalendarEventOccurrence>();

            // Check database for existing occurrences
            var existingOccurrences = await _context.CalendarEventOccurrences
                .Where(o => o.CalendarEventId == eventId &&
                           o.OccurrenceDate >= startDate.Date &&
                           o.OccurrenceDate <= endDate.Date)
                .ToListAsync();

            return existingOccurrences;
        }

        public async Task<CalendarEventOccurrence> UpdateOccurrenceAsync(
            int occurrenceId,
            CalendarEventOccurrence updates,
            string userId)
        {
            if (updates == null)
                throw new ArgumentNullException(nameof(updates));

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            var occurrence = await _context.CalendarEventOccurrences
                .Include(o => o.CalendarEvent)
                .FirstOrDefaultAsync(o => o.Id == occurrenceId);

            if (occurrence == null)
                throw new KeyNotFoundException($"Occurrence with ID {occurrenceId} not found");

            // Verify user owns the parent event
            if (occurrence.CalendarEvent.UserId != userId)
                throw new UnauthorizedAccessException("You do not have permission to modify this occurrence");

            occurrence.ModifiedTitle = updates.ModifiedTitle;
            occurrence.ModifiedDescription = updates.ModifiedDescription;
            occurrence.ModifiedStartDateTime = updates.ModifiedStartDateTime;
            occurrence.ModifiedEndDateTime = updates.ModifiedEndDateTime;
            occurrence.IsModified = true;
            occurrence.UpdatedAt = DateTime.UtcNow;

            _context.CalendarEventOccurrences.Update(occurrence);
            await _context.SaveChangesAsync();

            return occurrence;
        }

        public async Task<bool> CancelOccurrenceAsync(int occurrenceId, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            var occurrence = await _context.CalendarEventOccurrences
                .Include(o => o.CalendarEvent)
                .FirstOrDefaultAsync(o => o.Id == occurrenceId);

            if (occurrence == null)
                throw new KeyNotFoundException($"Occurrence with ID {occurrenceId} not found");

            if (occurrence.CalendarEvent.UserId != userId)
                throw new UnauthorizedAccessException("You do not have permission to cancel this occurrence");

            occurrence.IsCanceled = true;
            occurrence.UpdatedAt = DateTime.UtcNow;

            _context.CalendarEventOccurrences.Update(occurrence);
            await _context.SaveChangesAsync();

            return true;
        }

        public string ValidateEventTimes(DateTime startDateTime, DateTime endDateTime)
        {
            if (startDateTime >= endDateTime)
                return "Event start time must be before end time";

            return null;
        }

        public async Task<int> GetUserEventCountAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            return await _context.CalendarEvents
                .Where(e => e.UserId == userId)
                .CountAsync();
        }

        public async Task<IEnumerable<CalendarEvent>> GetDayEventsAsync(string userId, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            var dayStart = date.Date;
            var dayEnd = dayStart.AddDays(1);

            return await _context.CalendarEvents
                .Where(e => e.UserId == userId &&
                           e.StartDateTime < dayEnd &&
                           e.EndDateTime > dayStart)
                .Include(e => e.Category)
                .OrderBy(e => e.StartDateTime)
                .ToListAsync();
        }

        /// <summary>
        /// Validates event data for required fields and business rules.
        /// </summary>
        private void ValidateEventData(CalendarEvent calendarEvent)
        {
            if (string.IsNullOrWhiteSpace(calendarEvent.Title))
                throw new ArgumentException("Event title is required", nameof(calendarEvent.Title));

            if (calendarEvent.Title.Length > 200)
                throw new ArgumentException("Title cannot exceed 200 characters", nameof(calendarEvent.Title));

            var timeValidation = ValidateEventTimes(calendarEvent.StartDateTime, calendarEvent.EndDateTime);
            if (timeValidation != null)
                throw new ArgumentException(timeValidation);

            if (!string.IsNullOrEmpty(calendarEvent.ColorHex) &&
                !System.Text.RegularExpressions.Regex.IsMatch(calendarEvent.ColorHex, @"^#[0-9A-F]{6}$",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                throw new ArgumentException("Color must be a valid hex code", nameof(calendarEvent.ColorHex));

            var recurrenceValidation = _recurrenceService.ValidateRecurrencePattern(
                calendarEvent.RecurrencePattern,
                calendarEvent.RecurrenceEndDate,
                calendarEvent.WeeklyRecurrenceDays,
                calendarEvent.MonthlyRecurrenceDay);

            if (recurrenceValidation != null)
                throw new ArgumentException(recurrenceValidation);
        }

        /// <summary>
        /// Generates and saves occurrences for a recurring event (2 years forward).
        /// </summary>
        private async Task GenerateOccurrencesAsync(CalendarEvent calendarEvent)
        {
            var startDate = calendarEvent.StartDateTime;
            var endDate = DateTime.UtcNow.AddYears(2);

            var occurrences = _recurrenceService.GenerateOccurrences(calendarEvent, startDate, endDate);

            foreach (var occurrence in occurrences)
            {
                _context.CalendarEventOccurrences.Add(occurrence);
            }

            await _context.SaveChangesAsync();
        }
    }
}
