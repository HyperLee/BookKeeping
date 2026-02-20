# Tasks: Open BookKeeping — 開源個人記帳理財工具

**Input**: Design documents from `/specs/001-open-bookkeeping-core/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

**Tests**: 本 tasks.md 遵循憲章原則 II（測試優先開發 NON-NEGOTIABLE），每個 User Story 包含對應的單元測試與整合測試任務。測試任務置於各 Phase 的 `### Tests` 小節中，實作時應遵循紅-綠-重構（Red-Green-Refactor）流程：先撰寫失敗測試 → 實作功能 → 測試通過 → 重構。

**Organization**: 任務按 User Story 分組，每個 Story 可獨立實作與測試。

**Terminology**: 本文件統一使用「交易紀錄」指稱 Transaction 實體相關操作（同義詞「收支紀錄」僅在面向使用者的 UI 文案中使用）。

## Format: `[ID] [P?] [Story] Description`

- **[P]**: 可平行執行（不同檔案、無未完成相依性）
- **[Story]**: 任務所屬 User Story（US1, US2, US3 等）
- 所有描述包含精確檔案路徑

## Path Conventions

- **主專案**: `BookKeeping/`（ASP.NET Core Razor Pages）
- **測試專案**: `BookKeeping.Tests/`（xUnit）
- 路徑基於 plan.md 定義的專案結構

---

## Phase 1: Setup（專案初始化）

**Purpose**: 配置開發環境、安裝相依套件、建立專案基礎結構

- [ ] T001 Create .editorconfig with C# 14 formatting rules (file-scoped namespaces, nullable reference types, indentation) at repository root .editorconfig
- [ ] T002 Install NuGet packages (Microsoft.EntityFrameworkCore.Sqlite, Microsoft.EntityFrameworkCore.Design, Serilog.AspNetCore, Serilog.Formatting.Compact, HtmlSanitizer) in BookKeeping/BookKeeping.csproj
- [ ] T003 [P] Create libman.json and install Chart.js 4.x via LibMan to BookKeeping/wwwroot/lib/chart.js/ in BookKeeping/libman.json
- [ ] T004 [P] Create test project with xUnit, Moq, Microsoft.AspNetCore.Mvc.Testing, Microsoft.EntityFrameworkCore.InMemory, coverlet.collector in BookKeeping.Tests/BookKeeping.Tests.csproj

---

## Phase 2: Foundational（阻塞性基礎建設）

**Purpose**: 核心基礎架構，所有 User Story 的前置條件

**⚠️ CRITICAL**: 此階段必須全部完成，才能開始任何 User Story 的實作

- [ ] T005 [P] Create ISoftDeletable interface (IsDeleted, DeletedAt) in BookKeeping/Models/ISoftDeletable.cs
- [ ] T006 [P] Create IAuditable interface (CreatedAt, UpdatedAt) in BookKeeping/Models/IAuditable.cs
- [ ] T007 [P] Create TransactionType enum (Income=0, Expense=1) in BookKeeping/Models/TransactionType.cs
- [ ] T008 [P] Create AccountType enum (Cash=0, Bank=1, CreditCard=2, EPayment=3) in BookKeeping/Models/AccountType.cs
- [ ] T009 [P] Create BudgetPeriod enum (Monthly=0, Weekly=1) in BookKeeping/Models/BudgetPeriod.cs
- [ ] T010 [P] Create Transaction entity with data annotations, ISoftDeletable, IAuditable per data-model.md in BookKeeping/Models/Transaction.cs
- [ ] T011 [P] Create Category entity with data annotations, seed data constants, ISoftDeletable, IAuditable per data-model.md in BookKeeping/Models/Category.cs
- [ ] T012 [P] Create Account entity with data annotations, ISoftDeletable, IAuditable per data-model.md in BookKeeping/Models/Account.cs
- [ ] T013 [P] Create Budget entity with data annotations, ISoftDeletable, IAuditable per data-model.md in BookKeeping/Models/Budget.cs
- [ ] T014 Create BookKeepingDbContext with DbSets, global query filters (!IsDeleted), composite indexes, FK relationships (Restrict), SaveChangesAsync override for soft-delete and audit timestamps in BookKeeping/Data/BookKeepingDbContext.cs
- [ ] T015 [P] Configure appsettings.json with SQLite connection string (Data Source=bookkeeping.db) and Serilog settings (File sink, Day rolling, 30-day retention, CompactJsonFormatter) in BookKeeping/appsettings.json
- [ ] T016 [P] Configure appsettings.Development.json with Debug-level Serilog minimum level in BookKeeping/appsettings.Development.json
- [ ] T017 Configure Program.cs with AddDbContext (SQLite), Serilog bootstrap, HtmlSanitizer singleton, MapStaticAssets, WithStaticAssets, HTTPS redirection, HSTS, anti-forgery in BookKeeping/Program.cs
- [ ] T018 [P] Create TransactionValidator with Amount>0, valid date, required CategoryId/AccountId, Note max 500 chars rules in BookKeeping/Validation/TransactionValidator.cs
- [ ] T019 Update _Layout.cshtml with responsive navigation bar (mobile: bottom fixed nav with icons; desktop ≥768px: top/side nav), Chart.js script reference, Toast partial inclusion in BookKeeping/Pages/Shared/_Layout.cshtml
- [ ] T020 [P] Update _Layout.cshtml.css with navigation bar styles, mobile-first breakpoints, active state indicators in BookKeeping/Pages/Shared/_Layout.cshtml.css
- [ ] T021 [P] Create _Toast.cshtml partial view reading TempData["ToastMessage"] and TempData["ToastType"] to render Bootstrap 5 Toast (success=green auto-dismiss 3s, warning=yellow manual, error=red manual) in BookKeeping/Pages/Shared/_Toast.cshtml
- [ ] T022 [P] Create toast.js with Toast initialization logic (auto-show on page load, auto-dismiss timing, manual close handlers) in BookKeeping/wwwroot/js/toast.js
- [ ] T023 Update site.css with global mobile-first responsive styles (320px min-width, Bootstrap overrides, form styles, table styles) in BookKeeping/wwwroot/css/site.css
- [ ] T024 Create TestWebApplicationFactory with in-memory SQLite provider, test seed data, scoped DbContext replacement in BookKeeping.Tests/Helpers/TestWebApplicationFactory.cs

