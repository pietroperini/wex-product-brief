using Microsoft.EntityFrameworkCore;
using ProductBrief.Models;

namespace ProductBrief.Data.Repositories;

/// <summary>
/// Repository implementation for IdempotencyKey data access operations.
/// </summary>
public class IdempotencyKeyRepository : IIdempotencyKeyRepository
{
    private readonly PurchaseTransactionDbContext _dbContext;

    public IdempotencyKeyRepository(PurchaseTransactionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IdempotencyKey?> GetByKeyAsync(string key)
    {
        return await _dbContext.IdempotencyKeys
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Key == key);
    }

    public async Task<IdempotencyKey> CreateAsync(IdempotencyKey idempotencyKey)
    {
        _dbContext.IdempotencyKeys.Add(idempotencyKey);
        await _dbContext.SaveChangesAsync();
        return idempotencyKey;
    }

    public async Task<bool> UpdateTransactionIdAsync(string key, Guid transactionId)
    {
        var idempotencyKey = await _dbContext.IdempotencyKeys
            .FirstOrDefaultAsync(k => k.Key == key);

        if (idempotencyKey == null)
        {
            return false;
        }

        idempotencyKey.TransactionId = transactionId;
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
