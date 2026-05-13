using Microsoft.AspNetCore.Mvc;
using ProductBrief.Models;

namespace ProductBrief.Services;

public interface IPurchaseTransactionService
{
    Task<PurchaseTransactionResponse> CreateTransaction(CreatePurchaseTransactionRequest request);
    Task<PurchaseTransactionInCurrencyResponse> GetTransactionInCurrency(Guid id, string currencyCode);
}
