using System.Text.Json.Serialization;
using kz_webshop_be.Enums;

namespace kz_webshop_be.Models;

public class RelatedProduct
{
    public Guid ProductId { get; set; }
    public Guid RelatedToId { get; set; }
    public ProductType ProductType { get; set; }
    public ProductType RelatedToType { get; set; }

    [JsonIgnore]
    public Product? Product { get; set; }
    [JsonIgnore]
    public Product? RelatedTo { get; set; }
}