namespace kz_webshop_be.DTOs;

public class ProductReviewDto
{
    public Guid? Id { get; set; }
    public Guid UserId { get; set; }
    public int Stars { get; set; }
    public string? ReviewText { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}