namespace ProductBrief.Models;

public class PurchaseTransaction
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public decimal PurchaseAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}
