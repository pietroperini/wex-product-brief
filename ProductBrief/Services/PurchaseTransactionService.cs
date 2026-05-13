using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductBrief.Data;
using ProductBrief.Data.Repositories;
using ProductBrief.Models;

namespace ProductBrief.Services;

public class PurchaseTransactionService(ITransactionRepository transactionRepository, ITreasuryExchangeRateService treasuryExchangeRateService, ILogger<PurchaseTransactionService> logger) : IPurchaseTransactionService
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly ITreasuryExchangeRateService _treasuryExchangeRateService = treasuryExchangeRateService;        
    private readonly ILogger<PurchaseTransactionService> _logger = logger;

    public async Task<PurchaseTransactionResponse> CreateTransaction(CreatePurchaseTransactionRequest request)
    {
        //Create the transaction
        var transaction = new PurchaseTransaction
        {
            Description = request.Description,
            TransactionDate = request.TransactionDate,
            PurchaseAmount = Math.Round(request.PurchaseAmount, 2),
            CreatedAt = DateTime.UtcNow
        };

        var createdTransaction = await _transactionRepository.CreateAsync(transaction);

        return new PurchaseTransactionResponse
        {
            Id = createdTransaction.Id,
            Description = createdTransaction.Description,
            TransactionDate = createdTransaction.TransactionDate,
            PurchaseAmount = createdTransaction.PurchaseAmount
        };

    }

    public async Task<PurchaseTransactionInCurrencyResponse> GetTransactionInCurrency(Guid id, string countryCurrencyDesc)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id) ?? throw (new Exception("Purchase transaction not found."));

        if (countryCurrencyDesc.Equals("US-Dollar", StringComparison.OrdinalIgnoreCase))
        {
            return PurchaseTransactionInCurrencyResponseBuilder(transaction,null);
        }

        try
        {
            var exchangeRate = await _treasuryExchangeRateService.GetExchangeRateAsync(countryCurrencyDesc, transaction.TransactionDate);

            var convertedAmount = Math.Round(transaction.PurchaseAmount * exchangeRate, 2);

            return PurchaseTransactionInCurrencyResponseBuilder(transaction, convertedAmount, countryCurrencyDesc, exchangeRate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving exchange rate");
            throw;
        }
    }

    private static PurchaseTransactionInCurrencyResponse PurchaseTransactionInCurrencyResponseBuilder(PurchaseTransaction transaction, decimal? convertedAmount, string targetCurrency = "US-Dollar", decimal exchangeRate = 1m) 
    {
        var usdResponse = new PurchaseTransactionInCurrencyResponse
        {
            Id = transaction.Id,
            Description = transaction.Description,
            TransactionDate = transaction.TransactionDate,
            OriginalAmount = transaction.PurchaseAmount,
            OriginalCurrency = "US-Dollar",
            ConvertedAmount = convertedAmount ?? transaction.PurchaseAmount,
            TargetCurrency = targetCurrency,
            ExchangeRate = exchangeRate
        };
        return usdResponse;
    }
}