### Tests for Phase 2

- [ ] T070 [P] Create TransactionValidationTests — verify Amount>0, valid date required, CategoryId/AccountId required, Note max 500 chars, future date rejected in BookKeeping.Tests/Unit/Models/TransactionValidationTests.cs
- [ ] T071 [P] Create DbContextTests — verify global query filters (soft-deleted entities excluded), composite indexes exist, FK Restrict on delete, SaveChangesAsync auto-sets CreatedAt/UpdatedAt in BookKeeping.Tests/Integration/Data/DbContextTests.cs

**Checkpoint**: 基礎架構就緒、基礎測試通過 — 可開始 User Story 實作

---

## Phase 3: User Story 2 — 伺服器端資料持久化 (Priority: P1)

**Goal**: 確保所有資料持久儲存於伺服器端 SQLite 資料庫，不受瀏覽器關閉或快取清除影響

**Independent Test**: 新增數筆紀錄 → 關閉瀏覽器 → 重新開啟 App → 確認所有紀錄仍然存在且正確

- [ ] T025 [US2] Create DefaultDataSeeder with 8 preset expense categories (餐飲🍽️, 交通🚗, 娛樂🎮, 購物🛒, 居住🏠, 醫療🏥, 教育📚, 其他📎), 4 income categories (薪資💰, 獎金🎁, 投資收益📈, 其他收入💵), and 3 default accounts (現金💵, 銀行帳戶🏦, 信用卡💳) in BookKeeping/Data/Seed/DefaultDataSeeder.cs
- [ ] T026 [US2] Generate initial EF Core migration (InitialCreate) with all entity tables, indexes, and constraints via dotnet ef migrations add in BookKeeping/Data/Migrations/
- [ ] T027 [US2] Register database auto-migration (Migrate) and DefaultDataSeeder execution on application startup in BookKeeping/Program.cs

### Tests for User Story 2

- [ ] T072 [US2] Create SeedDataTests — verify 8 preset expense categories, 4 income categories, 3 default accounts seeded correctly; verify re-seeding is idempotent in BookKeeping.Tests/Integration/Data/SeedDataTests.cs

**Checkpoint**: 資料庫自動建立、種子資料填入、種子資料測試通過、重啟後資料完整保留 — US2 驗證通過

---

## Phase 4: User Story 1 — 新增收支紀錄 (Priority: P1) 🎯 MVP

**Goal**: 使用者能新增、編輯、刪除收支紀錄，並在明細列表與 Dashboard 中查看

**Independent Test**: 開啟 App → 點擊新增 → 填寫 $150 餐飲支出 → 儲存 → 在明細列表中確認正確顯示

### Implementation for User Story 1

