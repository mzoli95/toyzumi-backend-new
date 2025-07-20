namespace kz_webshop_be.DTOs;

public abstract class ProductDto
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
    public DateTime? ReleaseDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }


    public List<ProductImageDto>? Images { get; set; }
    public List<ProductReviewDto>? Reviews { get; set; }
    public List<ProductCommentDto>? Comments { get; set; }
    public List<RelatedProductDto>? RelatedProducts { get; set; }

}