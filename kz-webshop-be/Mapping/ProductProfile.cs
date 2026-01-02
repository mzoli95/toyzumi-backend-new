using AutoMapper;
using kz_webshop_be.Models;
using kz_webshop_be.DTOs;

namespace kz_webshop_be.Mapping;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<FunkoPop, FunkoPopDto>()
           .ForMember(dest => dest.Badges, opt => opt.MapFrom(src => src.Badges != null
               ? src.Badges.Select(b => new FunkoPopBadgeDto
               {
                   Id = b.Id,
                   FunkoPopId = b.FunkoPopId,
                   Badge = b.Badge
               }).ToList()
               : new List<FunkoPopBadgeDto>()))
           .ForMember(dest => dest.FunkoPopTags, opt => opt.MapFrom(src => src.FunkoPopTags))
           .ReverseMap()
           .ForMember(dest => dest.Badges, opt => opt.MapFrom(src => src.Badges != null
               ? src.Badges.Select(b => new FunkoPopBadge
               {
                   Id = b.Id ?? new Guid(),
                   FunkoPopId = b.FunkoPopId,
                   Badge = b.Badge
               }).ToList()
               : new List<FunkoPopBadge>()))
           .ForMember(dest => dest.FunkoPopTags, opt => opt.MapFrom(src => src.FunkoPopTags));

        CreateMap<FunkoPopBadge, FunkoPopBadgeDto>().ReverseMap();
        CreateMap<FunkoPopTag, FunkoPopTagDto>().ReverseMap();

        // Labubu
        CreateMap<Labubu, LabubuDto>().ReverseMap();

        // Közös Product elemek
        CreateMap<ProductImage, ProductImageDto>().ReverseMap();
        CreateMap<ProductReview, ProductReviewDto>().ReverseMap();
        CreateMap<ProductComment, ProductCommentDto>().ReverseMap();

        // Kapcsolódó termékek
        CreateMap<RelatedProduct, RelatedProductDto>().ReverseMap();
    }
}