- [ ] T028 [P] [US1] Create ICategoryService interface (GetAllAsync, GetByTypeAsync) and CategoryService implementation in BookKeeping/Services/ICategoryService.cs and BookKeeping/Services/CategoryService.cs
- [ ] T029 [P] [US1] Create IAccountService interface (GetAllAsync, GetBalanceAsync) and AccountService implementation with balance calculation (InitialBalance + Income - Expense) in BookKeeping/Services/IAccountService.cs and BookKeeping/Services/AccountService.cs
- [ ] T030 [US1] Create ITransactionService interface (GetPagedAsync, GetByIdAsync, CreateAsync, UpdateAsync, SoftDeleteAsync) and TransactionService implementation with pagination (PageSize=20) and soft-delete in BookKeeping/Services/ITransactionService.cs and BookKeeping/Services/TransactionService.cs
- [ ] T031 [P] [US1] Create TransactionInputModel, TransactionListViewModel, TransactionDto, TransactionFilter DTOs per contracts/api-endpoints.md in BookKeeping/ViewModels/TransactionViewModel.cs
- [ ] T032 [P] [US1] Create DashboardViewModel, AccountBalanceDto, BudgetProgressDto DTOs per contracts/api-endpoints.md in BookKeeping/ViewModels/DashboardViewModel.cs
- [ ] T033 [US1] Register ICategoryService, IAccountService, ITransactionService in DI container (AddScoped) in BookKeeping/Program.cs
- [ ] T034 [P] [US1] Create Transactions/Create Razor Page with form (date, amount, type radio, category dropdown with frequently-used categories prioritized at top, account dropdown, note textarea), jQuery Validation, anti-forgery token, TempData Toast on success in BookKeeping/Pages/Transactions/Create.cshtml and BookKeeping/Pages/Transactions/Create.cshtml.cs
- [ ] T035 [P] [US1] Create Transactions/Edit Razor Page with pre-filled form, OnGetAsync(int id), OnPostAsync update handler, 404 handling in BookKeeping/Pages/Transactions/Edit.cshtml and BookKeeping/Pages/Transactions/Edit.cshtml.cs
- [ ] T036 [P] [US1] Create Transactions/Index Razor Page with transaction list table (date, amount, type icon, category, account, note), pagination, OnPostDeleteAsync soft-delete handler with confirmation in BookKeeping/Pages/Transactions/Index.cshtml and BookKeeping/Pages/Transactions/Index.cshtml.cs
- [ ] T037 [US1] Create Dashboard (Index) page with current month summary cards (total income, total expense, balance), account balance list, recent 10 transactions in BookKeeping/Pages/Index.cshtml and BookKeeping/Pages/Index.cshtml.cs
- [ ] T038 [US1] Add anti-duplicate-submission JavaScript guard (disable submit button on click, re-enable on validation failure) in BookKeeping/wwwroot/js/site.js

### Tests for User Story 1

- [ ] T073 [P] [US1] Create TransactionServiceTests — verify CreateAsync (valid input, audit timestamps), UpdateAsync (amount change, UpdatedAt updated), SoftDeleteAsync (IsDeleted=true, DeletedAt set), GetPagedAsync (pagination, sort by date desc, excludes soft-deleted) in BookKeeping.Tests/Unit/Services/TransactionServiceTests.cs
- [ ] T074 [P] [US1] Create AccountServiceTests — verify GetBalanceAsync (InitialBalance + Income - Expense calculation with decimal precision), GetAllAsync in BookKeeping.Tests/Unit/Services/AccountServiceTests.cs
- [ ] T075 [US1] Create TransactionPagesTests — verify Create page returns 200, POST with valid input redirects and persists, POST with invalid input returns validation errors, Edit page loads existing transaction, Delete soft-deletes record in BookKeeping.Tests/Integration/Pages/TransactionPagesTests.cs

**Checkpoint**: 可新增/編輯/刪除收支紀錄，Dashboard 顯示月摘要，單元/整合測試通過 — US1 MVP 驗證通過

---

## Phase 5: User Story 3 — 收支分類管理 (Priority: P1)

**Goal**: 使用者能管理自訂分類（新增/編輯/刪除），並管理帳戶設定

**Independent Test**: 開啟 App → 確認預設分類存在 → 新增自訂分類「寵物」→ 新增紀錄時確認可選擇該分類

**⚠️ 相依性**: 此 Phase 依賴 US1（Phase 4）的 T028/T029 完成（擴展相同的 ICategoryService 和 IAccountService 檔案），**不可與 Phase 4 平行執行**。

### Implementation for User Story 3

