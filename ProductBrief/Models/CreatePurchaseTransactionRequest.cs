namespace ProductBrief.Models;

public class CreatePurchaseTransactionRequest
{
    public string Description { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public decimal PurchaseAmount { get; set; }
}
