using kz_webshop_be.Enums;

namespace kz_webshop_be.DTOs;
public class RecentlyViewedItemDto
{
    public Guid ProductId { get; set; }
    public ProductType ProductType { get; set; }
    public DateTime ViewedAt { get; set; }
}
