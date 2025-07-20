namespace kz_webshop_be.DTOs;

public class UserDiscountCodeDto
{
    public Guid UserId { get; set; }
    public Guid DiscountCodeId { get; set; }
    public int UseCount { get; set; }
    public DiscountCodeDto? DiscountCode { get; set; }
}