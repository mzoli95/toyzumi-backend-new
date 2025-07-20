namespace kz_webshop_be.Models;

public class Labubu : Product
{
    public string? Series { get; set; }
    public string? Edition { get; set; }
    public bool IsLimitedEdition { get; set; }
    public bool IsPreorder { get; set; }
    public bool IsUsed { get; set; }
    // Ha lesznek LabubuTag vagy RelatedLabubu entitások, azokat is itt lehet majd hozzáadni.
}