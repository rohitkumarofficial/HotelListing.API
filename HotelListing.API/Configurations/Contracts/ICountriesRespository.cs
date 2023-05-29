using HotelListing.API.Data;

namespace HotelListing.API.Configurations.Contracts
{
    public interface ICountriesRepository : IGenericRepository<Country>
    {
        Task<Country> GetDetails(int id);
    }
}
