using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductBrief.Data;
using ProductBrief.Facade;
using ProductBrief.Models;

namespace ProductBrief.Controllers;


[ApiController]
[Route("api/[controller]")]
public class PurchaseTransactionController(
    IValidator<CreatePurchaseTransactionRequest> validator,
    IPurchaseTransactionFacade purchaseTransactionFacade,
    ILogger<PurchaseTransactionController> logger) : ControllerBase
{
    private readonly IValidator<CreatePurchaseTransactionRequest> _validator = validator;
    private readonly IPurchaseTransactionFacade _purchaseTransactionFacade = purchaseTransactionFacade;
    private readonly ILogger<PurchaseTransactionController> _logger = logger;

    /// <summary>
    /// Create a new purchase transaction
    /// </summary>
    /// <remarks>
    /// Accepts an optional Idempotency-Key header to prevent duplicate transactions.
    /// If the same key is sent multiple times, the same transaction will be returned.
    /// Race condition safe: the key is reserved immediately using database UNIQUE constraint.
    /// </remarks>
    [HttpPost]
    public async Task<ActionResult<PurchaseTransactionResponse>> CreateTransaction(CreatePurchaseTransactionRequest request, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
    {
        // Validate the request
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }
        try
        {
            var result = await _purchaseTransactionFacade.CreatePurchaseTransactionAsync(request, idempotencyKey);

            if (!result.Success)
            {
                int statusCode = result.HttpCode switch
                {
                    "400" => 400,
                    "409" => 409,                    
                    _ => 500
                };
                return StatusCode(statusCode, new { Message = result.Error });
            }

            return Created(string.Empty, result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating purchase transaction");
            return StatusCode(500, new { Message = "An error occurred while processing your request." });
        }
    }

    /// <summary>
    /// Get a purchase transaction converted to a specified currency
    /// </summary>
    [HttpGet("{id}/currency/{countryCurrencyDesc}")]
    public async Task<ActionResult<PurchaseTransactionInCurrencyResponse>> GetTransactionInCurrency(Guid id, string countryCurrencyDesc)
    {

        if (string.IsNullOrWhiteSpace(countryCurrencyDesc) || !countryCurrencyDesc.Contains('-'))
        {
            return BadRequest(new { message = "Currency must be contain the country-currency" });
        }
        try
        {
            var result = await _purchaseTransactionFacade.GetPurchaseTransactionInCurrencyAsync(id, countryCurrencyDesc);

            if (!result.Success)
            {
                int statusCode = result.HttpCode switch
                {
                    "400" => 400,
                    "404" => 404,                    
                    _ => 500
                };
                return StatusCode(statusCode, new { Message = result.Error });
            }

            return Ok(result.Data);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving purchase transaction in specific currency");
            return StatusCode(500, new { Message = "An error occurred while processing your request." });
        }

    }
}
