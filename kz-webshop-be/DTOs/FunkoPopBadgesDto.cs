using kz_webshop_be.Enums;
using kz_webshop_be.Models;

namespace kz_webshop_be.DTOs
{
    public class FunkoPopBadgeDto
    {
        public Guid? Id { get; set; }
        public Guid FunkoPopId { get; set; }
        public Badge Badge { get; set; }
    }
}
