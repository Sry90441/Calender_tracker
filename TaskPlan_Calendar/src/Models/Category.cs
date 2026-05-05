using System;
using System.ComponentModel.DataAnnotations;

namespace taskplan_calendar.Models
{
    /// <summary>
    /// Represents a category for organizing todos and calendar events.
    /// Categories provide visual organization via colors and icons, and can be shared across features.
    /// </summary>
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Category name must be between 1 and 100 characters")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        /// <summary>
        /// Hex color code for visual identification (e.g., "#FF5733")
        /// Default: "#007BFF" (Bootstrap primary blue)
        /// </summary>
        [RegularExpression(@"^#[0-9A-F]{6}$", ErrorMessage = "Color must be a valid hex code (e.g., #FF5733)")]
        public string ColorHex { get; set; } = "#007BFF";

        /// <summary>
        /// Bootstrap icon name for visual representation (e.g., "calendar", "check-circle", "folder")
        /// </summary>
        [StringLength(50, ErrorMessage = "Icon name cannot exceed 50 characters")]
        public string IconName { get; set; } = "tag";

        [Required]
        public string UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
