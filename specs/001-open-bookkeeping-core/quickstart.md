# 快速入門指南：Open BookKeeping — 開源個人記帳理財工具

**分支**: `001-open-bookkeeping-core` | **日期**: 2026-02-20

## 前置需求

| 工具 | 版本 | 安裝方式 |
|------|------|----------|
| .NET SDK | 10.0+ | [https://dot.net/download](https://dot.net/download) |
| Git | 2.x+ | `brew install git`（macOS）或 `apt install git`（Linux） |
| VS Code（建議） | Latest | [https://code.visualstudio.com](https://code.visualstudio.com) |

**不需要額外安裝**：SQLite 內嵌於 EF Core SQLite provider，無需獨立安裝資料庫伺服器。

---

## 快速啟動

### 1. 複製專案

```bash
git clone https://github.com/HyperLee/BookKeeping.git
cd BookKeeping
```

### 2. 切換至功能分支

```bash
git checkout 001-open-bookkeeping-core
```

### 3. 還原相依套件

```bash
dotnet restore
```

### 4. 建構專案

```bash
dotnet build
```

### 5. 執行 EF Core 資料庫遷移

```bash
# 安裝 EF Core CLI（首次需要）
dotnet tool install --global dotnet-ef

# 執行遷移，建立 SQLite 資料庫
cd BookKeeping
dotnet ef database update
```

執行後會在 `BookKeeping/` 目錄下產生 `bookkeeping.db` 檔案，並自動填入預設分類與帳戶種子資料。

### 6. 啟動應用程式

```bash
dotnet run --project BookKeeping
```

應用程式會啟動在 `https://localhost:5001` 和 `http://localhost:5000`。

### 7. 開啟瀏覽器

前往 [https://localhost:5001](https://localhost:5001) 開始使用。

---

## 執行測試

### 全部測試

```bash
dotnet test
```

### 僅單元測試

```bash
dotnet test --filter "Category=Unit"
```

### 僅整合測試

```bash
dotnet test --filter "Category=Integration"
```

### 含覆蓋率報告

```bash
dotnet test --collect:"XPlat Code Coverage"
```

---

## 專案結構概覽

```
BookKeeping/                  # ASP.NET Core Razor Pages 主專案
├── Data/                     # EF Core DbContext + 遷移
├── Models/                   # 領域實體（Transaction、Category、Account、Budget）
├── Services/                 # 業務邏輯服務層
├── ViewModels/               # 頁面 ViewModel / DTO
├── Validation/               # 驗證邏輯
├── Pages/                    # Razor Pages（UI）
│   ├── Index.cshtml          # Dashboard
│   ├── Transactions/         # 收支紀錄 CRUD
│   ├── Reports/              # 月度報表 + 圖表
│   ├── Budgets/              # 預算管理
│   ├── Settings/             # 分類管理 + 帳戶管理
│   ├── Import/               # CSV 匯入
│   └── Shared/               # 共用佈局 + Partial Views
├── wwwroot/                  # 靜態資源（CSS、JS、第三方函式庫）
├── Program.cs                # 應用進入點 + DI 配置
└── appsettings.json          # 應用設定

BookKeeping.Tests/            # 測試專案
├── Unit/                     # 單元測試（Services、Models）
├── Integration/              # 整合測試（Pages、Data）
└── Helpers/                  # 測試輔助工具
```

---

## 關鍵配置說明

### appsettings.json

```jsonc
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=bookkeeping.db"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning"
      }
    },
    "WriteTo": [{
      "Name": "File",
      "Args": {
        "path": "logs/bookkeeping-.json",
        "rollingInterval": "Day",
        "retainedFileCountLimit": 30
      }
    }]
  }
}
```

### 開發環境設定（appsettings.Development.json）

```jsonc
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    }
  }
}
```

---

## 常見操作

### 新增 EF Core 遷移

```bash
cd BookKeeping
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

### 重設資料庫

```bash
cd BookKeeping
rm bookkeeping.db
dotnet ef database update
```

### 新增前端函式庫（LibMan）

```bash
dotnet tool install --global Microsoft.Web.LibraryManager.Cli
cd BookKeeping
libman install chart.js@4 --provider cdnjs --destination wwwroot/lib/chart.js
```

---

## NuGet 套件清單

### 主專案（BookKeeping.csproj）

| 套件 | 用途 |
|------|------|
| `Microsoft.EntityFrameworkCore.Sqlite` | SQLite 資料庫 provider |
| `Microsoft.EntityFrameworkCore.Design` | EF Core CLI 遷移工具 |
| `Serilog.AspNetCore` | 結構化日誌 |
| `Serilog.Formatting.Compact` | JSON 日誌格式化 |
| `HtmlSanitizer` | XSS 防護（CSV 匯入 sanitize） |

### 測試專案（BookKeeping.Tests.csproj）

| 套件 | 用途 |
|------|------|
| `Microsoft.AspNetCore.Mvc.Testing` | WebApplicationFactory 整合測試 |
| `xunit` | 測試框架 |
| `xunit.runner.visualstudio` | VS / VS Code 測試執行器 |
| `Moq` | Mock 框架 |
| `Microsoft.EntityFrameworkCore.InMemory` | 測試用 InMemory 資料庫 |
| `coverlet.collector` | 程式碼覆蓋率收集 |

---

## 疑難排解

| 問題 | 解決方式 |
|------|----------|
| `dotnet ef` 找不到命令 | 執行 `dotnet tool install --global dotnet-ef` |
| HTTPS 憑證警告 | 執行 `dotnet dev-certs https --trust` |
| SQLite 資料庫鎖定 | 確認沒有其他程式正在存取 `bookkeeping.db` |
| 前端函式庫缺失 | 執行 `libman restore`（需先安裝 LibMan CLI） |
| 測試執行失敗 | 確認已執行 `dotnet restore` 並且 SDK 版本為 10.0+ |
