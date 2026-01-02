using kz_webshop_be.Enums;
using kz_webshop_be.Models;

namespace kz_webshop_be.DTOs
{
    //public class ProductFilterDto
    //{
    //    public string? SearchTerm { get; set; }
    //    public List<FunkoCategory>? Categories { get; set; }
    //    public List<Franchise>? Franchises { get; set; }
    //    public List<ProductType>? Types { get; set; } 
    //    public PriceRangeDto? PriceRange { get; set; }
    //    public List<string>? Availability { get; set; } 
    //    public bool? IsOnSale { get; set; }
    //    public bool? IsNew { get; set; }
    //    public bool? IsLimitedEdition { get; set; }
    //    public bool? IsExclusive { get; set; }
    //    public bool? IsChase { get; set; }
    //    public bool? IsSpecial { get; set; }
    //    public ProductSortBy? SortBy { get; set; }
    //    public SortDirection? SortDirection { get; set; }
    //    public ProductView? View { get; set; }
    //}

    public class ProductFilterDto
    {
        public string? SearchTerm { get; set; }
        public List<int>? Categories { get; set; }
        public List<int>? Franchises { get; set; }
        public List<int>? Types { get; set; }
        public PriceRangeDto? PriceRange { get; set; }
        public List<string>? Availability { get; set; }
        public bool? IsOnSale { get; set; }
        public bool? IsNew { get; set; }
        public bool? IsLimitedEdition { get; set; }
        public bool? IsExclusive { get; set; }
        public bool? IsChase { get; set; }
        public bool? IsSpecial { get; set; }
        public List<int>? Size { get; set; }
        public List<int>? Badges { get; set; }
        public List<int>? Tags { get; set; }
        public ProductSortBy SortBy { get; set; } = ProductSortBy.Newest;
        public SortDirection? SortDirection { get; set; }
    }

    public class PriceRangeDto
    {
        public decimal Min { get; set; }
        public decimal Max { get; set; }
    }


    public enum ProductSortBy
    {
        Newest = 0,
        BestMatch = 1,
        Popular = 2,
        PriceAsc = 3,
        PriceDesc = 4
    }

    public enum SortDirection
    {
        Desc = 0,
        Asc = 1
    }
}
