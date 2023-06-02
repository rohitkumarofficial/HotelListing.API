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
        private ApiUser _user;

        private const string _loginProvider = "HotelListingApi";
        private const string _refreshToken = "RefreshToken";
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
            var _user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (_user == null)
            {
                return null;
            }

            bool isValidUserCredential = await _userManager.CheckPasswordAsync(_user, loginDto.Password);
           
            if (!isValidUserCredential) {
                return null;
            }

            var token = await GenerateToken(_user);

            return new AuthResponseDto
            {
                Token = token,
                UserId = _user.Id,
                RefreshToken = await CreateRefreshToken(_user)
            };
        }

        public async Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto)
        {
            _user = _mapper.Map<ApiUser>(userDto);
            _user.UserName = userDto.Email;
            var result = await _userManager.CreateAsync(_user, userDto.Password);
            if(result.Succeeded)
            {
                await _userManager.AddToRoleAsync(_user, "User");
            }
            return result.Errors;
        }

        public async Task<AuthResponseDto> VerifyRefreshToken(AuthResponseDto request)
        {
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var tokenContent = jwtSecurityTokenHandler.ReadJwtToken(request.Token);
            var username = tokenContent.Claims.ToList().FirstOrDefault(q => q.Type == JwtRegisteredClaimNames.Email)?.Value;
            _user = await _userManager.FindByNameAsync(username);
            if (_user == null || _user.Id != request.UserId)
            {
                return null;
            }
            var isValidRefreshToken = await _userManager.VerifyUserTokenAsync(_user, _loginProvider, _refreshToken, request.RefreshToken);

            if (isValidRefreshToken)
            {
                var token = await GenerateToken(_user);

                return new AuthResponseDto
                {
                    Token = token,
                    UserId = _user.Id,
                    RefreshToken = await CreateRefreshToken(_user),
                };
            }

            await _userManager.UpdateSecurityStampAsync(_user);

            return null;
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

        public async Task<string> CreateRefreshToken(ApiUser user)
        {
            await _userManager.RemoveAuthenticationTokenAsync(user, _loginProvider, _refreshToken);
            var newRefreshToken = await _userManager.GenerateUserTokenAsync(user, _loginProvider, _refreshToken);
            await _userManager.SetAuthenticationTokenAsync(user, _loginProvider, _refreshToken, newRefreshToken);
            return newRefreshToken;
        }
    }
}
