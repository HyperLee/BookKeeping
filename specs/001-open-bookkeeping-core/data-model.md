# 資料模型：Open BookKeeping — 開源個人記帳理財工具

**分支**: `001-open-bookkeeping-core` | **日期**: 2026-02-20

## 實體關係圖

```
┌──────────────┐       ┌──────────────┐
│   Category   │       │   Account    │
│──────────────│       │──────────────│
│ Id (PK)      │       │ Id (PK)      │
│ Name         │       │ Name         │
│ Icon         │       │ Type         │
│ Type         │◄──┐   │ Icon         │
│ Color        │   │   │ InitialBal.  │◄──┐
│ SortOrder    │   │   │ Currency     │   │
│ IsDefault    │   │   │ IsDeleted    │   │
│ IsDeleted    │   │   │ DeletedAt    │   │
│ DeletedAt    │   │   │ CreatedAt    │   │
│ CreatedAt    │   │   │ UpdatedAt    │   │
│ UpdatedAt    │   │   └──────────────┘   │
└──────┬───────┘   │                      │
       │           │   ┌──────────────┐   │
       │           └───│ Transaction  │───┘
       │               │──────────────│
       │               │ Id (PK)      │
       │               │ Date         │
       │               │ Amount       │
       │               │ Type         │
       │               │ CategoryId(FK)│
       │               │ AccountId(FK)│
       │               │ Note         │
       │               │ IsDeleted    │
       │               │ DeletedAt    │
       │               │ CreatedAt    │
       │               │ UpdatedAt    │
       │               └──────────────┘
       │
       ▼
┌──────────────┐
│    Budget    │
│──────────────│
│ Id (PK)      │
│ CategoryId(FK)│
│ Amount       │
│ Period       │
│ StartDate    │
│ IsDeleted    │
│ DeletedAt    │
│ CreatedAt    │
│ UpdatedAt    │
└──────────────┘
```

**關係摘要**:
- `Transaction` → `Category`: 多對一（每筆交易屬於一個分類）
- `Transaction` → `Account`: 多對一（每筆交易屬於一個帳戶）
- `Budget` → `Category`: 多對一（每筆預算對應一個分類）
- `Category` ← `Transaction`: 一對多
- `Category` ← `Budget`: 一對多
- `Account` ← `Transaction`: 一對多

---

## 實體定義

### Transaction（交易紀錄）

| 欄位 | C# 型別 | SQLite 型別 | 限制 | 說明 |
|------|---------|------------|------|------|
| `Id` | `int` | INTEGER | PK, Auto-increment | 唯一識別碼 |
| `Date` | `DateOnly` | TEXT | NOT NULL | 交易日期（ISO 8601: YYYY-MM-DD） |
| `Amount` | `decimal` | TEXT | NOT NULL, > 0 | 金額（正數，使用 `decimal` 確保精確度） |
| `Type` | `TransactionType` (enum) | INTEGER | NOT NULL | 收入 (Income=0) / 支出 (Expense=1) |
| `CategoryId` | `int` | INTEGER | FK → Category.Id, NOT NULL | 分類外鍵 |
| `AccountId` | `int` | INTEGER | FK → Account.Id, NOT NULL | 帳戶外鍵 |
| `Note` | `string?` | TEXT | MaxLength(500), NULLABLE | 備註 |
| `IsDeleted` | `bool` | INTEGER | NOT NULL, DEFAULT 0 | 軟刪除旗標 |
| `DeletedAt` | `DateTime?` | TEXT | NULLABLE | 刪除時間（UTC） |
| `CreatedAt` | `DateTime` | TEXT | NOT NULL | 建立時間（UTC）（FR-005） |
| `UpdatedAt` | `DateTime` | TEXT | NOT NULL | 最後更新時間（UTC）（FR-005） |

**驗證規則**:
- `Amount` MUST > 0（FR-004）
- `Date` MUST 為有效 ISO 8601 日期（FR-004）
- `CategoryId` MUST 對應存在的分類（FR-004）
- `AccountId` MUST 對應存在的帳戶
- `Note` 最大長度 500 字元

**索引**:
- `IX_Transaction_Date` — `Date` DESC（支援月報查詢、日期範圍篩選 FR-029）
- `IX_Transaction_CategoryId` — `CategoryId`（支援分類篩選 FR-029、報表分類統計 FR-013）
- `IX_Transaction_AccountId` — `AccountId`（支援帳戶篩選、餘額計算 FR-036）
- `IX_Transaction_AccountId_Type` — `AccountId`, `Type`（複合索引，加速帳戶餘額計算）

**狀態轉換**: 無（交易紀錄為數據物件，無狀態機）

---

### Category（分類）

