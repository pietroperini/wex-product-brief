namespace ProductBrief.Models;

public class CacheEntry<T>
{
    public T Value { get; set; } = default!;
    public DateTime ExpirationTime { get; set; }
    public bool IsExpired => DateTime.UtcNow > ExpirationTime;
}
