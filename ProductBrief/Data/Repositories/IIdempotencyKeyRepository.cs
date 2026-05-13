using ProductBrief.Models;

namespace ProductBrief.Data.Repositories;

/// <summary>
/// Repository abstraction for IdempotencyKey data access operations.
/// Provides loose coupling from the underlying database context.
/// </summary>
public interface IIdempotencyKeyRepository
{
    /// <summary>
    /// Retrieves an idempotency key by its key string.
    /// </summary>
    Task<IdempotencyKey?> GetByKeyAsync(string key);

    /// <summary>
    /// Creates a new idempotency key reservation.
    /// </summary>
    Task<IdempotencyKey> CreateAsync(IdempotencyKey idempotencyKey);

    /// <summary>
    /// Updates an existing idempotency key with the transaction ID.
    /// </summary>
    Task<bool> UpdateTransactionIdAsync(string key, Guid transactionId);
}
