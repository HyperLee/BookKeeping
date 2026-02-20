using Microsoft.EntityFrameworkCore;

using BookKeeping.Models;

namespace BookKeeping.Data;

/// <summary>
/// Entity Framework Core database context for BookKeeping application.
/// </summary>
public class BookKeepingDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BookKeepingDbContext"/> class.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    public BookKeepingDbContext(DbContextOptions<BookKeepingDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the transactions DbSet.
    /// </summary>
    public DbSet<Transaction> Transactions => Set<Transaction>();

    /// <summary>
    /// Gets or sets the categories DbSet.
    /// </summary>
    public DbSet<Category> Categories => Set<Category>();

    /// <summary>
    /// Gets or sets the accounts DbSet.
    /// </summary>
    public DbSet<Account> Accounts => Set<Account>();

    /// <summary>
    /// Gets or sets the budgets DbSet.
    /// </summary>
    public DbSet<Budget> Budgets => Set<Budget>();

    /// <summary>
    /// Configures the database model.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global Query Filters — soft delete
        modelBuilder.Entity<Transaction>().HasQueryFilter(t => !t.IsDeleted);
        modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
        modelBuilder.Entity<Account>().HasQueryFilter(a => !a.IsDeleted);
        modelBuilder.Entity<Budget>().HasQueryFilter(b => !b.IsDeleted);

        // decimal precision (SQLite TEXT storage)
        modelBuilder.Entity<Transaction>().Property(t => t.Amount).HasColumnType("TEXT");
        modelBuilder.Entity<Account>().Property(a => a.InitialBalance).HasColumnType("TEXT");
        modelBuilder.Entity<Budget>().Property(b => b.Amount).HasColumnType("TEXT");

        // Indexes
        modelBuilder.Entity<Transaction>().HasIndex(t => t.Date).IsDescending();
        modelBuilder.Entity<Transaction>().HasIndex(t => t.CategoryId);
        modelBuilder.Entity<Transaction>().HasIndex(t => new { t.AccountId, t.Type });
        modelBuilder.Entity<Category>().HasIndex(c => new { c.Name, c.Type }).IsUnique();
        modelBuilder.Entity<Account>().HasIndex(a => a.Name).IsUnique();

        // Foreign Key relationships with Restrict delete behavior
        modelBuilder.Entity<Transaction>()
            .HasOne<Category>()
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transaction>()
            .HasOne<Account>()
            .WithMany()
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Budget>()
            .HasOne<Category>()
            .WithMany()
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// Automatically handles soft delete and audit timestamps.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var entries = ChangeTracker.Entries();

        foreach (var entry in entries)
        {
            // Handle soft delete
            if (entry.State == EntityState.Deleted && entry.Entity is ISoftDeletable softDeletable)
            {
                entry.State = EntityState.Modified;
                softDeletable.IsDeleted = true;
                softDeletable.DeletedAt = now;
            }

            // Handle audit timestamps
            if (entry.Entity is IAuditable auditable)
            {
                if (entry.State == EntityState.Added)
                {
                    auditable.CreatedAt = now;
                    auditable.UpdatedAt = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    auditable.UpdatedAt = now;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
