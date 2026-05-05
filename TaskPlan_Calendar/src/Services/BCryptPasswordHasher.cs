using Microsoft.AspNetCore.Identity;
using taskplan_calendar.Models;

namespace taskplan_calendar.Services
{
    // BCrypt-based password hasher implementing ASP.NET Core Identity contract.
    // Uses BCrypt.Net (BCrypt.Net-Next package) for hashing and verification.
    public class BCryptPasswordHasher : IPasswordHasher<ApplicationUser>
    {
        public string HashPassword(ApplicationUser user, string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public PasswordVerificationResult VerifyHashedPassword(ApplicationUser user, string hashedPassword, string providedPassword)
        {
            if (BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword))
                return PasswordVerificationResult.Success;
            return PasswordVerificationResult.Failed;
        }
    }
}
