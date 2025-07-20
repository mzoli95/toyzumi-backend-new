namespace kz_webshop_be.DTOs;
public class LoginRequest
{
    public string FirebaseUid { get; set; }
    public string Email { get; set; }
    public string? Username { get; set; }
    public string? ProfilePicture { get; set; }
}