| 欄位 | C# 型別 | SQLite 型別 | 限制 | 說明 |
|------|---------|------------|------|------|
| `Id` | `int` | INTEGER | PK, Auto-increment | 唯一識別碼 |
| `Name` | `string` | TEXT | NOT NULL, MaxLength(50), UNIQUE per Type | 分類名稱 |
| `Icon` | `string` | TEXT | NOT NULL, MaxLength(10) | 圖示（emoji）（FR-008） |
| `Type` | `TransactionType` (enum) | INTEGER | NOT NULL | 收入分類 / 支出分類 |
| `Color` | `string?` | TEXT | NULLABLE, MaxLength(7) | 顏色 HEX（用於圖表，如 `#FF6384`） |
| `SortOrder` | `int` | INTEGER | NOT NULL, DEFAULT 0 | 排序順序 |
| `IsDefault` | `bool` | INTEGER | NOT NULL, DEFAULT 0 | 是否為系統預設分類 |
| `IsDeleted` | `bool` | INTEGER | NOT NULL, DEFAULT 0 | 軟刪除旗標 |
| `DeletedAt` | `DateTime?` | TEXT | NULLABLE | 刪除時間（UTC） |
| `CreatedAt` | `DateTime` | TEXT | NOT NULL | 建立時間（UTC） |
| `UpdatedAt` | `DateTime` | TEXT | NOT NULL | 最後更新時間（UTC） |

**驗證規則**:
- `Name` MUST NOT 為空白
- `Name` + `Type` 組合 MUST UNIQUE（同類型不可重名）
- `Icon` MUST NOT 為空白

**預設種子資料（FR-006、FR-007）**:

| 名稱 | Icon | Type | Color |
|------|------|------|-------|
| 餐飲 | 🍽️ | Expense | #FF6384 |
| 交通 | 🚗 | Expense | #36A2EB |
| 娛樂 | 🎮 | Expense | #FFCE56 |
| 購物 | 🛒 | Expense | #4BC0C0 |
| 居住 | 🏠 | Expense | #9966FF |
| 醫療 | 🏥 | Expense | #FF9F40 |
| 教育 | 📚 | Expense | #C9CBCF |
| 其他 | 📎 | Expense | #7C8798 |
| 薪資 | 💰 | Income | #4CAF50 |
| 獎金 | 🎁 | Income | #8BC34A |
| 投資收益 | 📈 | Income | #00BCD4 |
| 其他收入 | 💵 | Income | #009688 |

**業務規則**:
- 刪除分類前 MUST 檢查是否有交易紀錄引用（FR-009）
- 若有引用，MUST 提供遷移選項（將相關紀錄移至其他分類）
- 系統預設分類（`IsDefault = true`）可編輯名稱/圖示，但不可刪除

---

### Account（帳戶）

| 欄位 | C# 型別 | SQLite 型別 | 限制 | 說明 |
|------|---------|------------|------|------|
| `Id` | `int` | INTEGER | PK, Auto-increment | 唯一識別碼 |
| `Name` | `string` | TEXT | NOT NULL, MaxLength(50), UNIQUE | 帳戶名稱 |
| `Type` | `AccountType` (enum) | INTEGER | NOT NULL | 帳戶類型 |
| `Icon` | `string` | TEXT | NOT NULL, MaxLength(10) | 圖示（emoji） |
| `InitialBalance` | `decimal` | TEXT | NOT NULL, DEFAULT 0 | 初始餘額 |
| `Currency` | `string` | TEXT | NOT NULL, MaxLength(3), DEFAULT "TWD" | 幣別 ISO 4217（V1 單一幣別） |
| `IsDeleted` | `bool` | INTEGER | NOT NULL, DEFAULT 0 | 軟刪除旗標 |
| `DeletedAt` | `DateTime?` | TEXT | NULLABLE | 刪除時間（UTC） |
| `CreatedAt` | `DateTime` | TEXT | NOT NULL | 建立時間（UTC） |
| `UpdatedAt` | `DateTime` | TEXT | NOT NULL | 最後更新時間（UTC） |

**AccountType 列舉**:
```csharp
public enum AccountType
{
    Cash = 0,       // 現金
    Bank = 1,       // 銀行
    CreditCard = 2, // 信用卡
    EPayment = 3    // 電子支付
}
```

**驗證規則**:
- `Name` MUST NOT 為空白
- `Name` MUST UNIQUE
- `InitialBalance` >= 0（初始餘額不可為負數）

**計算屬性（不存入資料庫）**:
- `CurrentBalance` (decimal) = `InitialBalance` + SUM(收入) - SUM(支出)，就該帳戶的所有非刪除交易

**預設種子資料**:

| 名稱 | Type | Icon | InitialBalance |
|------|------|------|----------------|
| 現金 | Cash | 💵 | 0 |
| 銀行帳戶 | Bank | 🏦 | 0 |
| 信用卡 | CreditCard | 💳 | 0 |

---

### Budget（預算）

