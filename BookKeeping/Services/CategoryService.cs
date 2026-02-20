using Microsoft.EntityFrameworkCore;
using BookKeeping.Data;
using BookKeeping.Models;

namespace BookKeeping.Services;

/// <summary>
/// Service implementation for managing categories
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly BookKeepingDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryService"/> class.
    /// </summary>
    /// <param name="context">Database context.</param>
    public CategoryService(BookKeepingDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all categories ordered by type and sort order.
    /// </summary>
    public async Task<List<Category>> GetAllAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.Type)
            .ThenBy(c => c.SortOrder)
            .ToListAsync();
    }

    /// <summary>
    /// Get categories filtered by transaction type.
    /// </summary>
    public async Task<List<Category>> GetByTypeAsync(TransactionType type)
    {
        return await _context.Categories
            .Where(c => c.Type == type)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
    }

    /// <summary>
    /// Create a category after uniqueness validation.
    /// </summary>
    public async Task<Category> CreateAsync(Category category)
    {
        var normalizedName = category.Name.Trim();
        var hasDuplicate = await _context.Categories
            .AnyAsync(c => c.Type == category.Type && c.Name == normalizedName);

        if (hasDuplicate)
        {
            throw new InvalidOperationException("分類名稱已存在");
        }

        if (category.SortOrder <= 0)
        {
            var maxSortOrder = await _context.Categories
                .Where(c => c.Type == category.Type)
                .Select(c => (int?)c.SortOrder)
                .MaxAsync() ?? 0;
            category.SortOrder = maxSortOrder + 1;
        }

        category.Name = normalizedName;
        category.Icon = category.Icon.Trim();
        category.Color = NormalizeColor(category.Color);
        category.IsDefault = false;

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return category;
    }

    /// <summary>
    /// Update an existing category after uniqueness validation.
    /// </summary>
    public async Task<Category?> UpdateAsync(Category category)
    {
        var existingCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == category.Id);

        if (existingCategory is null)
        {
            return null;
        }

        var normalizedName = category.Name.Trim();
        var hasDuplicate = await _context.Categories
            .AnyAsync(c => c.Id != category.Id && c.Type == category.Type && c.Name == normalizedName);

        if (hasDuplicate)
        {
            throw new InvalidOperationException("分類名稱已存在");
        }

        if (category.SortOrder <= 0)
        {
            var maxSortOrder = await _context.Categories
                .Where(c => c.Type == category.Type && c.Id != category.Id)
                .Select(c => (int?)c.SortOrder)
                .MaxAsync() ?? 0;
            category.SortOrder = maxSortOrder + 1;
        }

        existingCategory.Name = normalizedName;
        existingCategory.Icon = category.Icon.Trim();
        existingCategory.Type = category.Type;
        existingCategory.Color = NormalizeColor(category.Color);
        existingCategory.SortOrder = category.SortOrder;

        await _context.SaveChangesAsync();

        return existingCategory;
    }

    /// <summary>
    /// Delete a category when it is not default and has no related transactions.
    /// </summary>
    public async Task<bool> DeleteAsync(int categoryId)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category is null || category.IsDefault || await HasTransactionsAsync(categoryId))
        {
            return false;
        }

        category.IsDeleted = true;
        category.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Check whether a category is referenced by any transactions.
    /// </summary>
    public async Task<bool> HasTransactionsAsync(int categoryId)
    {
        return await _context.Transactions.AnyAsync(t => t.CategoryId == categoryId);
    }

    /// <summary>
    /// Migrate transactions to a target category and delete the original category.
    /// </summary>
    public async Task<bool> DeleteAndMigrateAsync(int categoryId, int targetCategoryId)
    {
        if (categoryId == targetCategoryId)
        {
            return false;
        }

        var sourceCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (sourceCategory is null || sourceCategory.IsDefault)
        {
            return false;
        }

        var targetCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == targetCategoryId && c.Type == sourceCategory.Type);

        if (targetCategory is null)
        {
            return false;
        }

        var transactionsToMigrate = await _context.Transactions
            .Where(t => t.CategoryId == categoryId)
            .ToListAsync();

        foreach (var transaction in transactionsToMigrate)
        {
            transaction.CategoryId = targetCategoryId;
        }

        sourceCategory.IsDeleted = true;
        sourceCategory.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Normalize category color to null when input is blank.
    /// </summary>
    private static string? NormalizeColor(string? color)
    {
        return string.IsNullOrWhiteSpace(color) ? null : color.Trim();
    }
}
