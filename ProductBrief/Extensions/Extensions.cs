using ProductBrief.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ProductBrief.Extensions;

public static class Extensions
{
    public static string ToJsonString<T>(this T obj)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });
    }

    public static string ComputeRequestBodyHash(this string obj)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(obj));
            return Convert.ToBase64String(hashedBytes);
        }
    }
    public static PurchaseTransactionResponse MapToResponse(this PurchaseTransaction transaction)
    {
        return new PurchaseTransactionResponse
        {
            Id = transaction.Id,
            Description = transaction.Description,
            TransactionDate = transaction.TransactionDate,
            PurchaseAmount = transaction.PurchaseAmount
        };
    }
}

