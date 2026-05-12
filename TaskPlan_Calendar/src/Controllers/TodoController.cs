using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using taskplan_calendar.Data;
using taskplan_calendar.Models;
using taskplan_calendar.ViewModel;

namespace taskplan_calendar.Controllers
{
    [Authorize]
    public class TodoController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public TodoController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);
            
            // Get user's todo lists
            var todoLists = _db.TodoLists
                .Where(tl => tl.UserId == userId)
                .Include(tl => tl.Todos)
                    .ThenInclude(t => t.Category)
                .OrderBy(tl => tl.CreatedAt)
                .ToList();

            // Get todos not assigned to any list
            var unassignedTodos = _db.Todos
                .Where(t => t.UserId == userId && t.TodoListId == null)
                .Include(t => t.Category)
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.DueDate)
                .ToList();

            // Create a default list for unassigned todos
            if (unassignedTodos.Any() && !todoLists.Any(tl => tl.Name == "No List"))
            {
                var defaultList = new TodoList
                {
                    Name = "No List",
                    UserId = userId,
                    Todos = unassignedTodos
                };
                todoLists.Insert(0, defaultList);
            }

            ViewBag.TodoLists = todoLists;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateTodo([FromBody] CreateEditTodoViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = _userManager.GetUserId(User);

                var todo = new Todo
                {
                    UserId = userId,
                    Title = model.Title,
                    Description = model.Description,
                    DueDate = model.DueDate,
                    IsDone = false,
                    Priority = model.Priority,
                    CategoryId = model.CategoryId,
                    TodoListId = model.TodoListId,
                    IsRecurring = model.IsRecurring,
                    RecurrencePattern = model.RecurrencePattern,
                    RecurrenceEndDate = model.RecurrenceEndDate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _db.Todos.Add(todo);
                _db.SaveChanges();

                return Ok(new { id = todo.Id, message = "Todo created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateTodo(int id, [FromBody] CreateEditTodoViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = _userManager.GetUserId(User);
                var todo = _db.Todos.FirstOrDefault(t => t.Id == id && t.UserId == userId);

                if (todo == null)
                    return NotFound(new { error = "Todo not found" });

                todo.Title = model.Title;
                todo.Description = model.Description;
                todo.DueDate = model.DueDate;
                todo.IsDone = model.IsDone;
                todo.Priority = model.Priority;
                todo.CategoryId = model.CategoryId;
                todo.TodoListId = model.TodoListId;
                todo.IsRecurring = model.IsRecurring;
                todo.RecurrencePattern = model.RecurrencePattern;
                todo.RecurrenceEndDate = model.RecurrenceEndDate;
                todo.UpdatedAt = DateTime.UtcNow;

                _db.SaveChanges();

                return Ok(new { message = "Todo updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteTodo(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var todo = _db.Todos.FirstOrDefault(t => t.Id == id && t.UserId == userId);

                if (todo == null)
                    return NotFound(new { error = "Todo not found" });

                _db.Todos.Remove(todo);
                _db.SaveChanges();

                return Ok(new { message = "Todo deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}/toggle")]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleTodo(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var todo = _db.Todos.FirstOrDefault(t => t.Id == id && t.UserId == userId);

                if (todo == null)
                    return NotFound(new { error = "Todo not found" });

                todo.IsDone = !todo.IsDone;
                todo.UpdatedAt = DateTime.UtcNow;
                _db.SaveChanges();

                return Ok(new { isDone = todo.IsDone, message = "Todo toggled successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
