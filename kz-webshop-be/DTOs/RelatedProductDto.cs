using kz_webshop_be.Enums;

namespace kz_webshop_be.DTOs;

public class RelatedProductDto
{
    public Guid? ProductId { get; set; }
    public ProductType ProductType { get; set; }
    public Guid RelatedToId { get; set; }
    public ProductType RelatedToType { get; set; }
}