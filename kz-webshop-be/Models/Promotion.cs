namespace kz_webshop_be.Models
{
    public class Promotion
    {
        public Guid Id { get; set; }
        public int FreeShippingFrom { get; set; }
        public string PromotionText { get; set; } = string.Empty;
        public string? PromotionCode { get; set; }
        public decimal? DiscountPercent { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}