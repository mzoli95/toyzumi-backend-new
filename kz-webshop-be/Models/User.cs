using System.Text.Json.Serialization;
using kz_webshop_be.Models;

public class User
{
    public Guid Id { get; set; }
    public string FirebaseUid { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public RoleState Role { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? ProfilePicture { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public int Points { get; set; } = 0;

    [JsonIgnore]
    public List<Address> Addresses { get; set; } = new();
    [JsonIgnore]
    public List<UserDiscountCode> UserDiscountCodes { get; set; } = new();
    [JsonIgnore]
    public List<ShoppingCartItem> ShoppingCartItems { get; set; } = new();
    [JsonIgnore]
    public List<Order> Orders { get; set; } = new();
    [JsonIgnore]
    public List<LikedItem> LikedItems { get; set; } = new();

}

