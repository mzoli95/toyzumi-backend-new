namespace kz_webshop_be.DTOs
{
    public class PromotionDto
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