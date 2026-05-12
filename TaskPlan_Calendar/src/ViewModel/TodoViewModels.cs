using System;
using System.ComponentModel.DataAnnotations;

namespace taskplan_calendar.ViewModel
{
    public class TodoListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ColorHex { get; set; }
    }

    public class TodoViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsDone { get; set; }
        public int Priority { get; set; }
        public string CategoryName { get; set; }
        public string CategoryColor { get; set; }
        public int? TodoListId { get; set; }
        public string TodoListName { get; set; }
        public bool IsRecurring { get; set; }
        public string RecurrencePattern { get; set; }
    }

    public class CreateEditTodoViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(255)]
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime? DueDate { get; set; }

        public bool IsDone { get; set; } = false;

        [Range(1, 4, ErrorMessage = "Priority must be between 1 and 4")]
        public int Priority { get; set; } = 2;

        public int? CategoryId { get; set; }

        public int? TodoListId { get; set; }

        public bool IsRecurring { get; set; } = false;

        [StringLength(50)]
        public string RecurrencePattern { get; set; }

        public DateTime? RecurrenceEndDate { get; set; }
    }
}