- [ ] T039 [US3] Extend ICategoryService and CategoryService with CreateAsync, UpdateAsync, DeleteAsync, HasTransactionsAsync, DeleteAndMigrateAsync (move transactions to target category before delete) in BookKeeping/Services/ICategoryService.cs and BookKeeping/Services/CategoryService.cs
- [ ] T040 [US3] Extend IAccountService and AccountService with CreateAsync, UpdateAsync, DeleteAsync, HasTransactionsAsync for account management in BookKeeping/Services/IAccountService.cs and BookKeeping/Services/AccountService.cs
- [ ] T041 [P] [US3] Create CategoryInputModel (Name, Icon, Type, Color) in BookKeeping/ViewModels/CategoryViewModel.cs
- [ ] T042 [P] [US3] Create AccountInputModel (Name, Type, Icon, InitialBalance) in BookKeeping/ViewModels/AccountViewModel.cs
- [ ] T043 [US3] Create Settings/Categories Razor Page with income/expense category lists, OnPostCreateAsync, OnPostUpdateAsync, OnPostDeleteAsync (with in-use check), OnPostDeleteAndMigrateAsync handlers in BookKeeping/Pages/Settings/Categories.cshtml and BookKeeping/Pages/Settings/Categories.cshtml.cs
- [ ] T044 [US3] Create Settings/Accounts Razor Page with account list (showing calculated balances), OnPostCreateAsync, OnPostUpdateAsync, OnPostDeleteAsync (with in-use check) handlers in BookKeeping/Pages/Settings/Accounts.cshtml and BookKeeping/Pages/Settings/Accounts.cshtml.cs

### Tests for User Story 3

- [ ] T076 [P] [US3] Create CategoryServiceTests — verify CreateAsync (unique name+type), UpdateAsync, DeleteAsync (blocked when has transactions), DeleteAndMigrateAsync (transactions moved to target category), HasTransactionsAsync, default category cannot be deleted in BookKeeping.Tests/Unit/Services/CategoryServiceTests.cs
- [ ] T077 [P] [US3] Create AccountServiceTests — verify CreateAsync (unique name), UpdateAsync, DeleteAsync (blocked when has transactions), HasTransactionsAsync in BookKeeping.Tests/Unit/Services/AccountServiceTests.cs

**Checkpoint**: 可新增/編輯/刪除分類與帳戶，預設分類不可刪除，使用中的分類提供遷移選項，單元測試通過 — US3 驗證通過

---

## Phase 6: User Story 4 — 月度收支摘要與視覺化圖表 (Priority: P2)

**Goal**: 使用者能查看月度收支摘要、分類圓餅圖、每日趨勢圖

**Independent Test**: 輸入 30 天收支資料 → 切換至報表頁 → 確認月度摘要、圓餅圖分類佔比、趨勢圖資料點正確

### Implementation for User Story 4

- [ ] T045 [US4] Create IReportService interface and ReportService with GetMonthlySummaryAsync (total income/expense/balance), GetCategoryBreakdownAsync (category percentages), GetDailyTrendsAsync (daily income/expense) in BookKeeping/Services/IReportService.cs and BookKeeping/Services/ReportService.cs
- [ ] T046 [P] [US4] Create MonthlyReportViewModel, CategoryExpenseDto (Name, Color, Amount, Percentage), DailyTrendDto (Date, Income, Expense) per contracts/api-endpoints.md in BookKeeping/ViewModels/MonthlyReportViewModel.cs
- [ ] T047 [US4] Create Reports/Index Razor Page with year/month selector, summary cards (income/expense/balance), empty state message (FR-015), chart containers in BookKeeping/Pages/Reports/Index.cshtml and BookKeeping/Pages/Reports/Index.cshtml.cs
- [ ] T048 [US4] Create charts.js with Chart.js doughnut chart (category expense breakdown with colors from Category.Color) and line chart (daily income/expense trends) with responsive:true and touch-friendly tooltips in BookKeeping/wwwroot/js/charts.js
- [ ] T049 [US4] Implement OnGetChartDataAsync AJAX named handler returning JSON (categoryExpenses + dailyTrends) for async chart data loading in BookKeeping/Pages/Reports/Index.cshtml.cs

### Tests for User Story 4

- [ ] T078 [P] [US4] Create ReportServiceTests — verify GetMonthlySummaryAsync (correct totals, balance = income - expense with decimal precision), GetCategoryBreakdownAsync (percentages sum to 100%), GetDailyTrendsAsync (correct daily aggregation), empty month returns zero values in BookKeeping.Tests/Unit/Services/ReportServiceTests.cs
- [ ] T079 [US4] Create ReportPagesTests — verify Reports page returns 200, chart data AJAX endpoint returns valid JSON, empty month displays friendly message (FR-015) in BookKeeping.Tests/Integration/Pages/ReportPagesTests.cs

