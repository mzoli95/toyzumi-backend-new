namespace kz_webshop_be.DTOs;

public class LabubuDto : ProductDto
{
    public string? Series { get; set; }
    public string? Edition { get; set; }
    public bool IsLimitedEdition { get; set; }
    public bool IsPreorder { get; set; }
    public bool IsUsed { get; set; }
}