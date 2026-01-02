using kz_webshop_be.Enums;

namespace kz_webshop_be.DTOs;

public class FunkoPopTagDto
{
    public Guid? Id { get; set; }
    public Guid FunkoPopId { get; set; }

    public FunkoPopTagType Name { get; set; }
}