using AutoMapper;
using kz_webshop_be.Models;
using kz_webshop_be.DTOs;

namespace kz_webshop_be.Mapping;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        // FunkoPop
        CreateMap<FunkoPop, FunkoPopDto>().ReverseMap();
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