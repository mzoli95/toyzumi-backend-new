using kz_webshop_be.Enums;

namespace kz_webshop_be.DTOs;

public class FunkoPopDto : ProductDto
{
    public FunkoCategory Category { get; set; }
    public string? Franchise { get; set; }
    public bool IsLimitedEdition { get; set; }
    public bool IsPreorder { get; set; }
    public bool IsUsed { get; set; }
    public List<FunkoPopTagDto>? FunkoPopTags { get; set; }
}