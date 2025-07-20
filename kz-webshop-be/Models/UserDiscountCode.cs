using System.Text.Json.Serialization;

namespace kz_webshop_be.Models;
public class UserDiscountCode
{
    public Guid UserId { get; set; }

    public Guid DiscountCodeId { get; set; }

    public int UseCount { get; set; } = 0;

    [JsonIgnore]
    public DiscountCode? DiscountCode { get; set; }
    [JsonIgnore]
    public User? User { get; set; }
}