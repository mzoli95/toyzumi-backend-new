using System.Text.Json.Serialization;
using kz_webshop_be.Enums;

namespace kz_webshop_be.Models;

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public ProductType ProductType { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    [JsonIgnore]
    public Order Order { get; set; }
}