**Checkpoint**: 報表頁顯示月摘要卡片、分類圓餅圖、每日趨勢折線圖、空白月份友善提示，測試通過 — US4 驗證通過

---

## Phase 7: User Story 5 — 預算設定與追蹤 (Priority: P2)

**Goal**: 使用者能設定分類月預算，系統追蹤使用率並於 80%/100% 閾值觸發提醒

**Independent Test**: 設定餐飲月預算 $5,000 → 新增餐飲支出超過 $5,000 → 確認超支提示出現且 Dashboard 進度條正確

### Implementation for User Story 5

- [ ] T050 [US5] Create IBudgetService interface and BudgetService with CreateAsync, UpdateAsync, DeleteAsync, GetAllWithProgressAsync (usage rate + status: normal/warning/exceeded), CheckBudgetStatusAsync (single category) in BookKeeping/Services/IBudgetService.cs and BookKeeping/Services/BudgetService.cs
- [ ] T051 [P] [US5] Create BudgetProgressViewModel and BudgetInputModel (CategoryId, Amount, Period) per contracts/api-endpoints.md in BookKeeping/ViewModels/BudgetProgressViewModel.cs
- [ ] T052 [US5] Create Budgets/Index Razor Page with budget list (progress bars: green <80%, yellow 80-100%, red >100%), OnPostCreateAsync, OnPostUpdateAsync, OnPostDeleteAsync handlers, category dropdown (expense only) in BookKeeping/Pages/Budgets/Index.cshtml and BookKeeping/Pages/Budgets/Index.cshtml.cs
- [ ] T053 [US5] Implement OnGetCheckStatusAsync AJAX named handler returning budget status JSON (categoryName, budgetAmount, spentAmount, usageRate, status, message) in BookKeeping/Pages/Budgets/Index.cshtml.cs
- [ ] T054 [US5] Integrate budget status check into Transactions/Create page — after successful save, AJAX call CheckStatus for the saved category, display Toast warning/error if budget ≥80% in BookKeeping/Pages/Transactions/Create.cshtml and BookKeeping/wwwroot/js/site.js
- [ ] T055 [US5] Add budget progress section to Dashboard page showing all active budgets with progress bars and status indicators in BookKeeping/Pages/Index.cshtml and BookKeeping/Pages/Index.cshtml.cs

### Tests for User Story 5

- [ ] T080 [US5] Create BudgetServiceTests — verify CreateAsync, UpdateAsync, DeleteAsync, GetAllWithProgressAsync (usage rate calculation with decimal precision), CheckBudgetStatusAsync (normal <80%, warning 80-100%, exceeded >100%), new month resets spending calculation in BookKeeping.Tests/Unit/Services/BudgetServiceTests.cs

**Checkpoint**: 預算可設定/編輯/刪除，Dashboard 顯示進度條，新增支出後即時顯示預算警告，測試通過 — US5 驗證通過

---

## Phase 8: User Story 6 — CSV 匯出 (Priority: P2)

**Goal**: 使用者能匯出收支紀錄為 CSV，支援日期範圍篩選，特殊字元正確處理

**Independent Test**: 新增 100 筆紀錄 → 點擊匯出 → 下載 CSV → 以 Excel 開啟確認欄位完整且格式正確

### Implementation for User Story 6

- [ ] T056 [US6] Create ICsvService interface and CsvService export logic with RFC 4180 compliance (quoted fields with commas/quotes/newlines), UTF-8 BOM, header row (日期,類型,金額,分類,帳戶,備註), date range filtering in BookKeeping/Services/ICsvService.cs and BookKeeping/Services/CsvService.cs
- [ ] T057 [US6] Add OnGetExportAsync handler to Transactions page returning FileContentResult (text/csv, UTF-8 BOM, Content-Disposition attachment) with optional startDate/endDate params in BookKeeping/Pages/Transactions/Index.cshtml.cs
- [ ] T058 [US6] Add export UI controls (date range picker, export button) to Transactions/Index page in BookKeeping/Pages/Transactions/Index.cshtml

### Tests for User Story 6

- [ ] T081 [US6] Create CsvServiceExportTests — verify RFC 4180 compliance (fields with commas/quotes/newlines properly escaped), UTF-8 BOM present, header row correct, date range filtering works, empty export produces header-only file in BookKeeping.Tests/Unit/Services/CsvServiceTests.cs

**Checkpoint**: 可匯出全部或指定日期範圍的紀錄為 CSV，Excel 正確開啟，測試通過 — US6 驗證通過

