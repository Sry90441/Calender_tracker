using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using taskplan_calendar.Data;
using taskplan_calendar.Models;
using taskplan_calendar.Models;
using taskplan_calendar.Services;
using taskplan_calendar.ViewModel;

namespace taskplan_calendar.Controllers
{
    /// <summary>
    /// Controller for managing calendar events.
    /// Handles CRUD operations and recurrence management.
    /// All actions require user to be authenticated.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        private readonly ICalendarService _calendarService;

        public CalendarController(ICalendarService calendarService)
        {
            _calendarService = calendarService ?? throw new ArgumentNullException(nameof(calendarService));
        }

        /// <summary>
        /// Gets the current authenticated user's ID.
        /// </summary>
        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("User ID not found in claims");
        }

        /// <summary>
        /// GET: api/calendar?startDate=2026-05-01&endDate=2026-05-31
        /// Retrieves calendar events for the user within a date range.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CalendarEventViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetUserEvents(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate == DateTime.MinValue || endDate == DateTime.MinValue)
                    return BadRequest(new { error = "Both startDate and endDate query parameters are required" });

                var userId = GetUserId();
                var events = await _calendarService.GetUserEventsAsync(userId, startDate, endDate);

                var viewModels = new List<CalendarEventViewModel>();
                foreach (var evt in events)
                {
                    viewModels.Add(MapToViewModel(evt));
                }

                return Ok(viewModels);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/calendar/{id}
        /// Retrieves a specific calendar event by ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CalendarEventViewModel), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetEvent(int id)
        {
            try
            {
                var userId = GetUserId();
                var calendarEvent = await _calendarService.GetEventByIdAsync(id, userId);

                return Ok(MapToViewModel(calendarEvent));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Calendar event not found" });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "You do not have permission to access this event" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/calendar
        /// Creates a new calendar event.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CalendarEventViewModel), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEditCalendarEventViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetUserId();
                
                // Parse RecurrencePattern from string
                if (!Enum.TryParse<RecurrencePattern>(model.RecurrencePattern, out var recurrencePattern))
                    recurrencePattern = RecurrencePattern.None;

                var calendarEvent = new CalendarEvent
                {
                    Title = model.Title,
                    Description = model.Description,
                    StartDateTime = model.StartDateTime,
                    EndDateTime = model.EndDateTime,
                    IsAllDay = model.IsAllDay,
                    ColorHex = model.ColorHex,
                    CategoryId = model.CategoryId,
                    RecurrencePattern = recurrencePattern,
                    RecurrenceEndDate = model.RecurrenceEndDate,
                    WeeklyRecurrenceDays = model.WeeklyRecurrenceDays,
                    MonthlyRecurrenceDay = model.MonthlyRecurrenceDay
                };

                var createdEvent = await _calendarService.CreateEventAsync(calendarEvent, userId);
                var viewModel = MapToViewModel(createdEvent);

                return CreatedAtAction(nameof(GetEvent), new { id = createdEvent.Id }, viewModel);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// PUT: api/calendar/{id}
        /// Updates an existing calendar event.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CalendarEventViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] CreateEditCalendarEventViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetUserId();
                
                // Parse RecurrencePattern from string
                if (!Enum.TryParse<RecurrencePattern>(model.RecurrencePattern, out var recurrencePattern))
                    recurrencePattern = RecurrencePattern.None;

                var updatedEvent = new CalendarEvent
                {
                    Title = model.Title,
                    Description = model.Description,
                    StartDateTime = model.StartDateTime,
                    EndDateTime = model.EndDateTime,
                    IsAllDay = model.IsAllDay,
                    ColorHex = model.ColorHex,
                    CategoryId = model.CategoryId,
                    RecurrencePattern = recurrencePattern,
                    RecurrenceEndDate = model.RecurrenceEndDate,
                    WeeklyRecurrenceDays = model.WeeklyRecurrenceDays,
                    MonthlyRecurrenceDay = model.MonthlyRecurrenceDay
                };

                var result = await _calendarService.UpdateEventAsync(id, updatedEvent, userId);
                var viewModel = MapToViewModel(result);

                return Ok(viewModel);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Calendar event not found" });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "You do not have permission to update this event" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// DELETE: api/calendar/{id}
        /// Deletes a calendar event and all its occurrences.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            try
            {
                var userId = GetUserId();
                await _calendarService.DeleteEventAsync(id, userId);

                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Calendar event not found" });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "You do not have permission to delete this event" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/calendar/{id}/occurrences?startDate=2026-05-01&endDate=2026-05-31
        /// Retrieves all occurrences of a recurring event within a date range.
        /// </summary>
        [HttpGet("{id}/occurrences")]
        [ProducesResponseType(typeof(IEnumerable<CalendarEventOccurrenceViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetEventOccurrences(
            int id,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate == DateTime.MinValue || endDate == DateTime.MinValue)
                    return BadRequest(new { error = "Both startDate and endDate query parameters are required" });

                var userId = GetUserId();
                var occurrences = await _calendarService.GetEventOccurrencesAsync(id, startDate, endDate, userId);

                var viewModels = new List<CalendarEventOccurrenceViewModel>();
                foreach (var occurrence in occurrences)
                {
                    viewModels.Add(MapOccurrenceToViewModel(occurrence));
                }

                return Ok(viewModels);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Calendar event not found" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// PUT: api/calendar/occurrence/{occurrenceId}
        /// Updates a specific occurrence of a recurring event.
        /// </summary>
        [HttpPut("occurrence/{occurrenceId}")]
        [ProducesResponseType(typeof(CalendarEventOccurrenceViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> UpdateOccurrence(
            int occurrenceId,
            [FromBody] UpdateCalendarEventOccurrenceViewModel model)
        {
            try
            {
                var userId = GetUserId();
                var updates = new CalendarEventOccurrence
                {
                    ModifiedTitle = model.ModifiedTitle,
                    ModifiedDescription = model.ModifiedDescription,
                    ModifiedStartDateTime = model.ModifiedStartDateTime,
                    ModifiedEndDateTime = model.ModifiedEndDateTime
                };

                var result = await _calendarService.UpdateOccurrenceAsync(occurrenceId, updates, userId);
                var viewModel = MapOccurrenceToViewModel(result);

                return Ok(viewModel);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Occurrence not found" });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "You do not have permission to modify this occurrence" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// DELETE: api/calendar/occurrence/{occurrenceId}/cancel
        /// Cancels a specific occurrence of a recurring event.
        /// </summary>
        [HttpDelete("occurrence/{occurrenceId}/cancel")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> CancelOccurrence(int occurrenceId)
        {
            try
            {
                var userId = GetUserId();
                await _calendarService.CancelOccurrenceAsync(occurrenceId, userId);

                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Occurrence not found" });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "You do not have permission to cancel this occurrence" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/calendar/day/{date}
        /// Retrieves all events for a specific day.
        /// Date format: YYYY-MM-DD (e.g., 2026-05-15)
        /// </summary>
        [HttpGet("day/{date}")]
        [ProducesResponseType(typeof(IEnumerable<CalendarEventViewModel>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetDayEvents(string date)
        {
            try
            {
                if (!DateTime.TryParse(date, out var parsedDate))
                    return BadRequest(new { error = "Invalid date format. Use YYYY-MM-DD" });

                var userId = GetUserId();
                var events = await _calendarService.GetDayEventsAsync(userId, parsedDate);

                var viewModels = new List<CalendarEventViewModel>();
                foreach (var evt in events)
                {
                    viewModels.Add(MapToViewModel(evt));
                }

                return Ok(viewModels);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private CalendarEventViewModel MapToViewModel(CalendarEvent calendarEvent)
        {
            return new CalendarEventViewModel
            {
                Id = calendarEvent.Id,
                Title = calendarEvent.Title,
                Description = calendarEvent.Description,
                StartDateTime = calendarEvent.StartDateTime,
                EndDateTime = calendarEvent.EndDateTime,
                IsAllDay = calendarEvent.IsAllDay,
                ColorHex = calendarEvent.ColorHex,
                CategoryId = calendarEvent.CategoryId,
                CategoryName = calendarEvent.Category?.Name,
                RecurrencePattern = calendarEvent.RecurrencePattern.ToString(),
                RecurrenceEndDate = calendarEvent.RecurrenceEndDate,
                CreatedAt = calendarEvent.CreatedAt,
                UpdatedAt = calendarEvent.UpdatedAt
            };
        }

        private CalendarEventOccurrenceViewModel MapOccurrenceToViewModel(CalendarEventOccurrence occurrence)
        {
            return new CalendarEventOccurrenceViewModel
            {
                Id = occurrence.Id,
                CalendarEventId = occurrence.CalendarEventId,
                OccurrenceDate = occurrence.OccurrenceDate,
                IsModified = occurrence.IsModified,
                ModifiedTitle = occurrence.ModifiedTitle,
                ModifiedDescription = occurrence.ModifiedDescription,
                ModifiedStartDateTime = occurrence.ModifiedStartDateTime,
                ModifiedEndDateTime = occurrence.ModifiedEndDateTime,
                IsCanceled = occurrence.IsCanceled,
                CreatedAt = occurrence.CreatedAt,
                UpdatedAt = occurrence.UpdatedAt
            };
        }
    }
}

