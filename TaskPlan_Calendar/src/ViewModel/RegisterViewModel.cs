using System.ComponentModel.DataAnnotations;

namespace taskplan_calendar.ViewModel
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(7, ErrorMessage = "Password must be at least 7 characters long.")]
        [RegularExpression("^(?=.*\\d)(?=.*\\W).+$", ErrorMessage = "Password must contain at least one digit and one special character.")]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
    }
}
