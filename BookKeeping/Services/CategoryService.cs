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

    public CategoryService(BookKeepingDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.Type)
            .ThenBy(c => c.SortOrder)
            .ToListAsync();
    }

    public async Task<List<Category>> GetByTypeAsync(TransactionType type)
    {
        return await _context.Categories
            .Where(c => c.Type == type)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
    }
}
