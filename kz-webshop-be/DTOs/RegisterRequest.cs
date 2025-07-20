namespace kz_webshop_be.DTOs;

public class RegisterRequest
{
    public required string FirebaseUid { get; set; }
    public required string Email { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? ProfilePicture { get; set; }

}