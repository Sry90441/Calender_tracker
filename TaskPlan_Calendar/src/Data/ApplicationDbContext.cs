using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using taskplan_calendar.Models;

namespace taskplan_calendar.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Todo> Todos { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<TodoList> TodoLists { get; set; }
    }
}
