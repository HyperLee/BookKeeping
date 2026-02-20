using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookKeeping.Services;
using BookKeeping.ViewModels;

namespace BookKeeping.Pages.Transactions;

public class IndexModel : PageModel
{
    private readonly ITransactionService _transactionService;
    private readonly ICategoryService _categoryService;
    private readonly IAccountService _accountService;
    private readonly ICsvService _csvService;

    public IndexModel(
        ITransactionService transactionService,
        ICategoryService categoryService,
        IAccountService accountService,
        ICsvService csvService)
    {
        _transactionService = transactionService;
        _categoryService = categoryService;
        _accountService = accountService;
        _csvService = csvService;
    }

    public TransactionListViewModel ViewModel { get; set; } = new();

    public async Task OnGetAsync([FromQuery] TransactionFilter filter)
    {
        var (transactions, totalCount) = await _transactionService.GetPagedAsync(
            filter.Page,
            filter.PageSize,
            filter.StartDate,
            filter.EndDate,
            filter.CategoryId,
            filter.AccountId,
            filter.MinAmount,
            filter.MaxAmount,
            filter.Keyword);

        ViewModel.Transactions = transactions.Select(t => new TransactionDto
        {
            Id = t.Id,
            Date = t.Date,
            Amount = t.Amount,
            Type = t.Type,
            CategoryName = t.Category?.Name ?? "",
            CategoryIcon = t.Category?.Icon ?? "",
            AccountName = t.Account?.Name ?? "",
            Note = t.Note
        }).ToList();

        ViewModel.TotalCount = totalCount;
        ViewModel.TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);
        ViewModel.Filter = filter;

        // Load categories and accounts for filter dropdowns
        var categories = await _categoryService.GetAllAsync();
        ViewModel.Categories = categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Icon = c.Icon,
            Type = c.Type
        }).ToList();

        var accounts = await _accountService.GetAllAsync();
        ViewModel.Accounts = accounts.Select(a => new AccountDto
        {
            Id = a.Id,
            Name = a.Name,
            Icon = a.Icon
        }).ToList();
    }

    public async Task<FileContentResult> OnGetExportAsync(DateOnly? startDate, DateOnly? endDate)
    {
        var csvBytes = await _csvService.ExportTransactionsAsync(startDate, endDate);
        var fileName = $"bookkeeping-export-{DateOnly.FromDateTime(DateTime.Now):yyyyMMdd}.csv";

        return File(csvBytes, "text/csv; charset=utf-8", fileName);
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var success = await _transactionService.SoftDeleteAsync(id);
        
        if (success)
        {
            TempData["ToastMessage"] = "交易紀錄已刪除";
            TempData["ToastType"] = "success";
        }
        else
        {
            TempData["ToastMessage"] = "刪除失敗";
            TempData["ToastType"] = "error";
        }

        return RedirectToPage();
    }
}
