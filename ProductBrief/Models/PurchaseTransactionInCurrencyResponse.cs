namespace ProductBrief.Models;

public class PurchaseTransactionInCurrencyResponse
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public decimal OriginalAmount { get; set; }
    public string OriginalCurrency { get; set; } = "USD";
    public decimal ConvertedAmount { get; set; }
    public string TargetCurrency { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
}
