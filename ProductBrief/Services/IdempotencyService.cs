using Microsoft.EntityFrameworkCore;
using ProductBrief.Data;
using ProductBrief.Data.Repositories;
using ProductBrief.Models;

namespace ProductBrief.Services;

public class IdempotencyService(IIdempotencyKeyRepository idempotencyKeyRepository) : IIdempotencyService
{
    private readonly IIdempotencyKeyRepository _idempotencyKeyRepository = idempotencyKeyRepository;

    /// <summary>
    /// Processes idempotency for any request/response combination.
    /// </summary>
    /// <typeparam name="TResponse">The response type</typeparam>
    /// <param name="idempotencyKey">The idempotency key from the request header</param>
    /// <param name="requestBodyHash">The hash of the request body</param>
    /// <param name="getExistingResponseAsync">A callback to retrieve the existing response based on transaction ID</param>
    /// <returns>A Result with the existing response if the key was already processed, or null if this is a new request</returns>
    public async Task<Result<TResponse>?> ProcessIdempotencyAsync<TResponse>(string? idempotencyKey, string requestBodyHash, Func<IdempotencyResult, Task<TResponse?>> getExistingResponseAsync)
        where TResponse : class
    {
        // Reserve the idempotency key FIRST (prevents race condition via DB constraint)
        var idempotencyResult = await ReserveIdempotencyKeyAsync(idempotencyKey, requestBodyHash);

        // If same key is used with different body, return 409 Conflict
        if (idempotencyResult.IsConflict)
        {
            return new Result<TResponse> 
            { 
                Success = false, 
                Error = "An existing request with the same idempotency key was already processed with a different request body.", 
                HttpCode = "409" 
            };
        }

        // If key was already reserved by another request, return the existing response
        if (!idempotencyResult.IsNewRequest && idempotencyResult.ExistingTransactionId.HasValue)
        {
            var existingResponse = await getExistingResponseAsync(idempotencyResult);

            if (existingResponse != null)
            {
                return new Result<TResponse> 
                { 
                    Success = true, 
                    Error = null, 
                    HttpCode = "200", 
                    Data = existingResponse 
                };
            }
        }

        // If key was reserved but transaction not yet linked (still processing)
        if (!idempotencyResult.IsNewRequest && !idempotencyResult.ExistingTransactionId.HasValue)
        {
            return new Result<TResponse> 
            { 
                Success = false, 
                Error = "This request is already being processed. Please wait for the result.", 
                HttpCode = "409", 
                Data = null 
            };
        }

        // This is a new request, no idempotency result to return
        return null;
    }

    public async Task<IdempotencyResult> ReserveIdempotencyKeyAsync(string? idempotencyKey, string requestBodyHash)
    {
        // If no idempotency key is provided, treat it as a new request (no reservation)
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return new IdempotencyResult { IsNewRequest = true };
        }

        try
        {
            // Check if the idempotency key already exists
            var existingKey = await _idempotencyKeyRepository.GetByKeyAsync(idempotencyKey);

            if (existingKey != null)
            {
                // Key already exists - check if request body matches (same hash)
                if (existingKey.RequestBodyHash != requestBodyHash)
                {
                    // Different request body with same key - conflict
                    return new IdempotencyResult
                    {
                        IsNewRequest = false,
                        IsConflict = true
                    };
                }

                // Same key with same body - this is a duplicate request (idempotent)
                return new IdempotencyResult
                {
                    IsNewRequest = false,
                    IsConflict = false,
                    ExistingTransactionId = existingKey.TransactionId == Guid.Empty ? null : existingKey.TransactionId
                };
            }

            // Reserve the key with the request body hash
            var reservedKey = new IdempotencyKey
            {
                Key = idempotencyKey,
                TransactionId = Guid.Empty, // Placeholder, will be updated after transaction creation
                RequestBodyHash = requestBodyHash,
                CreatedAt = DateTime.UtcNow
            };

            await _idempotencyKeyRepository.CreateAsync(reservedKey);

            return new IdempotencyResult { IsNewRequest = true };
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE") ?? false)
        {
            // Unique constraint violation - another request reserved this key first
            // Wait a bit and try to retrieve the key with its transaction ID
            await Task.Delay(100); // Give the other request time to link the transaction

            var existingKey = await _idempotencyKeyRepository.GetByKeyAsync(idempotencyKey);

            if (existingKey != null)
            {
                // Check if request body matches
                if (existingKey.RequestBodyHash != requestBodyHash)
                {
                    // Different request body with same key - conflict
                    return new IdempotencyResult
                    {
                        IsNewRequest = false,
                        IsConflict = true
                    };
                }

                // Same key with same body
                if (existingKey.TransactionId != Guid.Empty)
                {
                    return new IdempotencyResult
                    {
                        IsNewRequest = false,
                        IsConflict = false,
                        ExistingTransactionId = existingKey.TransactionId
                    };
                }
            }

            // Key exists but transaction not yet linked - still processing
            // Return as not a new request, but without transaction ID (caller should retry)
            return new IdempotencyResult { IsNewRequest = false, IsConflict = false };
        }
    }

    /// <summary>
    /// Links the created transaction to its reserved idempotency key.
    /// This is called after the transaction is successfully created.
    /// </summary>
    public async Task LinkTransactionToKeyAsync(string idempotencyKey, Guid transactionId)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return;
        }

        await _idempotencyKeyRepository.UpdateTransactionIdAsync(idempotencyKey, transactionId);
    }
}
