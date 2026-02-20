using Microsoft.AspNetCore.Mvc.RazorPages;
using BookKeeping.Services;
using BookKeeping.ViewModels;

namespace BookKeeping.Pages;

public class IndexModel : PageModel
{
    private readonly ITransactionService _transactionService;
    private readonly IAccountService _accountService;

    public IndexModel(ITransactionService transactionService, IAccountService accountService)
    {
        _transactionService = transactionService;
        _accountService = accountService;
    }

    public DashboardViewModel ViewModel { get; set; } = new();

    public async Task OnGetAsync()
    {
        var now = DateTime.Now;
        ViewModel.Year = now.Year;
        ViewModel.Month = now.Month;

        // Get current month transactions
        var startDate = new DateOnly(now.Year, now.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var (transactions, _) = await _transactionService.GetPagedAsync(
            page: 1,
            pageSize: 1000,
            startDate: startDate,
            endDate: endDate);

        // Calculate totals
        ViewModel.TotalIncome = transactions
            .Where(t => t.Type == Models.TransactionType.Income)
            .Sum(t => t.Amount);

        ViewModel.TotalExpense = transactions
            .Where(t => t.Type == Models.TransactionType.Expense)
            .Sum(t => t.Amount);

        ViewModel.Balance = ViewModel.TotalIncome - ViewModel.TotalExpense;

        // Get account balances
        var accounts = await _accountService.GetAllAsync();
        ViewModel.AccountBalances = new List<AccountBalanceDto>();
        foreach (var account in accounts)
        {
            var balance = await _accountService.GetBalanceAsync(account.Id);
            ViewModel.AccountBalances.Add(new AccountBalanceDto
            {
                Id = account.Id,
                Name = account.Name,
                Icon = account.Icon,
                CurrentBalance = balance
            });
        }

        // Get recent 10 transactions
        var (recentTransactions, _) = await _transactionService.GetPagedAsync(
            page: 1,
            pageSize: 10);

        ViewModel.RecentTransactions = recentTransactions.Select(t => new TransactionDto
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
    }
}

