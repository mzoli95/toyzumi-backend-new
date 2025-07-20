using System.Text.Json.Serialization;

namespace kz_webshop_be.Models;

public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime OrderedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }

    [JsonIgnore]
    public User User { get; set; }
    [JsonIgnore]
    public Invoice Invoice { get; set; }
    [JsonIgnore]
    public List<OrderItem> Items { get; set; } = new();
}