namespace kz_webshop_be.DTOs
{
    public class ShoppingCartProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? MainImageUrl { get; set; }
        public decimal? OriginalPrice { get; set; }
        public bool isPreorder { get; set; } = false;
        public DateTime? ReleaseDate { get; set; }

    }
}