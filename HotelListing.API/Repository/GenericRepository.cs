using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.API.Configurations.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.API.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly HotelListingDbContext _context;
        private readonly IMapper _mapper;

        public GenericRepository(HotelListingDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<T> AddAsync(T entity)
        {
           await _context.AddAsync(entity);
           _context.SaveChanges();
            return entity;
        }

        public async Task DeleteAsync(int? id)
        {
            var entity = await GetAsync(id);
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<PageResult<TResult>> GetAllAsync<TResult>(QueryParameters queryParameters)
        {
            var totalSize =  await _context.Set<T>().CountAsync();
            var items = await _context.Set<T>()
                .Skip(queryParameters.StartIndex)
                .Take(queryParameters.PageSize)
                .ProjectTo<TResult>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new PageResult<TResult>
            {
                Items = items,
                PageNumber = queryParameters.PageNumber,
                RecordNumber = queryParameters.PageSize,
                TotalCount = totalSize
            };
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<T> GetAsync(int? id)
        {
            if(id == null) throw new ArgumentNullException("id");
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<bool> IsExists(int id)
        {
            var entity =  await GetAsync(id);
            return entity != null;
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
