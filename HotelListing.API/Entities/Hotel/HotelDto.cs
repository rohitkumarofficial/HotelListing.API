using System.ComponentModel.DataAnnotations.Schema;

namespace HotelListing.API.Entities.Hotel
{
    public class HotelDto : BaseHotelDto
    {
        public int Id { get; set; }
    }
}
