using kz_webshop_be.DTOs;
using kz_webshop_be.Enums;
using kz_webshop_be.Interfaces;

namespace kz_webshop_be.Services
{
    public class EnumService : IEnumService
    {
        public IEnumerable<string> GetFunkoCategories()
            => Enum.GetNames(typeof(FunkoCategory));

        public IEnumerable<string> GetFranchises()
            => Enum.GetNames(typeof(Franchise));

        public IEnumerable<string> GetBadges()
            => Enum.GetNames(typeof(Badge));

        public IEnumerable<string> GetFunkoPopTagTypes()
            => Enum.GetNames(typeof(FunkoPopTagType));

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

        public IEnumerable<EnumValueDto> GetFunkoCategoryList()
             => GetEnumValueList<FunkoCategory>();

        public IEnumerable<EnumValueDto> GetFranchiseList()
            => GetEnumValueList<Franchise>();

        public IEnumerable<EnumValueDto> GetBadgeList()
            => GetEnumValueList<Badge>();

        public IEnumerable<EnumValueDto> GetFunkoPopTagTypeList()
            => GetEnumValueList<FunkoPopTagType>();
    }
}
