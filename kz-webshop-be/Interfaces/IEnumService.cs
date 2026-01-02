using kz_webshop_be.DTOs;

namespace kz_webshop_be.Interfaces
{
    public interface IEnumService
    {
        IEnumerable<string> GetFunkoCategories();
        IEnumerable<string> GetFranchises();
        IEnumerable<string> GetBadges();
        IEnumerable<string> GetFunkoPopTagTypes();
        public List<EnumValueDto> GetEnumValueList<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
                .Cast<T>()
                .Select(e => new EnumValueDto
                {
                    Value = Convert.ToInt32(e),
                    Name = e.ToString()
                })
                .ToList();
        }
        IEnumerable<EnumValueDto> GetFunkoCategoryList();
        IEnumerable<EnumValueDto> GetFranchiseList();
        IEnumerable<EnumValueDto> GetBadgeList();
        IEnumerable<EnumValueDto> GetFunkoPopTagTypeList();
    }
}
