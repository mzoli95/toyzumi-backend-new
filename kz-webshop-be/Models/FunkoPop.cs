using kz_webshop_be.Enums;

namespace kz_webshop_be.Models;

public class FunkoPop : Product
{
    public FunkoCategory Category { get; set; }
    public string? Franchise { get; set; }
    public bool IsLimitedEdition { get; set; }
    public bool IsPreorder { get; set; }
    public bool IsUsed { get; set; }

    public List<FunkoPopTag>? FunkoPopTags { get; set; } = new();
}