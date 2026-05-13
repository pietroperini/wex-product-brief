using ProductBrief.Models;
using ProductBrief.Models.Validators;

namespace ProductBrief.Tests.Validators;

public class CreatePurchaseTransactionValidatorTests
{
    private readonly CreatePurchaseTransactionValidator _validator = new();

    [Fact]
    public void Validate_WithValidRequest_ShouldPass()
    {
        // Arrange
        var request = new CreatePurchaseTransactionRequest
        {
            Description = "Valid Purchase",
            TransactionDate = DateTime.UtcNow.AddDays(-1),
            PurchaseAmount = 100.50m
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyDescription_ShouldFail()
    {
        // Arrange
        var request = new CreatePurchaseTransactionRequest
        {
            Description = string.Empty,
            TransactionDate = DateTime.UtcNow.AddDays(-1),
            PurchaseAmount = 100.50m
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Description is required"));
    }

    [Fact]
    public void Validate_WithDescriptionExceeding50Characters_ShouldFail()
    {
        // Arrange
        var request = new CreatePurchaseTransactionRequest
        {
            Description = new string('A', 51),
            TransactionDate = DateTime.UtcNow.AddDays(-1),
            PurchaseAmount = 100.50m
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("must not exceed 50 characters"));
    }

    [Fact]
    public void Validate_WithFutureTransactionDate_ShouldFail()
    {
        // Arrange
        var request = new CreatePurchaseTransactionRequest
        {
            Description = "Valid Purchase",
            TransactionDate = DateTime.UtcNow.AddDays(1),
            PurchaseAmount = 100.50m
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("cannot be in the future"));
    }

    [Fact]
    public void Validate_WithNegativePurchaseAmount_ShouldFail()
    {
        // Arrange
        var request = new CreatePurchaseTransactionRequest
        {
            Description = "Valid Purchase",
            TransactionDate = DateTime.UtcNow.AddDays(-1),
            PurchaseAmount = -100.50m
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("valid positive amount"));
    }

    [Fact]
    public void Validate_WithZeroPurchaseAmount_ShouldFail()
    {
        // Arrange
        var request = new CreatePurchaseTransactionRequest
        {
            Description = "Valid Purchase",
            TransactionDate = DateTime.UtcNow.AddDays(-1),
            PurchaseAmount = 0m
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithPurchaseAmountNotRoundedToCent_ShouldFail()
    {
        // Arrange
        var request = new CreatePurchaseTransactionRequest
        {
            Description = "Valid Purchase",
            TransactionDate = DateTime.UtcNow.AddDays(-1),
            PurchaseAmount = 100.505m
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("rounded to the nearest cent"));
    }

    [Fact]
    public void Validate_WithMaxLengthDescription_ShouldPass()
    {
        // Arrange
        var request = new CreatePurchaseTransactionRequest
        {
            Description = new string('A', 50),
            TransactionDate = DateTime.UtcNow.AddDays(-1),
            PurchaseAmount = 100.50m
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithPastTransactionDate_ShouldPass()
    {
        // Arrange
        var request = new CreatePurchaseTransactionRequest
        {
            Description = "Valid Purchase",
            TransactionDate = DateTime.UtcNow.AddDays(-1),
            PurchaseAmount = 100.50m
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1.00)]
    [InlineData(999.99)]
    [InlineData(1000000.00)]
    public void Validate_WithValidRoundedAmounts_ShouldPass(decimal amount)
    {
        // Arrange
        var request = new CreatePurchaseTransactionRequest
        {
            Description = "Valid Purchase",
            TransactionDate = DateTime.UtcNow.AddDays(-1),
            PurchaseAmount = amount
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }
}
