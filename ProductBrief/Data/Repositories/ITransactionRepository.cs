using ProductBrief.Models;

namespace ProductBrief.Data.Repositories;

/// <summary>
/// Repository abstraction for PurchaseTransaction data access operations.
/// Provides loose coupling from the underlying database context.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Retrieves a purchase transaction by its ID.
    /// </summary>
    Task<PurchaseTransaction?> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates a new purchase transaction and saves it to the database.
    /// </summary>
    Task<PurchaseTransaction> CreateAsync(PurchaseTransaction transaction);

    /// <summary>
    /// Checks if a transaction with the given ID exists.
    /// </summary>
    Task<bool> ExistsAsync(Guid id);
}
