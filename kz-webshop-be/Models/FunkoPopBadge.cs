using kz_webshop_be.Enums;

namespace kz_webshop_be.Models
{
    public class FunkoPopBadge
    {
        public Guid Id { get; set; }
        public Guid FunkoPopId { get; set; }
        public Badge Badge { get; set; }
        public FunkoPop FunkoPop { get; set; } // Navigációs property
    }
}
