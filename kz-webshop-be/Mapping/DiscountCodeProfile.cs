using AutoMapper;
using kz_webshop_be.DTOs;
using kz_webshop_be.Models;

namespace kz_webshop_be.Mapping;

public class DiscountCodeProfile : Profile
{
    public DiscountCodeProfile()
    {
        CreateMap<DiscountCode, DiscountCodeDto>().ReverseMap();
        CreateMap<UserDiscountCode, UserDiscountCodeDto>().ReverseMap();
    }
}