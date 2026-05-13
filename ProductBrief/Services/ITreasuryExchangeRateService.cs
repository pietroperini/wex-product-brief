namespace ProductBrief.Services;
public interface ITreasuryExchangeRateService
{
    Task<decimal> GetExchangeRateAsync(string currencyCode, DateTime transactionDate);
}