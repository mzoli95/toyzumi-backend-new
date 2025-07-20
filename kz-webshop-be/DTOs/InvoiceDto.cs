namespace kz_webshop_be.DTOs;

public class InvoiceDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public string? CompanyName { get; set; }
    public string? TaxNumber { get; set; }
    public string? CompanyAddress { get; set; }
    public string? CompanyBankAccount { get; set; }
    public string? CompanyPhone { get; set; }
    public string? Website { get; set; }
    public string? Logo { get; set; }
    public string? Description { get; set; }
}