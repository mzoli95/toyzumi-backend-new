using AutoMapper;
using kz_webshop_be.Models;
using kz_webshop_be.DTOs;

namespace kz_webshop_be.Mapping;

public class AddressProfile : Profile
{
    public AddressProfile()
    {
        CreateMap<Address, AddressDto>().ReverseMap();
    }
}