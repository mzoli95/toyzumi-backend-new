using System.Text.Json.Serialization;
using kz_webshop_be.Enums;

namespace kz_webshop_be.Models;

public class LikedItem
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public ProductType ProductType { get; set; }

    [JsonIgnore]
    public User? User { get; set; }
}