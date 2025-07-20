using kz_webshop_be.Enums;

namespace kz_webshop_be.DTOs;

public class AddressDto
{
    public Guid Id { get; set; }
    public AddressType Type { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}