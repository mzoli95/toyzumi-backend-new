using System.Text.Json.Serialization;

namespace kz_webshop_be.Models;

public abstract class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public string? MainImageUrl { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }
    public bool IsVisible { get; set; }
    public int? MaxOrderQuantity { get; set; }
    public int? MinOrderQuantity { get; set; }
    public bool IsDeleted { get; set; } = false;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public string? Sku { get; set; } // Stock Keeping Unit
    public string? Barcode { get; set; } 
    public string? Brand { get; set; }
    public bool IsNew { get; set; } = false;
    public bool IsOnSale { get; set; } = false;
    public double? AverageRating { get; set; }
    public decimal? Weight { get; set; }
    public ProductDimensions? Dimensions { get; set; }
    public bool IsReStock { get; set; } = false;
    public bool IsAvailable { get; set; } = false;

    public DateTime? ReleaseDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public List<ProductImage>? Images { get; set; }
    [JsonIgnore]
    public List<ProductReview>? Reviews { get; set; }
    [JsonIgnore]
    public List<ProductComment>? Comments { get; set; }
    [JsonIgnore]
    public List<RelatedProduct>? RelatedProducts { get; set; } = new();

}
