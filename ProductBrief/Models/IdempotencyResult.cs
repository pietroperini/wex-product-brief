namespace ProductBrief.Models;

public class IdempotencyResult
{
    public bool IsNewRequest { get; set; }
    public Guid? ExistingTransactionId { get; set; }
    public bool IsConflict { get; set; }
}
