using AutoMapper;
using HotelListing.API.Data;
using HotelListing.API.Entities.Country;
using HotelListing.API.Entities.Hotel;
using HotelListing.API.Entities.Users;

namespace HotelListing.API.Configurations
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            CreateMap<Country, CreateCountryDto>().ReverseMap();
            CreateMap<Country, CountryDto>().ReverseMap();
            CreateMap<Country, CountryDetailDto>().ReverseMap();
            CreateMap<Country, UpdateCountryDto>().ReverseMap();

            CreateMap<Hotel, HotelDto>().ReverseMap();
            CreateMap<Hotel, CreateHotelDto>().ReverseMap();

            CreateMap<ApiUserDto, ApiUser>().ReverseMap();
            CreateMap<LoginDto, ApiUser>().ReverseMap();
        }
    }   
}
