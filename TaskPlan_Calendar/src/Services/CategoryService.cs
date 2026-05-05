using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using taskplan_calendar.Data;
using taskplan_calendar.Models;

namespace taskplan_calendar.Services
{
    /// <summary>
    /// Implementation of category management service.
    /// Handles all CRUD operations with proper validation, authorization, and error handling.
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Category>> GetUserCategoriesAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            return await _context.Categories
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(int id, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {id} not found for the current user");

            return category;
        }

        public async Task<Category> CreateCategoryAsync(Category category, string userId)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            ValidateCategoryData(category);

            if (await CategoryNameExistsAsync(category.Name, userId))
                throw new InvalidOperationException($"A category named '{category.Name}' already exists for this user");

            category.UserId = userId;
            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return category;
        }

        public async Task<Category> UpdateCategoryAsync(int id, Category updatedCategory, string userId)
        {
            if (updatedCategory == null)
                throw new ArgumentNullException(nameof(updatedCategory));

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            ValidateCategoryData(updatedCategory);

            var existingCategory = await GetCategoryByIdAsync(id, userId);

            if (await CategoryNameExistsAsync(updatedCategory.Name, userId, excludeId: id))
                throw new InvalidOperationException($"A category named '{updatedCategory.Name}' already exists for this user");

            existingCategory.Name = updatedCategory.Name;
            existingCategory.Description = updatedCategory.Description;
            existingCategory.ColorHex = updatedCategory.ColorHex;
            existingCategory.IconName = updatedCategory.IconName;
            existingCategory.UpdatedAt = DateTime.UtcNow;

            _context.Categories.Update(existingCategory);
            await _context.SaveChangesAsync();

            return existingCategory;
        }

        public async Task<bool> DeleteCategoryAsync(int id, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            var category = await GetCategoryByIdAsync(id, userId);

            int usageCount = await GetCategoryUsageCountAsync(id);
            if (usageCount > 0)
                throw new InvalidOperationException(
                    $"Cannot delete category '{category.Name}' because it is assigned to {usageCount} item(s). " +
                    "Please reassign or remove those items first.");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CategoryNameExistsAsync(string name, string userId, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            var query = _context.Categories
                .Where(c => c.UserId == userId && c.Name.ToLower() == name.ToLower());

            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<int> GetCategoryUsageCountAsync(int categoryId)
        {
            int todoCount = await _context.Todos
                .Where(t => t.CategoryId == categoryId)
                .CountAsync();

            return todoCount;
        }

        /// <summary>
        /// Validates category data for required fields and formats.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if validation fails</exception>
        private void ValidateCategoryData(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
                throw new ArgumentException("Category name is required", nameof(category.Name));

            if (category.Name.Length > 100)
                throw new ArgumentException("Category name cannot exceed 100 characters", nameof(category.Name));

            if (!string.IsNullOrEmpty(category.ColorHex))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(category.ColorHex, @"^#[0-9A-F]{6}$", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    throw new ArgumentException("Color must be a valid hex code (e.g., #FF5733)", nameof(category.ColorHex));
            }

            if (category.Description?.Length > 500)
                throw new ArgumentException("Description cannot exceed 500 characters", nameof(category.Description));
        }
    }
}
