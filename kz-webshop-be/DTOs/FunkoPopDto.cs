using kz_webshop_be.Enums;

namespace kz_webshop_be.DTOs;

public class FunkoPopDto : ProductDto
{
    public FunkoCategory Category { get; set; }
    public Franchise? Franchise { get; set; }
    public bool IsLimitedEdition { get; set; }
    public bool IsPreorder { get; set; }
    public bool IsExclusive { get; set; }
    public bool IsChase { get; set; }
    public List<FunkoPopBadgeDto>? Badges { get; set; } 
    public List<FunkoPopTagDto>? FunkoPopTags { get; set; }
}