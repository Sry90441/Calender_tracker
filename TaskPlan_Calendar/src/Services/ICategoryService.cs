using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using taskplan_calendar.Models;

namespace taskplan_calendar.Services
{
    /// <summary>
    /// Service interface for category management operations.
    /// Provides abstraction layer for business logic, enabling testability and loose coupling.
    /// </summary>
    public interface ICategoryService
    {
        /// <summary>
        /// Retrieves all categories for a specific user.
        /// </summary>
        Task<IEnumerable<Category>> GetUserCategoriesAsync(string userId);

        /// <summary>
        /// Retrieves a specific category by ID, ensuring user ownership.
        /// </summary>
        Task<Category> GetCategoryByIdAsync(int id, string userId);

        /// <summary>
        /// Creates a new category for the user.
        /// Validates name uniqueness per user and color format.
        /// </summary>
        /// <returns>Created category with assigned ID</returns>
        /// <exception cref="InvalidOperationException">Thrown if name already exists for user</exception>
        Task<Category> CreateCategoryAsync(Category category, string userId);

        /// <summary>
        /// Updates an existing category.
        /// Only owner (userId) can update.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Thrown if user doesn't own the category</exception>
        Task<Category> UpdateCategoryAsync(int id, Category updatedCategory, string userId);

        /// <summary>
        /// Deletes a category by ID.
        /// Only owner can delete. Checks if category is in use.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Thrown if user doesn't own the category</exception>
        /// <exception cref="InvalidOperationException">Thrown if category is still in use</exception>
        Task<bool> DeleteCategoryAsync(int id, string userId);

        /// <summary>
        /// Checks if a category name already exists for the user.
        /// </summary>
        Task<bool> CategoryNameExistsAsync(string name, string userId, int? excludeId = null);

        /// <summary>
        /// Gets the count of todos and calendar events using this category.
        /// </summary>
        Task<int> GetCategoryUsageCountAsync(int categoryId);
    }
}
