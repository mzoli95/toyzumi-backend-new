using System.Text.Json.Serialization;

namespace kz_webshop_be.Models;

public class Address
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public AddressType Type { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    public override string ToString() =>
        $"{Street}, {ZipCode} {City}, {State}, {Country}";

    [JsonIgnore]
    public User? User { get; set; }
}