---

## Phase 9: User Story 7 — CSV 匯入 (Priority: P3)

**Goal**: 使用者能匯入標準格式 CSV 批次建立紀錄，錯誤行跳過並回報原因

**Independent Test**: 準備 10 筆標準格式 CSV → 匯入 → 確認全部正確建立

### Implementation for User Story 7

- [ ] T059 [US7] Extend ICsvService and CsvService with ImportAsync logic — StreamReader line-by-line parsing, row validation (date format, amount>0, category/account lookup), HtmlSanitizer on text fields (FR-035), 5MB size limit (FR-034), 10,000 row limit, auto-create missing categories, error collection with line numbers in BookKeeping/Services/ICsvService.cs and BookKeeping/Services/CsvService.cs
- [ ] T060 [P] [US7] Create ImportResultViewModel (TotalRows, SuccessCount, FailedCount) and ImportError (LineNumber, ErrorMessage) in BookKeeping/ViewModels/ImportResultViewModel.cs
- [ ] T061 [US7] Create Import/Index Razor Page with file upload form (accept=.csv), format instructions (.csv template download link), file size validation (client-side + server-side), import results display (success/failed counts, error detail list) in BookKeeping/Pages/Import/Index.cshtml and BookKeeping/Pages/Import/Index.cshtml.cs

### Tests for User Story 7

- [ ] T082 [US7] Create CsvServiceImportTests — verify valid CSV import (all rows created), invalid date row skipped with error message, amount<=0 row skipped, missing category auto-created, HtmlSanitizer strips XSS vectors from Note/Category/Account fields (FR-035), 5MB size limit enforced (FR-034), 10,000 row limit enforced, empty CSV (header only) returns 「無有效資料」 in BookKeeping.Tests/Unit/Services/CsvServiceTests.cs

**Checkpoint**: 可匯入標準 CSV，成功/失敗筆數清楚回報，錯誤行含行號與原因，測試通過 — US7 驗證通過

---

## Phase 10: User Story 8 — 搜尋與篩選 (Priority: P3)

**Goal**: 使用者能依日期範圍、分類、帳戶、金額範圍、關鍵字篩選紀錄

**Independent Test**: 建立 50 筆不同分類紀錄 → 分類篩選確認僅顯示該分類 → 關鍵字搜尋備註確認正確匹配

### Implementation for User Story 8

- [ ] T062 [US8] Extend TransactionService with multi-criteria filter support (StartDate, EndDate, CategoryId, AccountId, MinAmount, MaxAmount, Keyword search on Note field) and IQueryable composition in BookKeeping/Services/TransactionService.cs
- [ ] T063 [US8] Add filter panel UI (date range pickers, category dropdown, account dropdown, min/max amount inputs, keyword search textbox, clear filters button) to Transactions/Index page in BookKeeping/Pages/Transactions/Index.cshtml
- [ ] T064 [US8] Update Transactions/Index OnGetAsync to bind TransactionFilter from query string, apply filters, preserve filter state in URL for pagination in BookKeeping/Pages/Transactions/Index.cshtml.cs

### Tests for User Story 8

- [ ] T083 [US8] Create TransactionFilterTests — verify single filter (date range, categoryId, accountId, amount range, keyword), combined filters (multiple criteria), keyword search matches Note field (case-insensitive), empty filter returns all records, pagination preserves filter state in BookKeeping.Tests/Unit/Services/TransactionServiceTests.cs (append to existing file)

**Checkpoint**: 明細列表支援所有篩選條件組合，分頁保留篩選狀態，關鍵字搜尋備註欄位，測試通過 — US8 驗證通過

---

## Phase 11: Polish & Cross-Cutting Concerns

**Purpose**: 跨 User Story 的品質改善、安全強化、合規驗證與收尾工作

