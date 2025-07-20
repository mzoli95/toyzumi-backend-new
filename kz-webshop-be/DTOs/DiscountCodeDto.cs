namespace kz_webshop_be.DTOs;

public class DiscountCodeDto
{
    public Guid? Id { get; set; }
    public string? Code { get; set; } = string.Empty;
    public decimal? DiscountAmount { get; set; }
    public bool? IsPercentage { get; set; }
    public DateTime? ExpirationDate { get; set; }
}