| 欄位 | C# 型別 | SQLite 型別 | 限制 | 說明 |
|------|---------|------------|------|------|
| `Id` | `int` | INTEGER | PK, Auto-increment | 唯一識別碼 |
| `CategoryId` | `int` | INTEGER | FK → Category.Id, NOT NULL | 關聯支出分類 |
| `Amount` | `decimal` | TEXT | NOT NULL, > 0 | 預算金額 |
| `Period` | `BudgetPeriod` (enum) | INTEGER | NOT NULL, DEFAULT Monthly | 週期 |
| `StartDate` | `DateOnly` | TEXT | NOT NULL | 預算起始日 |
| `IsDeleted` | `bool` | INTEGER | NOT NULL, DEFAULT 0 | 軟刪除旗標 |
| `DeletedAt` | `DateTime?` | TEXT | NULLABLE | 刪除時間（UTC） |
| `CreatedAt` | `DateTime` | TEXT | NOT NULL | 建立時間（UTC） |
| `UpdatedAt` | `DateTime` | TEXT | NOT NULL | 最後更新時間（UTC） |

**BudgetPeriod 列舉**:
```csharp
public enum BudgetPeriod
{
    Monthly = 0,  // 月預算（V1 主要支援）
    Weekly = 1    // 週預算（加分項）
}
```

**驗證規則**:
- `Amount` MUST > 0
- `CategoryId` MUST 對應存在的**支出**分類（收入分類不設預算）
- 同一 `CategoryId` + `Period` 組合 SHOULD UNIQUE（避免重複設定）

**計算屬性（不存入資料庫）**:
- `SpentAmount` (decimal) = 該分類當前週期內的支出總和
- `UsageRate` (decimal) = `SpentAmount` / `Amount` * 100（百分比）
- `Status`: 
  - < 80% → **正常**（綠色）
  - 80% ~ 100% → **接近上限**（黃色，FR-018）
  - \> 100% → **超出預算**（紅色，FR-019）

**業務規則**:
- 新增支出紀錄後 MUST 自動檢查對應分類的預算狀態（FR-020）
- 新月份開始時，預算花費自動歸零重新計算（以當月交易為準，無需重置操作）

---

## 共用介面

### ISoftDeletable

```csharp
/// <summary>
/// Marker interface for entities that support soft delete.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
}
```

### IAuditable

```csharp
/// <summary>
/// Marker interface for entities with audit timestamps.
/// </summary>
public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
}
```

所有四個實體（Transaction、Category、Account、Budget）均實作 `ISoftDeletable` 與 `IAuditable` 介面。

---

## TransactionType 列舉

```csharp
public enum TransactionType
{
    Income = 0,   // 收入
    Expense = 1   // 支出
}
```

---

## EF Core DbContext 配置摘要

```csharp
public class BookKeepingDbContext : DbContext
{
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Budget> Budgets => Set<Budget>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Global Query Filter — 軟刪除
        modelBuilder.Entity<Transaction>().HasQueryFilter(t => !t.IsDeleted);
        modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
        modelBuilder.Entity<Account>().HasQueryFilter(a => !a.IsDeleted);
        modelBuilder.Entity<Budget>().HasQueryFilter(b => !b.IsDeleted);

        // decimal 精確度（SQLite TEXT 儲存）
        modelBuilder.Entity<Transaction>().Property(t => t.Amount).HasColumnType("TEXT");
        modelBuilder.Entity<Account>().Property(a => a.InitialBalance).HasColumnType("TEXT");
        modelBuilder.Entity<Budget>().Property(b => b.Amount).HasColumnType("TEXT");

        // 索引
        modelBuilder.Entity<Transaction>().HasIndex(t => t.Date).IsDescending();
        modelBuilder.Entity<Transaction>().HasIndex(t => t.CategoryId);
        modelBuilder.Entity<Transaction>().HasIndex(t => new { t.AccountId, t.Type });
        modelBuilder.Entity<Category>().HasIndex(c => new { c.Name, c.Type }).IsUnique();
        modelBuilder.Entity<Account>().HasIndex(a => a.Name).IsUnique();

        // 關聯配置
        modelBuilder.Entity<Transaction>()
            .HasOne<Category>().WithMany().HasForeignKey(t => t.CategoryId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Transaction>()
            .HasOne<Account>().WithMany().HasForeignKey(t => t.AccountId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Budget>()
            .HasOne<Category>().WithMany().HasForeignKey(b => b.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}
```

**關鍵配置說明**:
1. **OnDelete(Restrict)**: 防止意外串聯刪除，確保分類/帳戶被引用時不可硬刪除
2. **Global Query Filter**: 自動過濾 `IsDeleted = true` 的記錄
3. **decimal 用 TEXT**: SQLite 不原生支援 decimal，以 TEXT 儲存確保精確度（EF Core 自動轉換）
4. **索引策略**: 針對常用查詢模式（月報、篩選、餘額計算）建立索引
