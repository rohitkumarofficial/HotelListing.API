using HotelListing.API.Data;
using HotelListing.API.Entities.Hotel;

namespace HotelListing.API.Entities.Country
{
    public class CountryDto : BaseCountryDto
    {
        public int Id { get; set; }
    }

    public class CountryDetailDto : BaseCountryDto
    {
        public int Id { get; set; }
        public IList<HotelDto> Hotels { get; set; }
    }
}
