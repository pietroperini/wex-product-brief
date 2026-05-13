namespace ProductBrief.Models;

public class IdempotencyKey
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public Guid TransactionId { get; set; }
    public string RequestBodyHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
