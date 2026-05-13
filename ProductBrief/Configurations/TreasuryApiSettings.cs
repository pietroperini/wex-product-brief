namespace ProductBrief.Configurations;

public class TreasuryApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public int LookbackMonths { get; set; }
    public int CacheTtlMinutes { get; set; }
}
