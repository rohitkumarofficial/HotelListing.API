using HotelListing.API.Data;
using HotelListing.API.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelListing.API.Configurations.Contracts
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetAsync(int? id);
        Task<List<T>> GetAllAsync();
        Task<PageResult<TResult>> GetAllAsync<TResult>(QueryParameters queryParameters);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int? id);
        Task<bool> IsExists(int id);
    }
}
 