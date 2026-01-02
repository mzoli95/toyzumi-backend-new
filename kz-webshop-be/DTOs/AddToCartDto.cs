using kz_webshop_be.Enums;

namespace kz_webshop_be.DTOs
{
    public class AddToCartDto
    {
        public Guid ProductId { get; set; }
        public ProductType ProductType { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
