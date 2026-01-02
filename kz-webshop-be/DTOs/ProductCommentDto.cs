namespace kz_webshop_be.DTOs;

public class ProductCommentDto
{
    public Guid? Id { get; set; }
    public Guid UserId { get; set; }
    public string CommentText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}