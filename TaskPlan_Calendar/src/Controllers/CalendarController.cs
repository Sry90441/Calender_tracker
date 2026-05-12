using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using taskplan_calendar.ViewModel;

namespace taskplan_calendar.Controllers
{
    [Authorize]
    public class CalendarController : Controller
    {
        public IActionResult Index(string view = "monthly")
        {
            var validViews = new[] { "daily", "weekly", "monthly" };
            if (!Array.Exists(validViews, element => element == view))
                view = "monthly";

            var model = new CalendarViewViewModel
            {
                ViewType = view,
                ViewDate = DateTime.UtcNow
            };

            return View(model);
        }
    }
}
