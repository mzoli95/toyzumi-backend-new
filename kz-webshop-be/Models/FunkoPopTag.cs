using kz_webshop_be.Enums;

namespace kz_webshop_be.Models;

public class FunkoPopTag
{
    public Guid Id { get; set; }
    public Guid FunkoPopId { get; set; }
    public FunkoPopTagType Name { get; set; }
    public FunkoPop FunkoPop { get; set; }
}
