using HotelListing.API.Entities.Users;
using Microsoft.AspNetCore.Identity;

namespace HotelListing.API.Configurations.Contracts
{
    public interface IAuthManager
    {
        public Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto);
        public Task<AuthResponseDto> Login(LoginDto userDto);
    }
}
