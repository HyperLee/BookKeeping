using BookKeeping.Data;
using BookKeeping.Models;
using BookKeeping.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping.Services;

/// <summary>
/// Service implementation for budget CRUD and budget progress calculation.
/// </summary>
public class BudgetService : IBudgetService
{
    private readonly BookKeepingDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="BudgetService"/> class.
    /// </summary>
    /// <param name="context">Database context.</param>
    public BudgetService(BookKeepingDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<Budget> CreateAsync(Budget budget)
    {
        await ValidateExpenseCategoryAsync(budget.CategoryId);
        await EnsureUniqueBudgetAsync(budget.CategoryId, budget.Period);

        if (budget.StartDate == default)
        {
            budget.StartDate = GetDefaultStartDate();
        }

        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();
        return budget;
    }

    /// <inheritdoc/>
    public async Task<Budget?> UpdateAsync(Budget budget)
    {
        var existingBudget = await _context.Budgets
            .FirstOrDefaultAsync(item => item.Id == budget.Id);

        if (existingBudget is null)
        {
            return null;
        }

        await ValidateExpenseCategoryAsync(budget.CategoryId);
        await EnsureUniqueBudgetAsync(budget.CategoryId, budget.Period, budget.Id);

        existingBudget.CategoryId = budget.CategoryId;
        existingBudget.Amount = budget.Amount;
        existingBudget.Period = budget.Period;
        existingBudget.StartDate = budget.StartDate == default
            ? existingBudget.StartDate
            : budget.StartDate;

        await _context.SaveChangesAsync();
        return existingBudget;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int budgetId)
    {
        var budget = await _context.Budgets
            .FirstOrDefaultAsync(item => item.Id == budgetId);

        if (budget is null)
        {
            return false;
        }

        budget.IsDeleted = true;
        budget.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc/>
    public async Task<List<BudgetProgressViewModel>> GetAllWithProgressAsync(DateOnly? referenceDate = null)
    {
        var targetDate = referenceDate ?? DateOnly.FromDateTime(DateTime.Now);
        var budgets = await _context.Budgets
            .Join(
                _context.Categories.Where(c => c.Type == TransactionType.Expense),
                budget => budget.CategoryId,
                category => category.Id,
                (budget, category) => new { Budget = budget, Category = category })
            .OrderBy(item => item.Category.SortOrder)
            .ThenBy(item => item.Category.Id)
            .ToListAsync();

        var results = new List<BudgetProgressViewModel>(budgets.Count);
        foreach (var item in budgets)
        {
            var progress = await BuildProgressAsync(item.Budget, item.Category, targetDate);
            if (progress is not null)
            {
                results.Add(progress);
            }
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task<BudgetProgressViewModel?> CheckBudgetStatusAsync(int categoryId, DateOnly? referenceDate = null)
    {
        var targetDate = referenceDate ?? DateOnly.FromDateTime(DateTime.Now);
        var budgets = await _context.Budgets
            .Where(item => item.CategoryId == categoryId)
            .Join(
                _context.Categories.Where(c => c.Type == TransactionType.Expense),
                budget => budget.CategoryId,
                category => category.Id,
                (budget, category) => new { Budget = budget, Category = category })
            .ToListAsync();

        if (budgets.Count == 0)
        {
            return null;
        }

        var selectedBudget = budgets
            .OrderBy(item => item.Budget.Period == BudgetPeriod.Monthly ? 0 : 1)
            .ThenBy(item => item.Budget.Id)
            .First();

        return await BuildProgressAsync(selectedBudget.Budget, selectedBudget.Category, targetDate);
    }

    /// <summary>
    /// Calculates progress for a budget in the resolved period.
    /// </summary>
    /// <param name="budget">Budget entity.</param>
    /// <param name="category">Expense category entity.</param>
    /// <param name="referenceDate">Reference date.</param>
    /// <returns>Progress result when budget is active; otherwise <see langword="null"/>.</returns>
    private async Task<BudgetProgressViewModel?> BuildProgressAsync(Budget budget, Category category, DateOnly referenceDate)
    {
        var (periodStartDate, periodEndDate) = ResolvePeriodRange(budget.Period, referenceDate, budget.StartDate);
        if (budget.StartDate > periodEndDate)
        {
            return null;
        }

        var spentAmount = await _context.Transactions
            .Where(transaction =>
                transaction.Type == TransactionType.Expense &&
                transaction.CategoryId == budget.CategoryId &&
                transaction.Date >= periodStartDate &&
                transaction.Date <= periodEndDate)
            .SumAsync(transaction => (decimal?)transaction.Amount) ?? 0m;

        var usageRate = budget.Amount <= 0m
            ? 0m
            : spentAmount / budget.Amount * 100m;

        return new BudgetProgressViewModel
        {
            BudgetId = budget.Id,
            CategoryId = budget.CategoryId,
            CategoryName = category.Name,
            CategoryIcon = category.Icon,
            BudgetAmount = budget.Amount,
            SpentAmount = spentAmount,
            UsageRate = usageRate,
            Status = ResolveStatus(usageRate),
            Period = budget.Period,
            StartDate = budget.StartDate
        };
    }

    /// <summary>
    /// Resolves budget period start and end date for calculations.
    /// </summary>
    /// <param name="period">Budget period.</param>
    /// <param name="referenceDate">Reference date.</param>
    /// <param name="budgetStartDate">Configured budget start date.</param>
    /// <returns>Period start and end date.</returns>
    private static (DateOnly StartDate, DateOnly EndDate) ResolvePeriodRange(BudgetPeriod period, DateOnly referenceDate, DateOnly budgetStartDate)
    {
        return period switch
        {
            BudgetPeriod.Weekly => ResolveWeeklyRange(referenceDate, budgetStartDate),
            _ => ResolveMonthlyRange(referenceDate, budgetStartDate)
        };
    }

    /// <summary>
    /// Resolves monthly date range and applies budget start date lower bound.
    /// </summary>
    /// <param name="referenceDate">Reference date.</param>
    /// <param name="budgetStartDate">Configured budget start date.</param>
    /// <returns>Monthly range.</returns>
    private static (DateOnly StartDate, DateOnly EndDate) ResolveMonthlyRange(DateOnly referenceDate, DateOnly budgetStartDate)
    {
        var monthStartDate = new DateOnly(referenceDate.Year, referenceDate.Month, 1);
        var monthEndDate = monthStartDate.AddMonths(1).AddDays(-1);
        var effectiveStartDate = budgetStartDate > monthStartDate ? budgetStartDate : monthStartDate;
        return (effectiveStartDate, monthEndDate);
    }

    /// <summary>
    /// Resolves weekly date range (Monday to Sunday) and applies budget start date lower bound.
    /// </summary>
    /// <param name="referenceDate">Reference date.</param>
    /// <param name="budgetStartDate">Configured budget start date.</param>
    /// <returns>Weekly range.</returns>
    private static (DateOnly StartDate, DateOnly EndDate) ResolveWeeklyRange(DateOnly referenceDate, DateOnly budgetStartDate)
    {
        var daysSinceMonday = ((int)referenceDate.DayOfWeek + 6) % 7;
        var weekStartDate = referenceDate.AddDays(-daysSinceMonday);
        var weekEndDate = weekStartDate.AddDays(6);
        var effectiveStartDate = budgetStartDate > weekStartDate ? budgetStartDate : weekStartDate;
        return (effectiveStartDate, weekEndDate);
    }

    /// <summary>
    /// Resolves status by usage rate threshold.
    /// </summary>
    /// <param name="usageRate">Usage rate percentage.</param>
    /// <returns>Status string value.</returns>
    private static string ResolveStatus(decimal usageRate)
    {
        return usageRate switch
        {
            < 80m => "normal",
            <= 100m => "warning",
            _ => "exceeded"
        };
    }

    /// <summary>
    /// Ensures budget uniqueness by category and period.
    /// </summary>
    /// <param name="categoryId">Category identifier.</param>
    /// <param name="period">Budget period.</param>
    /// <param name="budgetId">Optional current budget identifier for updates.</param>
    private async Task EnsureUniqueBudgetAsync(int categoryId, BudgetPeriod period, int? budgetId = null)
    {
        var duplicateExists = await _context.Budgets
            .AnyAsync(item =>
                item.CategoryId == categoryId &&
                item.Period == period &&
                (!budgetId.HasValue || item.Id != budgetId.Value));

        if (duplicateExists)
        {
            throw new InvalidOperationException("此分類與週期的預算已存在");
        }
    }

    /// <summary>
    /// Validates that category exists and is an expense category.
    /// </summary>
    /// <param name="categoryId">Category identifier.</param>
    private async Task ValidateExpenseCategoryAsync(int categoryId)
    {
        var isExpenseCategory = await _context.Categories
            .AnyAsync(category => category.Id == categoryId && category.Type == TransactionType.Expense);

        if (!isExpenseCategory)
        {
            throw new InvalidOperationException("預算僅支援支出分類");
        }
    }

    /// <summary>
    /// Gets default start date for new budgets.
    /// </summary>
    /// <returns>First day of current month.</returns>
    private static DateOnly GetDefaultStartDate()
    {
        var now = DateTime.Now;
        return new DateOnly(now.Year, now.Month, 1);
    }
}