- [ ] T065 [P] Add Serilog structured logging to all services — Information level for CRUD operations (create/update/delete with entity ID and **old/new value snapshots for audit trail**; financial amounts logged as masked values e.g. "***50" per constitution §V/§VI), Error level for exceptions with stack trace and request context (FR-039, FR-040, FR-041) in BookKeeping/Services/*.cs
- [ ] T066 [P] Enhance global error handling — configure UseExceptionHandler middleware, update Error.cshtml with user-friendly error page, ensure all unhandled exceptions log to Serilog in BookKeeping/Program.cs and BookKeeping/Pages/Error.cshtml
- [ ] T067 Verify responsive design across all pages — 320px minimum width (iPhone SE), 768px desktop breakpoint, touch-friendly chart interactions, bottom nav on mobile in BookKeeping/wwwroot/css/site.css
- [ ] T068 Add XML doc comments to all public APIs (Models, Services interfaces, ViewModels) with <summary>, <param>, <returns>, <example> tags per .github/instructions/csharp.instructions.md
- [ ] T069 Run quickstart.md end-to-end validation — dotnet build, dotnet ef database update, verify seed data, dotnet run, navigate all 9 pages, confirm CRUD operations, verify CSV export/import, check Chart.js rendering
- [ ] T084 [P] Configure Content Security Policy (CSP) middleware — add CSP headers (default-src 'self', script-src 'self' for Chart.js/jQuery, style-src 'self' for Bootstrap, img-src 'self' data: for emoji icons) in BookKeeping/Program.cs (constitution §VI)
- [ ] T085 [P] WCAG 2.1 accessibility audit — verify semantic HTML structure (headings, landmarks, form labels), ARIA attributes on interactive elements (Toast, modal confirmations, progress bars), keyboard navigation for all pages, color contrast ratio ≥ 4.5:1 for text (constitution §III)
- [ ] T086 [P] Performance validation — basic benchmarks for SC-001 (new transaction < 30s UX flow), SC-002 (100-record monthly report < 2s), SC-003 (1,000-record CSV export < 5s), SC-007 (10,000-record list scroll/filter remains responsive); document results in validation notes
- [ ] T087 [P] Privacy & dependency audit (FR-027, SC-009) — audit all NuGet packages for telemetry/external API calls, verify no outbound network requests other than to self, document findings; create privacy self-assessment checklist
- [ ] T088 [P] UX walkthrough validation (SC-008) — manual walkthrough as first-time user: verify intuitive navigation, clear form labels, helpful empty states, Toast feedback, and successful first transaction creation without documentation
- [ ] T089 Create DashboardPageTests — verify Dashboard returns 200, displays current month summary (income/expense/balance), shows account balances, displays budget progress bars, lists recent 10 transactions in BookKeeping.Tests/Integration/Pages/DashboardPageTests.cs

---

## Dependencies & Execution Order

### Phase Dependencies

```
Phase 1: Setup ─────────────────► Phase 2: Foundational ─────────────────► Phase 3: US2
                                                                                │
                                                                                ▼
                                                              ┌─────── Phase 4: US1 (MVP) ──────► Phase 5: US3 (sequential)
                                                              │                │
                                                              │    ┌───────────┼───────────┐
                                                              │    ▼           ▼           ▼
                                                              │ Phase 6:    Phase 7:    Phase 8:
                                                              │   US4         US5         US6
                                                              │                           │
                                                              │    ┌───────────────────────┘
                                                              │    ▼           ▼
                                                              │ Phase 9:    Phase 10:
                                                              │   US7         US8
                                                              │    │           │
                                                              │    └─────┬─────┘
                                                              │          ▼
                                                              └──► Phase 11: Polish
```

### User Story Dependencies

- **US2 (P1)**: 無 Story 相依性，依賴 Foundational（Phase 2）完成 — 建立資料庫與種子資料
- **US1 (P1)** 🎯 MVP: 依賴 US2 完成（需要資料庫與種子資料）
- **US3 (P1)**: 依賴 US1（Phase 4）的 T028/T029 完成（擴展相同的 Service 介面檔案）。**不可與 US1 平行執行**
- **US4 (P2)**: 依賴 US1 完成（需要交易紀錄資料產生報表）
- **US5 (P2)**: 依賴 US1 完成（需要交易紀錄資料計算預算使用率）。可與 US4、US6 平行
- **US6 (P2)**: 依賴 US1 完成（需要交易紀錄資料進行匯出）。可與 US4、US5 平行
- **US7 (P3)**: 依賴 US1 完成（需要交易服務基礎架構）
- **US8 (P3)**: 依賴 US1 完成（擴展交易明細頁面）。可與 US7 平行

### Within Each User Story

- Models / DTOs 先於 Services
- Services 先於 Pages（Razor Pages）
- 核心實作先於整合（如預算檢查整合至新增交易流程）
- 每個 Story 完成後驗證 Checkpoint 再進入下一個

### Parallel Opportunities

**Phase 1**: T003 + T004 可平行（Chart.js 安裝與測試專案建立）

**Phase 2**: 高度平行
- T005~T009: 所有介面與列舉可平行
- T010~T013: 所有 Entity Model 可平行
- T015~T016: appsettings 配置可平行
- T018, T020~T022: UI 基礎建設可平行

**Phase 4 (US1)**: T028+T029 服務可平行；T031+T032 DTOs 可平行；T034+T035+T036 頁面可平行

**Phase 5 (US3)**: T041+T042 ViewModel 可平行（T039/T040 依序擴展 Service 介面，不可平行）

**Phase 6~8 (P2 Stories)**: US4、US5、US6 三個 P2 Story 在 US1 完成後**可平行執行**

**Phase 9~10 (P3 Stories)**: US7、US8 兩個 P3 Story **可平行執行**

---

## Parallel Example: User Story 1

```bash
# Step 1: 平行建立服務層（T028 + T029）
Task: "Create ICategoryService/CategoryService in BookKeeping/Services/"
Task: "Create IAccountService/AccountService in BookKeeping/Services/"

# Step 2: 建立 TransactionService（依賴 Step 1 完成）
Task: "Create ITransactionService/TransactionService in BookKeeping/Services/"

# Step 3: 平行建立 ViewModels（T031 + T032）
Task: "Create TransactionViewModel.cs in BookKeeping/ViewModels/"
Task: "Create DashboardViewModel.cs in BookKeeping/ViewModels/"

# Step 4: 註冊 DI（T033）
Task: "Register services in Program.cs"

# Step 5: 平行建立頁面（T034 + T035 + T036）
Task: "Create Transactions/Create page"
Task: "Create Transactions/Edit page"
Task: "Create Transactions/Index page"

# Step 6: Dashboard 頁面（T037）
Task: "Create Dashboard (Index) page"
```

---

## Parallel Example: P2 Stories

```bash
# US1 完成後，三個 P2 Story 可由不同開發者同時進行：

# Developer A: US4 — 月度報表
Task: "Create ReportService → Reports/Index page → charts.js"

# Developer B: US5 — 預算追蹤
Task: "Create BudgetService → Budgets/Index page → Dashboard integration"

# Developer C: US6 — CSV 匯出
Task: "Create CsvService export → Transactions export handler"
```

---

## Implementation Strategy

### MVP First（僅 User Story 1 + US2）

1. ✅ Complete Phase 1: Setup
2. ✅ Complete Phase 2: Foundational（核心基礎建設）
3. ✅ Complete Phase 3: US2（資料庫持久化）
4. ✅ Complete Phase 4: US1（收支紀錄 CRUD + Dashboard）
5. **STOP and VALIDATE**: 測試 US1 獨立功能（新增/編輯/刪除紀錄、Dashboard 摘要）
6. Deploy/Demo — 最小可用版本

### Incremental Delivery

1. Setup + Foundational + US2 → 資料庫就緒
2. + US1 → **MVP!**（核心記帳功能）→ Deploy/Demo
3. + US3 → 分類/帳戶管理 → Deploy/Demo
4. + US4 → 月度報表與圖表 → Deploy/Demo
5. + US5 → 預算追蹤 → Deploy/Demo
6. + US6 → CSV 匯出 → Deploy/Demo
7. + US7 → CSV 匯入 → Deploy/Demo
8. + US8 → 搜尋與篩選 → Deploy/Demo
9. + Polish → 日誌、錯誤處理、文件 → 正式發布

### Parallel Team Strategy

多開發者協作：

1. 團隊共同完成 Phase 1 + 2 + 3（Setup + Foundational + US2）
2. US2 完成後：
   - **Developer A**: US1（收支紀錄 CRUD）
   - US1 完成後：US3（分類管理，擴展相同 Service 介面）
3. US1 完成後：
   - **Developer A**: US4（報表）
   - **Developer B**: US5（預算）
   - **Developer C**: US6（CSV 匯出）
4. P2 完成後：
   - **Developer A**: US7（CSV 匯入）
   - **Developer B**: US8（搜尋篩選）
5. 全部完成 → Phase 11 Polish

---

## Notes

- [P] 標記的任務 = 不同檔案、無未完成相依性，可平行執行
- [Story] 標籤對映至 spec.md 中的 User Story，確保可追溯性
- 每個 User Story 應可獨立完成與測試
- **測試任務（T070-T083, T089）遵循憲章原則 II（TDD）：建議在對應實作任務之前撰寫測試**
- 每個任務或邏輯群組完成後建議 commit
- 在任何 Checkpoint 處可暫停驗證 Story 獨立性
- 金額一律使用 `decimal` 型別，確保財務計算精確度
- 所有 POST handler 預設包含 Anti-Forgery Token（Razor Pages 內建）
- 軟刪除透過 DbContext SaveChangesAsync override 自動處理
- 日誌中的金額採用部分遮罩策略（如 "***50"），符合憲章 §V 稽核需求與 §VI 隱私保護
