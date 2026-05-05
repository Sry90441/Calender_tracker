using System;
using System.ComponentModel.DataAnnotations;

namespace taskplan_calendar.Models
{
    public class Todo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime? DueDate { get; set; }

        public bool IsDone { get; set; }

        public int? CategoryId { get; set; }
        public Category Category { get; set; }

        public string UserId { get; set; }
    }
}
