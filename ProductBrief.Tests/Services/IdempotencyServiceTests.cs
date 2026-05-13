using ProductBrief.Data.Repositories;
using ProductBrief.Models;
using ProductBrief.Services;

namespace ProductBrief.Tests.Services;

public class IdempotencyServiceTests
{
    private readonly IdempotencyService _service;
    private readonly MockIdempotencyKeyRepository _mockRepository;

    public IdempotencyServiceTests()
    {
        _mockRepository = new MockIdempotencyKeyRepository();
        _service = new IdempotencyService(_mockRepository);
    }

    [Fact]
    public async Task ProcessIdempotencyAsync_WithNoIdempotencyKey_ReturnsNull()
    {
        // Arrange
        string? idempotencyKey = null;
        var requestBodyHash = "hash123";

        // Act
        var result = await _service.ProcessIdempotencyAsync<string>(idempotencyKey, requestBodyHash, _ => Task.FromResult<string?>(null));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ProcessIdempotencyAsync_WithEmptyIdempotencyKey_ReturnsNull()
    {
        // Arrange
        var idempotencyKey = string.Empty;
        var requestBodyHash = "hash123";

        // Act
        var result = await _service.ProcessIdempotencyAsync<string>(idempotencyKey, requestBodyHash, _ => Task.FromResult<string?>(null));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ProcessIdempotencyAsync_WithConflictingRequest_ReturnsConflictError()
    {
        // Arrange
        var idempotencyKey = "key123";
        var requestBodyHash = "hash123";
        var differentHash = "hash456";
        var transactionId = Guid.NewGuid();

        var existingKey = new IdempotencyKey
        {
            Key = idempotencyKey,
            RequestBodyHash = differentHash,
            TransactionId = transactionId
        };

        _mockRepository.SetExistingKey(existingKey);

        // Act
        var result = await _service.ProcessIdempotencyAsync<string>(
            idempotencyKey,
            requestBodyHash,
            _ => Task.FromResult<string?>(null)
        );

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("409", result.HttpCode);
        Assert.Contains("different request body", result.Error);
    }

    [Fact]
    public async Task ProcessIdempotencyAsync_WithExistingKeyAndMatchingHash_ReturnsExistingResponse()
    {
        // Arrange
        var idempotencyKey = "key123";
        var requestBodyHash = "hash123";
        var expectedResponse = "existing response";
        var transactionId = Guid.NewGuid();

        var existingKey = new IdempotencyKey
        {
            Key = idempotencyKey,
            RequestBodyHash = requestBodyHash,
            TransactionId = transactionId
        };

        _mockRepository.SetExistingKey(existingKey);

        // Act
        var result = await _service.ProcessIdempotencyAsync(
            idempotencyKey,
            requestBodyHash,
            async idempotencyResult => 
            {
                await Task.Delay(0); // Simulate async operation
                return expectedResponse;
            }
        );

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("200", result.HttpCode);
        Assert.Equal(expectedResponse, result.Data);
    }

    [Fact]
    public async Task ReserveIdempotencyKeyAsync_WithNoKey_ReturnsNewRequest()
    {
        // Arrange
        string? idempotencyKey = null;
        var requestBodyHash = "hash123";

        // Act
        var result = await _service.ReserveIdempotencyKeyAsync(idempotencyKey, requestBodyHash);

        // Assert
        Assert.True(result.IsNewRequest);
        Assert.False(result.IsConflict);
    }

    [Fact]
    public async Task ReserveIdempotencyKeyAsync_WithNewKey_CreatesAndReturnsNewRequest()
    {
        // Arrange
        var idempotencyKey = "newkey123";
        var requestBodyHash = "hash123";

        // Act
        var result = await _service.ReserveIdempotencyKeyAsync(idempotencyKey, requestBodyHash);

        // Assert
        Assert.True(result.IsNewRequest);
        Assert.False(result.IsConflict);
    }

    [Fact]
    public async Task ReserveIdempotencyKeyAsync_WithExistingKeyAndDifferentHash_ReturnsConflict()
    {
        // Arrange
        var idempotencyKey = "existingkey";
        var requestBodyHash = "hash123";
        var storedHash = "hash456";
        var transactionId = Guid.NewGuid();

        var existingKey = new IdempotencyKey
        {
            Key = idempotencyKey,
            RequestBodyHash = storedHash,
            TransactionId = transactionId
        };

        _mockRepository.SetExistingKey(existingKey);

        // Act
        var result = await _service.ReserveIdempotencyKeyAsync(idempotencyKey, requestBodyHash);

        // Assert
        Assert.False(result.IsNewRequest);
        Assert.True(result.IsConflict);
    }

    [Fact]
    public async Task ReserveIdempotencyKeyAsync_WithExistingKeyAndMatchingHash_ReturnsExistingRequest()
    {
        // Arrange
        var idempotencyKey = "existingkey";
        var requestBodyHash = "hash123";
        var transactionId = Guid.NewGuid();

        var existingKey = new IdempotencyKey
        {
            Key = idempotencyKey,
            RequestBodyHash = requestBodyHash,
            TransactionId = transactionId
        };

        _mockRepository.SetExistingKey(existingKey);

        // Act
        var result = await _service.ReserveIdempotencyKeyAsync(idempotencyKey, requestBodyHash);

        // Assert
        Assert.False(result.IsNewRequest);
        Assert.False(result.IsConflict);
        Assert.Equal(transactionId, result.ExistingTransactionId);
    }
}

/// <summary>
/// Mock implementation of IIdempotencyKeyRepository for testing
/// </summary>
internal class MockIdempotencyKeyRepository : IIdempotencyKeyRepository
{
    private IdempotencyKey? _existingKey;
    private readonly List<IdempotencyKey> _createdKeys = [];

    public void SetExistingKey(IdempotencyKey key)
    {
        _existingKey = key;
    }

    public async Task<IdempotencyKey?> GetByKeyAsync(string key)
    {
        await Task.Delay(0);
        return _existingKey?.Key == key ? _existingKey : null;
    }

    public async Task<IdempotencyKey> CreateAsync(IdempotencyKey idempotencyKey)
    {
        await Task.Delay(0);
        _createdKeys.Add(idempotencyKey);
        return idempotencyKey;
    }

    public async Task<bool> UpdateTransactionIdAsync(string key, Guid transactionId)
    {
        await Task.Delay(0);
        if (_existingKey?.Key == key)
        {
            _existingKey.TransactionId = transactionId;
            return true;
        }
        return false;
    }
}
