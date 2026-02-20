using BookKeeping.Models;
using BookKeeping.Services;
using BookKeeping.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookKeeping.Pages.Settings;

/// <summary>
/// Account settings page model.
/// </summary>
public class AccountsModel : PageModel
{
    private readonly IAccountService _accountService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountsModel"/> class.
    /// </summary>
    /// <param name="accountService">Account service.</param>
    public AccountsModel(IAccountService accountService)
    {
        _accountService = accountService;
    }

    /// <summary>
    /// Gets account list.
    /// </summary>
    public List<Account> Accounts { get; private set; } = [];

    /// <summary>
    /// Gets calculated account balances.
    /// </summary>
    public Dictionary<int, decimal> AccountBalances { get; private set; } = [];

    /// <summary>
    /// Loads account settings page data.
    /// </summary>
    public async Task OnGetAsync()
    {
        await LoadAccountsAsync();
    }

    /// <summary>
    /// Creates an account.
    /// </summary>
    public async Task<IActionResult> OnPostCreateAsync(AccountInputModel input)
    {
        if (!ModelState.IsValid)
        {
            await LoadAccountsAsync();
            TempData["ToastMessage"] = "請檢查輸入資料";
            TempData["ToastType"] = "error";
            return Page();
        }

        try
        {
            await _accountService.CreateAsync(new Account
            {
                Name = input.Name,
                Type = input.Type,
                Icon = input.Icon,
                InitialBalance = input.InitialBalance,
                Currency = "TWD"
            });
        }
        catch (InvalidOperationException)
        {
            await LoadAccountsAsync();
            TempData["ToastMessage"] = "帳戶名稱已存在";
            TempData["ToastType"] = "warning";
            return Page();
        }

        TempData["ToastMessage"] = "帳戶已新增";
        TempData["ToastType"] = "success";
        return RedirectToPage();
    }

    /// <summary>
    /// Updates an account.
    /// </summary>
    public async Task<IActionResult> OnPostUpdateAsync(int id, AccountInputModel input)
    {
        if (!ModelState.IsValid)
        {
            await LoadAccountsAsync();
            TempData["ToastMessage"] = "請檢查輸入資料";
            TempData["ToastType"] = "error";
            return Page();
        }

        var existingAccount = (await _accountService.GetAllAsync())
            .FirstOrDefault(a => a.Id == id);

        if (existingAccount is null)
        {
            TempData["ToastMessage"] = "找不到帳戶";
            TempData["ToastType"] = "error";
            return RedirectToPage();
        }

        existingAccount.Name = input.Name;
        existingAccount.Type = input.Type;
        existingAccount.Icon = input.Icon;
        existingAccount.InitialBalance = input.InitialBalance;

        try
        {
            await _accountService.UpdateAsync(existingAccount);
        }
        catch (InvalidOperationException)
        {
            await LoadAccountsAsync();
            TempData["ToastMessage"] = "帳戶名稱已存在";
            TempData["ToastType"] = "warning";
            return Page();
        }

        TempData["ToastMessage"] = "帳戶已更新";
        TempData["ToastType"] = "success";
        return RedirectToPage();
    }

    /// <summary>
    /// Deletes an account when no transactions are linked.
    /// </summary>
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        if (await _accountService.HasTransactionsAsync(id))
        {
            TempData["ToastMessage"] = "帳戶正在使用中，無法刪除";
            TempData["ToastType"] = "warning";
            return RedirectToPage();
        }

        var success = await _accountService.DeleteAsync(id);
        TempData["ToastMessage"] = success ? "帳戶已刪除" : "刪除帳戶失敗";
        TempData["ToastType"] = success ? "success" : "error";
        return RedirectToPage();
    }

    /// <summary>
    /// Loads accounts and calculated balances.
    /// </summary>
    private async Task LoadAccountsAsync()
    {
        Accounts = await _accountService.GetAllAsync();
        AccountBalances = [];

        foreach (var account in Accounts)
        {
            AccountBalances[account.Id] = await _accountService.GetBalanceAsync(account.Id);
        }
    }
}
