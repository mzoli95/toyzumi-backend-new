
using System.Text.Json.Serialization;
using kz_webshop_be.Models;

public class DiscountCode
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public bool IsPercentage { get; set; }
    public DateTime ExpirationDate { get; set; }

    [JsonIgnore]
    public List<UserDiscountCode> UserDiscountCodes { get; set; } = new();
}
