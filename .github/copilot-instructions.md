# BookKeeping Copilot Instructions

## Build and test commands

```bash
# Build the whole solution
dotnet build BookKeeping.sln

# Run the web app
dotnet run --project BookKeeping/BookKeeping.csproj

# Run the full test suite
dotnet test BookKeeping.Tests/BookKeeping.Tests.csproj

# Run a single test (replace the FullyQualifiedName suffix as needed)
dotnet test BookKeeping.Tests/BookKeeping.Tests.csproj --no-build --filter "FullyQualifiedName~BookKeeping.Tests.Unit.Services.TransactionServiceTests.CreateAsync_ShouldPersistTransactionAndSetAuditTimestamps"

# Apply or create EF Core migrations when schema changes
dotnet ef database update --project BookKeeping/BookKeeping.csproj
dotnet ef migrations add <MigrationName> --project BookKeeping/BookKeeping.csproj

# Restore client-side libraries when static assets are missing
libman restore
```

## High-level architecture

- `BookKeeping/` is a single ASP.NET Core 10 Razor Pages app; `BookKeeping.Tests/` contains xUnit unit and integration tests.
- `Program.cs` is the composition root: it configures Serilog from `appsettings*.json`, registers the SQLite `BookKeepingDbContext`, `HtmlSanitizer`, and all scoped domain services, enables antiforgery, adds security headers/CSP nonce middleware, and runs `Database.Migrate()` plus `DefaultDataSeeder` on startup.
- The app uses a service layer directly over EF Core rather than a repository layer. Page models depend on interfaces such as `ITransactionService`, `IReportService`, `IBudgetService`, `ICategoryService`, `IAccountService`, and `ICsvService`; services query `BookKeepingDbContext` and apply the business rules.
- `BookKeepingDbContext` encodes several cross-cutting persistence rules: global soft-delete query filters, audit timestamp updates in `SaveChangesAsync`, `DeleteBehavior.Restrict` on foreign keys, and SQLite decimal columns stored as `TEXT`.
- Razor PageModels manually map entities into `ViewModels`/DTOs instead of using AutoMapper. POST handlers usually follow PRG: on success they `RedirectToPage()` and set `TempData["ToastMessage"]` / `TempData["ToastType"]`; on validation failure they reload dropdown/reference data and return `Page()`.
- Some pages expose handler endpoints in addition to HTML rendering: reports return chart JSON via `?handler=ChartData`, budgets return status JSON via `?handler=CheckStatus`, transactions export CSV via `?handler=Export`, and the import page delegates parsing/sanitization to `CsvService`.
- Integration tests use `TestWebApplicationFactory` to replace the app database with in-memory SQLite and exercise real Razor Pages over HTTP. Unit tests usually instantiate `BookKeepingDbContext` with EF Core InMemory and test services directly.

## Key conventions

- Core entities (`Transaction`, `Category`, `Account`, `Budget`) all implement `ISoftDeletable` and `IAuditable`. Deletes generally set `IsDeleted`/`DeletedAt` instead of removing rows, and normal queries rely on the global query filters. Use `IgnoreQueryFilters()` only when intentionally reading deleted rows.
- Default seed data is part of the runtime contract: startup seeds 8 expense categories, 4 income categories, and 3 TWD accounts if none exist. The seeded names are localized Chinese values such as `餐飲`, `薪資`, `現金`, and `銀行帳戶`; many tests and UI flows assume this data exists.
- Services own normalization and business-rule enforcement before save. Examples: trimming names/icons, preventing duplicate account names and duplicate category names within the same transaction type, restricting budgets to expense categories, and masking money values in structured logs.
- CSV behavior is localized and security-sensitive. Import/export uses UTF-8 with BOM, the fixed header `日期,類型,金額,分類,帳戶,備註`, and the labels `收入` / `支出`. `CsvService` sanitizes imported text with `HtmlSanitizer`, rejects files over 5 MB or 10,000 rows, auto-creates missing categories, and requires imported accounts to already exist.
- Tests are organized by folder (`Unit/` and `Integration/`), not by xUnit traits. For targeted runs, use `--filter "FullyQualifiedName~..."` instead of `Category=...` filters.
- When testing POST handlers for Razor Pages, fetch the page first and include the `__RequestVerificationToken`; the integration tests use this pattern throughout.
