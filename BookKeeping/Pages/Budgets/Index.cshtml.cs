using BookKeeping.Models;
using BookKeeping.Services;
using BookKeeping.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookKeeping.Pages.Budgets;

/// <summary>
/// Budget settings and tracking page model.
/// </summary>
public class IndexModel : PageModel
{
    private readonly IBudgetService _budgetService;
    private readonly ICategoryService _categoryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexModel"/> class.
    /// </summary>
    /// <param name="budgetService">Budget service.</param>
    /// <param name="categoryService">Category service.</param>
    public IndexModel(IBudgetService budgetService, ICategoryService categoryService)
    {
        _budgetService = budgetService;
        _categoryService = categoryService;
    }

    /// <summary>
    /// Gets active budgets with usage progress.
    /// </summary>
    public List<BudgetProgressViewModel> Budgets { get; private set; } = [];

    /// <summary>
    /// Gets expense categories for dropdown selection.
    /// </summary>
    public List<Category> ExpenseCategories { get; private set; } = [];

    /// <summary>
    /// Loads budgets and expense categories.
    /// </summary>
    public async Task OnGetAsync()
    {
        await LoadPageDataAsync();
    }

    /// <summary>
    /// Creates a budget.
    /// </summary>
    /// <param name="input">Budget input model.</param>
    /// <returns>Result for PRG flow.</returns>
    public async Task<IActionResult> OnPostCreateAsync(BudgetInputModel input)
    {
        if (!ModelState.IsValid)
        {
            await LoadPageDataAsync();
            TempData["ToastMessage"] = "請檢查輸入資料";
            TempData["ToastType"] = "error";
            return Page();
        }

        try
        {
            await _budgetService.CreateAsync(new Budget
            {
                CategoryId = input.CategoryId,
                Amount = input.Amount,
                Period = input.Period,
                StartDate = ResolveStartDate(input.StartDate)
            });
        }
        catch (InvalidOperationException exception)
        {
            await LoadPageDataAsync();
            TempData["ToastMessage"] = exception.Message;
            TempData["ToastType"] = "warning";
            return Page();
        }

        TempData["ToastMessage"] = "預算已新增";
        TempData["ToastType"] = "success";
        return RedirectToPage();
    }

    /// <summary>
    /// Updates a budget.
    /// </summary>
    /// <param name="id">Budget identifier.</param>
    /// <param name="input">Budget input model.</param>
    /// <returns>Result for PRG flow.</returns>
    public async Task<IActionResult> OnPostUpdateAsync(int id, BudgetInputModel input)
    {
        if (!ModelState.IsValid)
        {
            await LoadPageDataAsync();
            TempData["ToastMessage"] = "請檢查輸入資料";
            TempData["ToastType"] = "error";
            return Page();
        }

        try
        {
            var updatedBudget = await _budgetService.UpdateAsync(new Budget
            {
                Id = id,
                CategoryId = input.CategoryId,
                Amount = input.Amount,
                Period = input.Period,
                StartDate = ResolveStartDate(input.StartDate)
            });

            if (updatedBudget is null)
            {
                TempData["ToastMessage"] = "找不到預算";
                TempData["ToastType"] = "error";
                return RedirectToPage();
            }
        }
        catch (InvalidOperationException exception)
        {
            await LoadPageDataAsync();
            TempData["ToastMessage"] = exception.Message;
            TempData["ToastType"] = "warning";
            return Page();
        }

        TempData["ToastMessage"] = "預算已更新";
        TempData["ToastType"] = "success";
        return RedirectToPage();
    }

    /// <summary>
    /// Deletes a budget.
    /// </summary>
    /// <param name="id">Budget identifier.</param>
    /// <returns>Result for PRG flow.</returns>
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var deleted = await _budgetService.DeleteAsync(id);
        TempData["ToastMessage"] = deleted ? "預算已刪除" : "找不到預算";
        TempData["ToastType"] = deleted ? "success" : "error";
        return RedirectToPage();
    }

    /// <summary>
    /// Returns budget status json for a specific category.
    /// </summary>
    /// <param name="categoryId">Expense category identifier.</param>
    /// <returns>Status payload or not found.</returns>
    public async Task<IActionResult> OnGetCheckStatusAsync(int categoryId)
    {
        if (categoryId <= 0)
        {
            return BadRequest();
        }

        var budgetStatus = await _budgetService.CheckBudgetStatusAsync(categoryId);
        if (budgetStatus is null)
        {
            return NotFound();
        }

        return new JsonResult(new
        {
            categoryName = budgetStatus.CategoryName,
            budgetAmount = budgetStatus.BudgetAmount,
            spentAmount = budgetStatus.SpentAmount,
            usageRate = budgetStatus.UsageRate,
            status = budgetStatus.Status,
            message = BuildStatusMessage(budgetStatus)
        });
    }

    /// <summary>
    /// Loads page budgets and expense categories.
    /// </summary>
    private async Task LoadPageDataAsync()
    {
        ExpenseCategories = await _categoryService.GetByTypeAsync(TransactionType.Expense);
        Budgets = await _budgetService.GetAllWithProgressAsync();
    }

    /// <summary>
    /// Resolves start date with fallback to current month first day.
    /// </summary>
    /// <param name="startDate">Optional input start date.</param>
    /// <returns>Resolved start date.</returns>
    private static DateOnly ResolveStartDate(DateOnly? startDate)
    {
        if (startDate.HasValue)
        {
            return startDate.Value;
        }

        var now = DateTime.Now;
        return new DateOnly(now.Year, now.Month, 1);
    }

    /// <summary>
    /// Builds user-facing budget status message for toast display.
    /// </summary>
    /// <param name="budgetStatus">Budget status.</param>
    /// <returns>Status message.</returns>
    private static string BuildStatusMessage(BudgetProgressViewModel budgetStatus)
    {
        var periodLabel = budgetStatus.Period == BudgetPeriod.Weekly ? "本週" : "本月";
        return budgetStatus.Status switch
        {
            "warning" => $"{budgetStatus.CategoryName}{periodLabel}已使用 {budgetStatus.UsageRate:0.##}%（${budgetStatus.SpentAmount:N0} / ${budgetStatus.BudgetAmount:N0}）",
            "exceeded" => $"{budgetStatus.CategoryName}{periodLabel}已超支 {budgetStatus.UsageRate:0.##}%（${budgetStatus.SpentAmount:N0} / ${budgetStatus.BudgetAmount:N0}）",
            _ => $"{budgetStatus.CategoryName}{periodLabel}使用率 {budgetStatus.UsageRate:0.##}%（${budgetStatus.SpentAmount:N0} / ${budgetStatus.BudgetAmount:N0}）"
        };
    }
}
