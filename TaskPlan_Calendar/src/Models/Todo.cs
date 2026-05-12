using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace taskplan_calendar.Models
{
    public class Todo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime? DueDate { get; set; }

        public bool IsDone { get; set; } = false;

        public int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Priority level: 1 (Low), 2 (Medium), 3 (High), 4 (Critical)
        /// </summary>
        public int Priority { get; set; } = 2;

        /// <summary>
        /// Optional list/group ID for organizing todos into lists
        /// </summary>
        public int? TodoListId { get; set; }

        [ForeignKey("TodoListId")]
        public virtual TodoList TodoList { get; set; }

        /// <summary>
        /// Indicates if this is a recurring todo
        /// </summary>
        public bool IsRecurring { get; set; } = false;

        /// <summary>
        /// Recurrence pattern: "daily", "weekly", "monthly", "yearly"
        /// </summary>
        public string RecurrencePattern { get; set; }

        /// <summary>
        /// Optional end date for recurring todos. NULL means forever.
        /// </summary>
        public DateTime? RecurrenceEndDate { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
