using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace taskplan_calendar.Models
{
    /// <summary>
    /// Represents a list/collection of todos for organizational purposes.
    /// Users can organize their todos into multiple lists.
    /// </summary>
    public class TodoList
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Optional hex color for visual differentiation
        /// </summary>
        [StringLength(7)]
        public string ColorHex { get; set; } = "#007bff";

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<Todo> Todos { get; set; } = new List<Todo>();

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
