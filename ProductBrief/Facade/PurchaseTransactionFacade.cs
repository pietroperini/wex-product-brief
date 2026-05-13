using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductBrief.Data;
using ProductBrief.Data.Repositories;
using ProductBrief.Extensions;
using ProductBrief.Models;
using ProductBrief.Services;
using System.Text.Json;

namespace ProductBrief.Facade;

public class PurchaseTransactionFacade(IPurchaseTransactionService purchaseTransactionService, IIdempotencyService idempotencyService, IValidator<CreatePurchaseTransactionRequest> validator, ITransactionRepository transactionRepository) : IPurchaseTransactionFacade
{    
    private readonly IPurchaseTransactionService _purchaseTransactionService = purchaseTransactionService;
    private readonly IIdempotencyService _idempotencyService = idempotencyService;
    private readonly IValidator<CreatePurchaseTransactionRequest> _validator = validator;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;

    public async Task<Result<PurchaseTransactionResponse>> CreatePurchaseTransactionAsync(CreatePurchaseTransactionRequest request, string? idempotencyKey)
    {
        //Process the Idempotency key, if provided this will check if there's an existing transaction linked to the key and return it, otherwise it will reserve the key for the new transaction we're about to create.
        var existingResult = await _idempotencyService.ProcessIdempotencyAsync<PurchaseTransactionResponse>(
            idempotencyKey,
            request.ToJsonString().ComputeRequestBodyHash(),
            async (result) =>
            {
                // This callback retrieves the existing response based on transaction ID
                var transaction = await _transactionRepository.GetByIdAsync(result.ExistingTransactionId!.Value);
                return transaction?.MapToResponse();
            }
        );

        // If the idempotency service already has a stored response, return it directly.
        if (existingResult != null)
        {
            return existingResult;
        }

        //Validate the request
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return new Result<PurchaseTransactionResponse> { Success = false, Error = "Validation failed: " + string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)), HttpCode = "400"};            
        }

        var response = await _purchaseTransactionService.CreateTransaction(request);

        //Link the transaction to the reserved idempotency key
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await _idempotencyService.LinkTransactionToKeyAsync(idempotencyKey, response.Id);
        }

        return new Result<PurchaseTransactionResponse> { Success = true, Error = null, HttpCode = "201", Data = response };        
    }

    public async Task<Result<PurchaseTransactionInCurrencyResponse>> GetPurchaseTransactionInCurrencyAsync(Guid id, string countryCurrencyDesc)
    {
        try
        {
            var response = await _purchaseTransactionService.GetTransactionInCurrency(id, countryCurrencyDesc);
            return new Result<PurchaseTransactionInCurrencyResponse> { Success = true, Error = null, HttpCode = "200", Data = response };
        }
        catch (Exception ex)
        {
            return new Result<PurchaseTransactionInCurrencyResponse> { Success = false, Error = ex.Message, HttpCode = "404", Data = null };
        }
    }


}

