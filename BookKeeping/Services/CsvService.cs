using System.Globalization;
using System.Text;
using BookKeeping.Data;
using BookKeeping.Models;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping.Services;

/// <summary>
/// Service implementation for exporting transactions to RFC 4180 compatible CSV.
/// </summary>
public class CsvService : ICsvService
{
    private const string HeaderRow = "日期,類型,金額,分類,帳戶,備註";
    private const string CsvNewLine = "\r\n";
    private readonly BookKeepingDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvService"/> class.
    /// </summary>
    /// <param name="context">Database context.</param>
    public CsvService(BookKeepingDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportTransactionsAsync(DateOnly? startDate = null, DateOnly? endDate = null)
    {
        var query = _context.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.Account)
            .AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(t => t.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.Date <= endDate.Value);
        }

        var transactions = await query
            .OrderBy(t => t.Date)
            .ThenBy(t => t.Id)
            .ToListAsync();

        var csvContent = BuildCsvContent(transactions);
        return EncodeWithUtf8Bom(csvContent);
    }

    /// <summary>
    /// Builds CSV text with a fixed header and transaction rows.
    /// </summary>
    /// <param name="transactions">Transactions to export.</param>
    /// <returns>RFC 4180 compatible CSV content.</returns>
    private static string BuildCsvContent(IEnumerable<Transaction> transactions)
    {
        var builder = new StringBuilder();
        builder.Append(HeaderRow).Append(CsvNewLine);

        foreach (var transaction in transactions)
        {
            var row = string.Join(',',
                EscapeCsvField(transaction.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                EscapeCsvField(GetTransactionTypeLabel(transaction.Type)),
                EscapeCsvField(transaction.Amount.ToString(CultureInfo.InvariantCulture)),
                EscapeCsvField(transaction.Category?.Name ?? string.Empty),
                EscapeCsvField(transaction.Account?.Name ?? string.Empty),
                EscapeCsvField(transaction.Note ?? string.Empty));

            builder.Append(row).Append(CsvNewLine);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Converts transaction type values to localized CSV labels.
    /// </summary>
    /// <param name="type">Transaction type.</param>
    /// <returns>Localized label.</returns>
    private static string GetTransactionTypeLabel(TransactionType type)
    {
        return type switch
        {
            TransactionType.Income => "收入",
            TransactionType.Expense => "支出",
            _ => type.ToString()
        };
    }

    /// <summary>
    /// Escapes a CSV field according to RFC 4180.
    /// </summary>
    /// <param name="value">Original field value.</param>
    /// <returns>Escaped field value.</returns>
    private static string EscapeCsvField(string value)
    {
        var escapedValue = value.Replace("\"", "\"\"");

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{escapedValue}\"";
        }

        return escapedValue;
    }

    /// <summary>
    /// Encodes text using UTF-8 and prepends BOM bytes for Excel compatibility.
    /// </summary>
    /// <param name="content">CSV text content.</param>
    /// <returns>UTF-8 BOM encoded bytes.</returns>
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
