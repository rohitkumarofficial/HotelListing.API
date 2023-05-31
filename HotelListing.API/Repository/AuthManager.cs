using AutoMapper;
using HotelListing.API.Configurations.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Entities.Users;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelListing.API.Repository
{
    public class AuthManager : IAuthManager
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApiUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthManager(
            IMapper mapper,
            UserManager<ApiUser> userManager,
            IConfiguration configuration
            )
        {
            _mapper = mapper;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
            {
                return null;
            }

            bool isValidUserCredential = await _userManager.CheckPasswordAsync(user, loginDto.Password);
           
            if (!isValidUserCredential) {
                return null;
            }

            var token = await GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                UserId = user.Id
            };
        }

        public async Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto)
        {
            var user = _mapper.Map<ApiUser>(userDto);
            user.UserName = userDto.Email;
            var result = await _userManager.CreateAsync(user, userDto.Password);
            if(result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
            }
            return result.Errors;
        }

        private async Task<string> GenerateToken(ApiUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!));
            var credential = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList();
            var userClaims = await _userManager.GetClaimsAsync(user);

            var claims = new List<Claim>
            {
                new Claim (JwtRegisteredClaimNames.Sub, user.Email!),
                new Claim (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim (JwtRegisteredClaimNames.Email, user.Email!),
            }
            .Union(userClaims).Union(roleClaims);

            var token = new JwtSecurityToken(
                    issuer: _configuration["JwtSettings:Issuer"],
                    audience: _configuration["JwtSettings:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["JwtSettings:DurationInMinutes"])),
                    signingCredentials: credential
               );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
