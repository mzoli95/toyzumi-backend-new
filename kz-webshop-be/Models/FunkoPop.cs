using kz_webshop_be.Enums;

namespace kz_webshop_be.Models;

public class FunkoPop : Product
{
    public FunkoCategory Category { get; set; }
    public Franchise? Franchise { get; set; }
    public bool IsLimitedEdition { get; set; }
    public bool IsPreorder { get; set; }
    public bool IsExclusive { get; set; }
    public bool IsChase { get; set; }
    public bool IsSpecial { get; set; }
    public List<FunkoPopBadge> Badges { get; set; } = new();
    public List<FunkoPopTag>? FunkoPopTags { get; set; } = new();
}