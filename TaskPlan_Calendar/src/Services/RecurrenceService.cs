using System;
using System.Collections.Generic;
using System.Linq;
using taskplan_calendar.Models;

namespace taskplan_calendar.Services
{
    /// <summary>
    /// Implementation of recurrence pattern logic for calendar events.
    /// Handles generation and validation of recurring event occurrences.
    /// </summary>
    public class RecurrenceService : IRecurrenceService
    {
        public IEnumerable<CalendarEventOccurrence> GenerateOccurrences(
            CalendarEvent calendarEvent,
            DateTime startDate,
            DateTime endDate)
        {
            if (calendarEvent == null)
                throw new ArgumentNullException(nameof(calendarEvent));

            if (calendarEvent.RecurrencePattern == RecurrencePattern.None)
                return Enumerable.Empty<CalendarEventOccurrence>();

            if (startDate > endDate)
                throw new ArgumentException("Start date must be before end date");

            var occurrences = new List<CalendarEventOccurrence>();
            var currentDate = calendarEvent.StartDateTime.Date;

            // Determine recurrence end boundary
            var recurrenceEnd = calendarEvent.RecurrenceEndDate?.Date ?? endDate.Date;
            recurrenceEnd = recurrenceEnd < endDate.Date ? recurrenceEnd : endDate.Date;

            while (currentDate <= recurrenceEnd)
            {
                if (ShouldCreateOccurrence(calendarEvent, currentDate))
                {
                    if (currentDate >= startDate.Date && currentDate <= endDate.Date)
                    {
                        occurrences.Add(new CalendarEventOccurrence
                        {
                            CalendarEventId = calendarEvent.Id,
                            OccurrenceDate = currentDate,
                            IsModified = false,
                            IsCanceled = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                }

                // Move to next potential occurrence date
                currentDate = GetNextPotentialDate(calendarEvent, currentDate);

                // Safety check: prevent infinite loops
                if (occurrences.Count > 10000)
                    break;
            }

            return occurrences;
        }

        public DateTime? GetNextOccurrenceDate(CalendarEvent calendarEvent, DateTime afterDate)
        {
            if (calendarEvent == null)
                throw new ArgumentNullException(nameof(calendarEvent));

            if (calendarEvent.RecurrencePattern == RecurrencePattern.None)
                return null;

            var currentDate = afterDate.Date.AddDays(1);
            var recurrenceEnd = calendarEvent.RecurrenceEndDate?.Date ?? DateTime.MaxValue.Date;

            while (currentDate <= recurrenceEnd && currentDate <= DateTime.UtcNow.Date.AddYears(10))
            {
                if (ShouldCreateOccurrence(calendarEvent, currentDate))
                    return currentDate;

                currentDate = GetNextPotentialDate(calendarEvent, currentDate);
            }

            return null;
        }

        public string ValidateRecurrencePattern(
            RecurrencePattern pattern,
            DateTime? endDate,
            byte? weeklyDays,
            int? monthlyDay)
        {
            if (pattern == RecurrencePattern.None)
                return null;

            if (endDate.HasValue && endDate < DateTime.UtcNow)
                return "Recurrence end date must be in the future";

            if (pattern == RecurrencePattern.Weekly && (!weeklyDays.HasValue || weeklyDays == 0))
                return "Weekly recurrence requires at least one day selected";

            if (pattern == RecurrencePattern.Monthly)
            {
                if (!monthlyDay.HasValue || monthlyDay < 1 || monthlyDay > 31)
                    return "Monthly recurrence requires a valid day (1-31)";
            }

            return null;
        }

        /// <summary>
        /// Determines if an occurrence should be created for the given date.
        /// </summary>
        private bool ShouldCreateOccurrence(CalendarEvent calendarEvent, DateTime date)
        {
            return calendarEvent.RecurrencePattern switch
            {
                RecurrencePattern.Daily => true,
                RecurrencePattern.Weekly => IsWeeklyMatch(date, calendarEvent),
                RecurrencePattern.Monthly => IsMonthlyMatch(date, calendarEvent),
                RecurrencePattern.Yearly => IsYearlyMatch(date, calendarEvent),
                _ => false
            };
        }

        private bool IsWeeklyMatch(DateTime date, CalendarEvent calendarEvent)
        {
            if (!calendarEvent.WeeklyRecurrenceDays.HasValue)
                return false;

            // WeeklyRecurrenceDays is a bitmask: bit 0=Sun, 1=Mon, ..., 6=Sat
            int dayBit = (int)date.DayOfWeek;
            return ((calendarEvent.WeeklyRecurrenceDays >> dayBit) & 1) != 0;
        }

        private bool IsMonthlyMatch(DateTime date, CalendarEvent calendarEvent)
        {
            if (!calendarEvent.MonthlyRecurrenceDay.HasValue)
                return false;

            // Handle edge case: if requested day > days in month, don't create occurrence
            int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            if (calendarEvent.MonthlyRecurrenceDay > daysInMonth)
                return false;

            return date.Day == calendarEvent.MonthlyRecurrenceDay;
        }

        private bool IsYearlyMatch(DateTime date, CalendarEvent calendarEvent)
        {
            // Yearly: same month and day as original event
            return date.Month == calendarEvent.StartDateTime.Month &&
                   date.Day == calendarEvent.StartDateTime.Day;
        }

        /// <summary>
        /// Gets the next date to check for recurrence, based on pattern.
        /// </summary>
        private DateTime GetNextPotentialDate(CalendarEvent calendarEvent, DateTime currentDate)
        {
            return calendarEvent.RecurrencePattern switch
            {
                RecurrencePattern.Daily => currentDate.AddDays(1),
                RecurrencePattern.Weekly => currentDate.AddDays(1),
                RecurrencePattern.Monthly => currentDate.AddDays(1),
                RecurrencePattern.Yearly => currentDate.AddYears(1),
                _ => currentDate.AddDays(1)
            };
        }
    }
}
