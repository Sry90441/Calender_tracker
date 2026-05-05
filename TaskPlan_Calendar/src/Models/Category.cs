using System;
using System.ComponentModel.DataAnnotations;

namespace taskplan_calendar.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string UserId { get; set; }
    }
}
