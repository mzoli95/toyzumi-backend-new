using AutoMapper;
using kz_webshop_be.Models;
using kz_webshop_be.DTOs;

namespace kz_webshop_be.Mapping;

public class LabubuProfile : Profile
{
    public LabubuProfile()
    {
        CreateMap<Labubu, LabubuDto>().ReverseMap();
    }
}