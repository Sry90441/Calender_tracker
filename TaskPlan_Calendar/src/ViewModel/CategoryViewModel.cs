using System;
using System.ComponentModel.DataAnnotations;

namespace taskplan_calendar.ViewModel
{
    /// <summary>
    /// ViewModel for displaying category information in views.
    /// Separates data presentation logic from model concerns.
    /// </summary>
    public class CategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, MinimumLength = 1, 
            ErrorMessage = "Category name must be between 1 and 100 characters")]
        [Display(Name = "Category Name")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Color is required")]
        [RegularExpression(@"^#[0-9A-F]{6}$", 
            ErrorMessage = "Color must be a valid hex code (e.g., #FF5733)")]
        [Display(Name = "Color")]
        public string ColorHex { get; set; } = "#007BFF";

        [StringLength(50, ErrorMessage = "Icon name cannot exceed 50 characters")]
        [Display(Name = "Icon Name")]
        public string IconName { get; set; } = "tag";

        [Display(Name = "Created")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Last Updated")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Number of todos/events currently using this category.
        /// Used for UI warnings before deletion.
        /// </summary>
        [Display(Name = "In Use")]
        public int UsageCount { get; set; }

        /// <summary>
        /// Helper property for displaying color in HTML.
        /// Returns inline style for use in templates.
        /// </summary>
        public string ColorStyle => $"background-color: {ColorHex}; color: white;";

        /// <summary>
        /// Helper property for Bootstrap badge styling.
        /// </summary>
        public string BadgeClass => $"badge badge-color" 
            + (IsLight() ? " text-dark" : " text-white");

        /// <summary>
        /// Determines if color is light (for text color contrast).
        /// </summary>
        private bool IsLight()
        {
            if (string.IsNullOrEmpty(ColorHex) || ColorHex.Length < 7)
                return false;

            var hex = ColorHex.Substring(1);
            int r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            int g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            int b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            int brightness = (r * 299 + g * 587 + b * 114) / 1000;
            return brightness > 155;
        }
    }

    /// <summary>
    /// ViewModel for listing all user categories.
    /// Used for display-only scenarios.
    /// </summary>
    public class CategoryListViewModel
    {
        public System.Collections.Generic.List<CategoryViewModel> Categories { get; set; }
            = new System.Collections.Generic.List<CategoryViewModel>();

        public int TotalCount { get; set; }
    }

    /// <summary>
    /// ViewModel for create/edit operations.
    /// Contains only editable fields to prevent mass assignment attacks.
    /// </summary>
    public class CreateEditCategoryViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, MinimumLength = 1,
            ErrorMessage = "Category name must be between 1 and 100 characters")]
        [Display(Name = "Category Name")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Color is required")]
        [RegularExpression(@"^#[0-9A-F]{6}$",
            ErrorMessage = "Color must be a valid hex code (e.g., #FF5733)")]
        [Display(Name = "Color (Hex)")]
        public string ColorHex { get; set; } = "#007BFF";

        [StringLength(50, ErrorMessage = "Icon name cannot exceed 50 characters")]
        [Display(Name = "Icon Name (Bootstrap icon)")]
        public string IconName { get; set; } = "tag";

        public string ActionLabel => Id.HasValue ? "Update Category" : "Create Category";
    }
}
