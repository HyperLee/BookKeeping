using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookKeeping.Models;
using BookKeeping.Services;
using BookKeeping.ViewModels;

namespace BookKeeping.Pages.Transactions;

public class EditModel : PageModel
{
    private readonly ICategoryService _categoryService;
    private readonly IAccountService _accountService;
    private readonly ITransactionService _transactionService;

    public EditModel(
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
    public int TransactionId { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var transaction = await _transactionService.GetByIdAsync(id);
        if (transaction == null)
        {
            return NotFound();
        }

        TransactionId = id;
        Input = new TransactionInputModel
        {
            Date = transaction.Date,
            Amount = transaction.Amount,
            Type = transaction.Type,
            CategoryId = transaction.CategoryId,
            AccountId = transaction.AccountId,
            Note = transaction.Note
        };

        // Load categories and accounts
        ExpenseCategories = await _categoryService.GetByTypeAsync(TransactionType.Expense);
        IncomeCategories = await _categoryService.GetByTypeAsync(TransactionType.Income);
        Accounts = await _accountService.GetAllAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            TransactionId = id;
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
            
            TransactionId = id;
            ExpenseCategories = await _categoryService.GetByTypeAsync(TransactionType.Expense);
            IncomeCategories = await _categoryService.GetByTypeAsync(TransactionType.Income);
            Accounts = await _accountService.GetAllAsync();
            
            TempData["ToastMessage"] = "日期不可為未來日期";
            TempData["ToastType"] = "error";
            return Page();
        }

        var transaction = await _transactionService.GetByIdAsync(id);
        if (transaction == null)
        {
            return NotFound();
        }

        // Update transaction
        transaction.Date = Input.Date;
        transaction.Amount = Input.Amount;
        transaction.Type = Input.Type;
        transaction.CategoryId = Input.CategoryId;
        transaction.AccountId = Input.AccountId;
        transaction.Note = Input.Note;

        await _transactionService.UpdateAsync(transaction);

        TempData["ToastMessage"] = "交易紀錄已更新";
        TempData["ToastType"] = "success";

        return RedirectToPage("/Transactions/Index");
    }
}
