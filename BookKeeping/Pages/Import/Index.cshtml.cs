using System.Text;
using BookKeeping.Services;
using BookKeeping.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookKeeping.Pages.Import;

/// <summary>
/// CSV import page model.
/// </summary>
public class IndexModel : PageModel
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private readonly ICsvService _csvService;

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexModel"/> class.
    /// </summary>
    /// <param name="csvService">CSV service.</param>
    public IndexModel(ICsvService csvService)
    {
        _csvService = csvService;
    }

    /// <summary>
    /// Gets or sets uploaded CSV file.
    /// </summary>
    [BindProperty]
    public IFormFile? CsvFile { get; set; }

    /// <summary>
    /// Gets import summary result.
    /// </summary>
    public ImportResultViewModel? ImportResult { get; private set; }

    /// <summary>
    /// Handles CSV import.
    /// </summary>
    /// <returns>Page result.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        if (CsvFile is null || CsvFile.Length == 0)
        {
            ModelState.AddModelError(nameof(CsvFile), "請選擇 CSV 檔案");
            return Page();
        }

        if (!Path.GetExtension(CsvFile.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(CsvFile), "僅支援 .csv 檔案格式");
            return Page();
        }

        if (CsvFile.Length > MaxFileSizeBytes)
        {
            ModelState.AddModelError(nameof(CsvFile), "檔案大小不可超過 5MB");
            return Page();
        }

        await using var csvStream = CsvFile.OpenReadStream();
        ImportResult = await _csvService.ImportTransactionsAsync(csvStream, CsvFile.Length, HttpContext.RequestAborted);

        if (ImportResult.Errors.Count == 0)
        {
            TempData["ToastMessage"] = $"CSV 匯入完成，成功 {ImportResult.SuccessCount} 筆";
            TempData["ToastType"] = "success";
        }
        else if (ImportResult.SuccessCount > 0)
        {
            TempData["ToastMessage"] = $"CSV 匯入完成，成功 {ImportResult.SuccessCount} 筆，失敗 {ImportResult.FailedCount} 筆";
            TempData["ToastType"] = "warning";
        }
        else
        {
            TempData["ToastMessage"] = "CSV 匯入失敗";
            TempData["ToastType"] = "error";
        }

        return Page();
    }

    /// <summary>
    /// Downloads CSV template.
    /// </summary>
    /// <returns>CSV template file.</returns>
    public FileContentResult OnGetTemplate()
    {
        var template = "日期,類型,金額,分類,帳戶,備註\r\n2026-02-15,支出,120,餐飲,現金,午餐";
        var templateBytes = EncodeWithUtf8Bom(template);
        return File(templateBytes, "text/csv; charset=utf-8", "bookkeeping-import-template.csv");
    }

    /// <summary>
    /// Encodes CSV text to UTF-8 BOM bytes.
    /// </summary>
    /// <param name="content">CSV text.</param>
    /// <returns>Encoded bytes.</returns>
    private static byte[] EncodeWithUtf8Bom(string content)
    {
        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        var preamble = encoding.GetPreamble();
        var contentBytes = encoding.GetBytes(content);
        var result = new byte[preamble.Length + contentBytes.Length];

        Buffer.BlockCopy(preamble, 0, result, 0, preamble.Length);
        Buffer.BlockCopy(contentBytes, 0, result, preamble.Length, contentBytes.Length);

        return result;
    }
}
