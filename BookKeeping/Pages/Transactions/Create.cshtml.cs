using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookKeeping.Models;
using BookKeeping.Services;
using BookKeeping.ViewModels;

namespace BookKeeping.Pages.Transactions;

public class CreateModel : PageModel
{
    private readonly ICategoryService _categoryService;
    private readonly IAccountService _accountService;
    private readonly ITransactionService _transactionService;

    public CreateModel(
        ICategoryService categoryService,
        IAccountService accountService,
        ITransactionService transactionService)
    {
        _categoryService = categoryService;
        _accountService = accountService;
        _transactionService = transactionService;
    }

    [BindProperty]
    public TransactionInputModel Input { get; set; } = new();

    public List<Category> ExpenseCategories { get; set; } = [];
    public List<Category> IncomeCategories { get; set; } = [];
    public List<Account> Accounts { get; set; } = [];

    public async Task OnGetAsync()
    {
        // Set default date to today
        Input.Date = DateOnly.FromDateTime(DateTime.Now);

        // Load categories and accounts
        ExpenseCategories = await _categoryService.GetByTypeAsync(TransactionType.Expense);
        IncomeCategories = await _categoryService.GetByTypeAsync(TransactionType.Income);
        Accounts = await _accountService.GetAllAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            // Reload dropdowns on validation failure
            ExpenseCategories = await _categoryService.GetByTypeAsync(TransactionType.Expense);
            IncomeCategories = await _categoryService.GetByTypeAsync(TransactionType.Income);
            Accounts = await _accountService.GetAllAsync();
            
            TempData["ToastMessage"] = "請檢查輸入資料";
            TempData["ToastType"] = "error";
            return Page();
        }

        // Validate date is not in the future
        if (Input.Date > DateOnly.FromDateTime(DateTime.Now))
        {
            ModelState.AddModelError(nameof(Input.Date), "日期不可為未來日期");
            
            ExpenseCategories = await _categoryService.GetByTypeAsync(TransactionType.Expense);
            IncomeCategories = await _categoryService.GetByTypeAsync(TransactionType.Income);
            Accounts = await _accountService.GetAllAsync();
            
            TempData["ToastMessage"] = "日期不可為未來日期";
            TempData["ToastType"] = "error";
            return Page();
        }

        // Create transaction
        var transaction = new Transaction
        {
            Date = Input.Date,
            Amount = Input.Amount,
            Type = Input.Type,
            CategoryId = Input.CategoryId,
            AccountId = Input.AccountId,
            Note = Input.Note
        };

        await _transactionService.CreateAsync(transaction);

        TempData["ToastMessage"] = "交易紀錄已成功新增";
        TempData["ToastType"] = "success";
        if (transaction.Type == TransactionType.Expense)
        {
            TempData["BudgetCheckCategoryId"] = transaction.CategoryId;
        }

        return RedirectToPage();
    }
}
