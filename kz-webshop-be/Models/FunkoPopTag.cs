namespace kz_webshop_be.Models;

public class FunkoPopTag
{
    public Guid Id { get; set; }
    public Guid FunkoPopId { get; set; }
    public string Name { get; set; } = string.Empty;

    public FunkoPop FunkoPop { get; set; }
}