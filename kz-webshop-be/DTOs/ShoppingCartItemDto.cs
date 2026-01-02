using kz_webshop_be.Enums;

namespace kz_webshop_be.DTOs;

public class ShoppingCartItemDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public ProductType ProductType { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime AddedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
        public string Name { get; set; } = string.Empty;
    public string MainImageUrl { get; set; } = string.Empty;
    public decimal? OriginalPrice { get; set; }
    public bool? IsPreorder { get; set; } = false;
    public DateTime? ReleaseDate { get; set; }
}