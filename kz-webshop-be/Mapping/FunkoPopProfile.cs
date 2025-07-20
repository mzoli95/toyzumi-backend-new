using AutoMapper;
using kz_webshop_be.Models;
using kz_webshop_be.DTOs;

namespace kz_webshop_be.Mapping;

public class FunkoPopProfile : Profile
{
    public FunkoPopProfile()
    {
        CreateMap<FunkoPop, FunkoPopDto>().ReverseMap();
        CreateMap<FunkoPopTag, FunkoPopTagDto>().ReverseMap();
    }
}