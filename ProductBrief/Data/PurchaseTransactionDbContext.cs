using Microsoft.EntityFrameworkCore;
using ProductBrief.Models;
using System.Reflection.Emit;

namespace ProductBrief.Data;

public class PurchaseTransactionDbContext(DbContextOptions<PurchaseTransactionDbContext> options) : DbContext(options)
{
    public DbSet<PurchaseTransaction> PurchaseTransactions => Set<PurchaseTransaction>();
    public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PurchaseTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)                
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.TransactionDate)
                .IsRequired();

            entity.Property(e => e.PurchaseAmount)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.TransactionDate);
        });

        modelBuilder.Entity<IdempotencyKey>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)                
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Key)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.TransactionId)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            entity.HasIndex(e => e.Key)
                .IsUnique();

            entity.HasIndex(e => e.CreatedAt);
        });
    }
}

