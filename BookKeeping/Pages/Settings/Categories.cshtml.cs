using BookKeeping.Models;
using BookKeeping.Services;
using BookKeeping.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookKeeping.Pages.Settings;

/// <summary>
/// Category settings page model.
/// </summary>
public class CategoriesModel : PageModel
{
    private readonly ICategoryService _categoryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoriesModel"/> class.
    /// </summary>
    /// <param name="categoryService">Category service.</param>
    public CategoriesModel(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Gets expense categories.
    /// </summary>
    public List<Category> ExpenseCategories { get; private set; } = [];

    /// <summary>
    /// Gets income categories.
    /// </summary>
    public List<Category> IncomeCategories { get; private set; } = [];

    /// <summary>
    /// Loads category settings page data.
    /// </summary>
    public async Task OnGetAsync()
    {
        await LoadCategoriesAsync();
    }

    /// <summary>
    /// Creates a category.
    /// </summary>
    public async Task<IActionResult> OnPostCreateAsync(CategoryInputModel input)
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync();
            TempData["ToastMessage"] = "請檢查輸入資料";
            TempData["ToastType"] = "error";
            return Page();
        }

        try
        {
            await _categoryService.CreateAsync(new Category
            {
                Name = input.Name,
                Icon = input.Icon,
                Type = input.Type,
                Color = input.Color
            });
        }
        catch (InvalidOperationException)
        {
            await LoadCategoriesAsync();
            TempData["ToastMessage"] = "分類名稱已存在";
            TempData["ToastType"] = "warning";
            return Page();
        }

        TempData["ToastMessage"] = "分類已新增";
        TempData["ToastType"] = "success";
        return RedirectToPage();
    }

    /// <summary>
    /// Updates a category.
    /// </summary>
    public async Task<IActionResult> OnPostUpdateAsync(int id, CategoryInputModel input)
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync();
            TempData["ToastMessage"] = "請檢查輸入資料";
            TempData["ToastType"] = "error";
            return Page();
        }

        var existingCategory = (await _categoryService.GetAllAsync())
            .FirstOrDefault(c => c.Id == id);

        if (existingCategory is null)
        {
            TempData["ToastMessage"] = "找不到分類";
            TempData["ToastType"] = "error";
            return RedirectToPage();
        }

        existingCategory.Name = input.Name;
        existingCategory.Icon = input.Icon;
        existingCategory.Type = input.Type;
        existingCategory.Color = input.Color;

        try
        {
            await _categoryService.UpdateAsync(existingCategory);
        }
        catch (InvalidOperationException)
        {
            await LoadCategoriesAsync();
            TempData["ToastMessage"] = "分類名稱已存在";
            TempData["ToastType"] = "warning";
            return Page();
        }

        TempData["ToastMessage"] = "分類已更新";
        TempData["ToastType"] = "success";
        return RedirectToPage();
    }

    /// <summary>
    /// Deletes a category when no transactions are linked.
    /// </summary>
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        if (await _categoryService.HasTransactionsAsync(id))
        {
            TempData["ToastMessage"] = "分類正在使用中，請改用刪除並遷移";
            TempData["ToastType"] = "warning";
            return RedirectToPage();
        }

        var success = await _categoryService.DeleteAsync(id);
        TempData["ToastMessage"] = success ? "分類已刪除" : "無法刪除分類";
        TempData["ToastType"] = success ? "success" : "warning";
        return RedirectToPage();
    }

    /// <summary>
    /// Deletes a category and migrates linked transactions.
    /// </summary>
    public async Task<IActionResult> OnPostDeleteAndMigrateAsync(int id, int targetCategoryId)
    {
        var success = await _categoryService.DeleteAndMigrateAsync(id, targetCategoryId);
        TempData["ToastMessage"] = success ? "分類已刪除，交易已遷移" : "刪除或遷移失敗";
        TempData["ToastType"] = success ? "success" : "error";
        return RedirectToPage();
    }

    /// <summary>
    /// Loads expense and income categories.
    /// </summary>
    private async Task LoadCategoriesAsync()
    {
        var categories = await _categoryService.GetAllAsync();
        ExpenseCategories = categories
            .Where(c => c.Type == TransactionType.Expense)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Id)
            .ToList();
        IncomeCategories = categories
            .Where(c => c.Type == TransactionType.Income)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Id)
            .ToList();
    }
}
