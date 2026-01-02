namespace kz_webshop_be.DTOs
{
    public class EnumListsDto
    {
        public required IEnumerable<EnumValueDto> Categories { get; set; }
        public required IEnumerable<EnumValueDto> Franchises { get; set; }
        public required IEnumerable<EnumValueDto> Badges { get; set; }
        public required IEnumerable<EnumValueDto> FunkoPopTagTypes { get; set; }
    }
}
