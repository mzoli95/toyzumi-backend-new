namespace kz_webshop_be.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string FirebaseUid { get; set; }
    public required string Email { get; set; }
    public RoleState Role { get; set; }
    public required string Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? ProfilePicture { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public int Points { get; set; }
    public List<UserDiscountCodeDto>? DiscountCodes { get; set; }
}