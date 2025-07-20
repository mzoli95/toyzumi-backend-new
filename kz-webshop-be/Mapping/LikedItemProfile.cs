using AutoMapper;
using kz_webshop_be.DTOs;
using kz_webshop_be.Models;

namespace kz_webshop_be.Mapping;

public class LikedItemProfile : Profile
{
    public LikedItemProfile()
    {
        CreateMap<LikedItem, LikedItemDto>().ReverseMap();
    }
}