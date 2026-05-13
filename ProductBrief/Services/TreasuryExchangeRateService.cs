using Microsoft.Extensions.Options;
using ProductBrief.Configurations;
using ProductBrief.Models;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;

namespace ProductBrief.Services;

public class TreasuryExchangeRateService(IHttpClientFactory httpClientFactory, IOptions<TreasuryApiSettings> settings, JsonSerializerOptions options) : ITreasuryExchangeRateService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private const string HttpClientName = "TreasuryApi";
    private readonly TreasuryApiSettings _settings = settings.Value;
    private readonly ConcurrentDictionary<string, CacheEntry<decimal>> _exchangeRateCache = new();

    public async Task<decimal> GetExchangeRateAsync(string currencyCode, DateTime transactionDate)
    {
        try
        {
            // Create cache key from currency code and transaction date
            var cacheKey = $"{currencyCode}_{transactionDate:yyyy-MM-dd}";

            // Check if rate exists in cache and is not expired
            if (_exchangeRateCache.TryGetValue(cacheKey, out var cachedEntry) && !cachedEntry.IsExpired)
            {
                return cachedEntry.Value;
            }

            // Calculate the date range: transaction date to lookbackMoths before
            var minDate = transactionDate.AddMonths(-_settings.LookbackMonths);
            var formattedTransactionDate = transactionDate.ToString("yyyy-MM-dd");
            var formattedMinDate = minDate.ToString("yyyy-MM-dd");
            
            var filter = $"country_currency_desc:eq:{currencyCode},effective_date:lte:{formattedTransactionDate},effective_date:gte:{formattedMinDate}";
            var url = $"{_settings.BaseUrl}?filter={filter}&sort=-effective_date&page[size]=1";

            var httpClient = _httpClientFactory.CreateClient(HttpClientName);
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var jsonString = await response.Content.ReadAsStringAsync();
            var content = JsonSerializer.Deserialize<TreasuryExchangeRateResponse>(jsonString, options);

            if (content?.Data != null && content.Data.Count > 0)
            {
                // Get the most recent rate within the lookback period
                var rate = decimal.Parse(content.Data[0].Exchange_Rate, CultureInfo.InvariantCulture);

                // Store in cache
                _exchangeRateCache[cacheKey] = new CacheEntry<decimal>
                {
                    Value = rate,
                    ExpirationTime = DateTime.UtcNow.AddMinutes(_settings.CacheTtlMinutes)
                };

                return rate;
            }

            throw new InvalidOperationException(
                $"No exchange rate found for {currencyCode} within {_settings.LookbackMonths} months before {formattedTransactionDate}. " +
                $"Purchase cannot be converted to the target currency.");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("No exchange rate found"))
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to retrieve exchange rate from Treasury API for {currencyCode} on {transactionDate:yyyy-MM-dd}: {ex.Message}", ex);
        }
    }
}
