using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using taskplan_calendar.Data;
using taskplan_calendar.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using taskplan_calendar.ViewModel;

namespace taskplan_calendar.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);
            var today = DateTime.UtcNow.Date;
            var todos = _db.Todos.Where(t => t.UserId == userId && (t.DueDate == null || t.DueDate.Value.Date == today)).ToList();
            return View(todos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddTodo(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return RedirectToAction("Index");

            var userId = _userManager.GetUserId(User);
            var todo = new Todo { Title = title, UserId = userId, DueDate = DateTime.UtcNow.Date };
            _db.Todos.Add(todo);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Display the calendar page with monthly view by default.
        /// Data is loaded asynchronously via API calls from JavaScript.
        /// </summary>
        public IActionResult Calendar(string view = "monthly")
        {
            if (!new[] { "daily", "weekly", "monthly" }.Contains(view))
                view = "monthly";

            var model = new CalendarViewViewModel
            {
                ViewType = view,
                ViewDate = DateTime.UtcNow,
                Events = new System.Collections.Generic.List<CalendarEventViewModel>()
            };

            return View("~/Views/Calendar/Index.cshtml", model);
        }
    }
}