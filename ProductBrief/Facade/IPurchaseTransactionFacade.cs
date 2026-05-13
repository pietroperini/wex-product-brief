using Microsoft.AspNetCore.Mvc;
using ProductBrief.Models;

namespace ProductBrief.Facade;

public interface IPurchaseTransactionFacade
{
    Task<Result<PurchaseTransactionResponse>> CreatePurchaseTransactionAsync(CreatePurchaseTransactionRequest request, string? idempotencyKey);
    Task<Result<PurchaseTransactionInCurrencyResponse>> GetPurchaseTransactionInCurrencyAsync(Guid id, string countryCurrencyDesc);
}
