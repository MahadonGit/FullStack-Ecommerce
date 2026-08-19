using Ecommerce.Models;
using Ecommerce.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Text;

namespace Ecommerce.Services.Implementations
{
    public class TokenService : ITokenService

    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public TokenService(IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        //Creating the token here;


        public async Task<string> CreateTokenAsync(ApplicationUser user)
        {


            //Go into roles and get the roles of the user and add them to the claims
            var roles = await _userManager.GetRolesAsync(user);

                    var claims = new List<Claim>
                    {
                           new Claim(ClaimTypes.NameIdentifier, user.Id),
                           new Claim(  ClaimTypes.Email,user.Email ?? string.Empty),
                           new Claim( ClaimTypes.Name,user.UserName ?? string.Empty)
                    };
            //Add the roles to the claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));

            }
            //settings for the token, like key, issuer, audience, and expiry time

            var key = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "JWT Key is missing.");

            var issuer = _configuration["Jwt:Issuer"];

            var audience = _configuration["Jwt:Audience"];

            var expiryMinutes = int.Parse(
                _configuration["Jwt:ExpiryMinutes"] ?? "60");

            var securityKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(key));

            var credentials =
                new SigningCredentials(
                    securityKey,
                    SecurityAlgorithms.HmacSha256);

            //make the token and return it

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    expiryMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }

    }
}
