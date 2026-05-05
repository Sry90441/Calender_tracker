using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using taskplan_calendar.Models;
using taskplan_calendar.Services;
using taskplan_calendar.ViewModel;

namespace taskplan_calendar.Controllers
{
    /// <summary>
    /// Controller for managing user categories.
    /// Handles CRUD operations and ensures proper authorization and validation.
    /// All actions require user to be authenticated.
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
        }

        /// <summary>
        /// Gets the current authenticated user's ID.
        /// </summary>
        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? throw new UnauthorizedAccessException("User ID not found in claims");
        }

        /// <summary>
        /// GET: api/category
        /// Retrieves all categories for the current user.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(CategoryListViewModel), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetUserCategories()
        {
            try
            {
                var userId = GetUserId();
                var categories = await _categoryService.GetUserCategoriesAsync(userId);

                var viewModels = new List<CategoryViewModel>();
                foreach (var category in categories)
                {
                    var usageCount = await _categoryService.GetCategoryUsageCountAsync(category.Id);
                    viewModels.Add(MapToViewModel(category, usageCount));
                }

                return Ok(new CategoryListViewModel
                {
                    Categories = viewModels,
                    TotalCount = viewModels.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/category/{id}
        /// Retrieves a specific category by ID (must be owned by current user).
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CategoryViewModel), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetCategory(int id)
        {
            try
            {
                var userId = GetUserId();
                var category = await _categoryService.GetCategoryByIdAsync(id, userId);
                var usageCount = await _categoryService.GetCategoryUsageCountAsync(id);

                return Ok(MapToViewModel(category, usageCount));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Category not found" });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "You do not have permission to access this category" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/category
        /// Creates a new category for the current user.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CategoryViewModel), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> CreateCategory([FromBody] CreateEditCategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetUserId();
                var category = new Category
                {
                    Name = model.Name,
                    Description = model.Description,
                    ColorHex = model.ColorHex ?? "#007BFF",
                    IconName = model.IconName ?? "tag"
                };

                var createdCategory = await _categoryService.CreateCategoryAsync(category, userId);
                var viewModel = MapToViewModel(createdCategory, 0);

                return CreatedAtAction(nameof(GetCategory), new { id = createdCategory.Id }, viewModel);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// PUT: api/category/{id}
        /// Updates an existing category (must be owned by current user).
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CategoryViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CreateEditCategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetUserId();
                var updatedCategory = new Category
                {
                    Name = model.Name,
                    Description = model.Description,
                    ColorHex = model.ColorHex ?? "#007BFF",
                    IconName = model.IconName ?? "tag"
                };

                var result = await _categoryService.UpdateCategoryAsync(id, updatedCategory, userId);
                var usageCount = await _categoryService.GetCategoryUsageCountAsync(id);
                var viewModel = MapToViewModel(result, usageCount);

                return Ok(viewModel);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Category not found" });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "You do not have permission to update this category" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// DELETE: api/category/{id}
        /// Deletes a category (must be owned by current user and not in use).
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var userId = GetUserId();
                await _categoryService.DeleteCategoryAsync(id, userId);

                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Category not found" });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "You do not have permission to delete this category" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Helper method to map Category entity to CategoryViewModel.
        /// </summary>
        private CategoryViewModel MapToViewModel(Category category, int usageCount)
        {
            return new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ColorHex = category.ColorHex,
                IconName = category.IconName,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                UsageCount = usageCount
            };
        }
    }
}
