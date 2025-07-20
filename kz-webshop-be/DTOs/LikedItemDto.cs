using kz_webshop_be.Enums;

namespace kz_webshop_be.DTOs;

public class LikedItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public ProductType ProductType { get; set; }
}