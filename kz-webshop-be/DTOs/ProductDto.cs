using kz_webshop_be.Enums;
using kz_webshop_be.Models;

namespace kz_webshop_be.DTOs;

public abstract class ProductDto
{
    public Guid? Id { get; set; }
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
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public string? Brand { get; set; }
    public bool IsNew { get; set; } = false;
    public bool IsOnSale { get; set; } = false;
    public bool IsFavorite { get; set; } = false;
    public double? AverageRating { get; set; }
    public decimal? Weight { get; set; }
    public ProductDimensions? Dimensions { get; set; }
    public bool IsReStock { get; set; } = false;
    public bool IsAvailable { get; set; } = false;
    public DateTime? ReleaseDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ProductType ProductType { get; set; } 


    public List<ProductImageDto>? Images { get; set; }
    public List<ProductReviewDto>? Reviews { get; set; }
    public List<ProductCommentDto>? Comments { get; set; }
    public List<RelatedProductDto>? RelatedProducts { get; set; }
}