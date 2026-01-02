using kz_webshop_be.Enums;

namespace kz_webshop_be.DTOs
{
    public class PromotionItemDto
    {
        public ProductType Type { get; set; }
        public FunkoPopDto? FunkoPop { get; set; }
        public LabubuDto? Labubu { get; set; }
        public bool IsFavorite { get; set; }

    }
}
