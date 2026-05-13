using ProductBrief.Models;

namespace ProductBrief.Data.Repositories;

/// <summary>
/// Repository implementation for PurchaseTransaction data access operations.
/// </summary>
public class TransactionRepository : ITransactionRepository
{
    private readonly PurchaseTransactionDbContext _dbContext;

    public TransactionRepository(PurchaseTransactionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PurchaseTransaction?> GetByIdAsync(Guid id)
    {
        return await _dbContext.PurchaseTransactions.FindAsync(id);
    }

    public async Task<PurchaseTransaction> CreateAsync(PurchaseTransaction transaction)
    {
        _dbContext.PurchaseTransactions.Add(transaction);
        await _dbContext.SaveChangesAsync();
        return transaction;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var transaction = await _dbContext.PurchaseTransactions.FindAsync(id);
        return transaction != null;
    }
}
