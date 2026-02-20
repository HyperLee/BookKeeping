# BookKeeping Development Guidelines

Auto-generated from all feature plans. Last updated: 2026-02-20

## Active Technologies

- C# 14 / .NET 10.0 (001-open-bookkeeping-core)
- ASP.NET Core 10.0 Razor Pages
- Entity Framework Core 10.0 + SQLite
- Bootstrap 5 + jQuery 3.x + jQuery Validation
- Chart.js 4.x
- Serilog (structured JSON logging)
- HtmlSanitizer (XSS protection)
- xUnit + Moq + WebApplicationFactory

## Project Structure

```text
BookKeeping/                          # ASP.NET Core Razor Pages 主專案
├── Data/                             # EF Core DbContext + Migrations + Seed
├── Models/                           # 領域實體 (Transaction, Category, Account, Budget)
├── Services/                         # 業務邏輯服務層 (介面 + 實作)
├── ViewModels/                       # 頁面 ViewModel / DTO
├── Validation/                       # 驗證邏輯
├── Pages/                            # Razor Pages
│   ├── Index.cshtml                  # Dashboard
│   ├── Transactions/                 # 收支紀錄 CRUD
│   ├── Reports/                      # 月度報表 + 圖表
│   ├── Budgets/                      # 預算管理
│   ├── Settings/                     # 分類管理 + 帳戶管理
│   ├── Import/                       # CSV 匯入
│   └── Shared/                       # 共用佈局 + Partial Views
├── wwwroot/                          # 靜態資源
└── Program.cs                        # 進入點 + DI 配置

BookKeeping.Tests/                    # 測試專案
├── Unit/                             # 單元測試
├── Integration/                      # 整合測試
└── Helpers/                          # 測試輔助
```

## Commands

```bash
# Build
dotnet build BookKeeping/BookKeeping.csproj

# Run
dotnet run --project BookKeeping

# Test
dotnet test

# EF Core migrations
cd BookKeeping && dotnet ef migrations add <Name> && dotnet ef database update
```

## Code Style

C# 14: File-scoped namespaces, pattern matching, nullable reference types enabled.
Follow PascalCase for public members, camelCase for private fields, prefix interfaces with "I".
All monetary amounts MUST use `decimal` (never float/double).
All entities implement ISoftDeletable + IAuditable interfaces.
XML doc comments required for public APIs.

## Recent Changes

- 001-open-bookkeeping-core: MVP — 收支紀錄管理、分類系統、月度摘要、視覺化圖表、預算追蹤、CSV 匯出匯入

<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->
