using ProductBrief.Models;

namespace ProductBrief.Services;

public interface IIdempotencyService
{
    Task<IdempotencyResult> ReserveIdempotencyKeyAsync(string? idempotencyKey, string requestBodyHash);

    Task LinkTransactionToKeyAsync(string idempotencyKey, Guid transactionId);

    Task<Result<TResponse>?> ProcessIdempotencyAsync<TResponse>(string? idempotencyKey, string requestBodyHash, Func<IdempotencyResult, Task<TResponse?>> getExistingResponseAsync)
        where TResponse : class;
}
