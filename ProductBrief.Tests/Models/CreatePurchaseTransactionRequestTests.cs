using ProductBrief.Models;

namespace ProductBrief.Tests.Models;

public class CreatePurchaseTransactionRequestTests
{
    [Fact]
    public void CreatePurchaseTransactionRequest_CanBeInstantiated()
    {
        // Arrange & Act
        var request = new CreatePurchaseTransactionRequest();

        // Assert
        Assert.NotNull(request);
        Assert.Equal(string.Empty, request.Description);
    }

    [Fact]
    public void CreatePurchaseTransactionRequest_PropertiesCanBeSet()
    {
        // Arrange
        var request = new CreatePurchaseTransactionRequest();
        var now = DateTime.UtcNow;
        const decimal amount = 150.75m;
        const string description = "Test Transaction";

        // Act
        request.Description = description;
        request.TransactionDate = now;
        request.PurchaseAmount = amount;

        // Assert
        Assert.Equal(description, request.Description);
        Assert.Equal(now, request.TransactionDate);
        Assert.Equal(amount, request.PurchaseAmount);
    }

    [Fact]
    public void CreatePurchaseTransactionRequest_CanBeCreatedWithInitializer()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var request = new CreatePurchaseTransactionRequest
        {
            Description = "Test",
            TransactionDate = now,
            PurchaseAmount = 99.99m
        };

        // Assert
        Assert.Equal("Test", request.Description);
        Assert.Equal(now, request.TransactionDate);
        Assert.Equal(99.99m, request.PurchaseAmount);
    }
}
