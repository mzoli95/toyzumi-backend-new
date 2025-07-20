using System.Text.Json.Serialization;

namespace kz_webshop_be.Models;

public class ProductImage
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Url { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public Product? Product { get; set; }
}