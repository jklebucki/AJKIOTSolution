using AJKIOT.Api.Models;
using AJKIOT.Shared.Enums;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AJKIOT.Api.Services
{
    public class TokenService : ITokenService
    {
        private const int ExpirationMinutes = 30;
        private const int RefreshTokenExpirationDays = 7;
        private readonly ILogger<TokenService> _logger;
        private readonly IConfigurationSection _jwtTokenConfig;

        public TokenService(ILogger<TokenService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _jwtTokenConfig = configuration.GetSection("JwtTokenSettings");
            if (_jwtTokenConfig == null || string.IsNullOrEmpty(_jwtTokenConfig["SymmetricSecurityKey"])
                || string.IsNullOrEmpty(_jwtTokenConfig["JwtRegisteredClaimNamesSub"])
                || string.IsNullOrEmpty(_jwtTokenConfig["ValidIssuer"])
                || string.IsNullOrEmpty(_jwtTokenConfig["ValidAudience"]))
            {
                throw new InvalidOperationException("JwtTokenSettings section is missing in configuration.");
            }
        }

        public string[] CreateToken(ApplicationUser user)
        {
            var accessTokenExpiration = DateTime.UtcNow.AddMinutes(ExpirationMinutes);
            var accessToken = CreateJwtToken(
                CreateClaims(user, false), // False indicates this is not a refresh token
                CreateSigningCredentials(),
                accessTokenExpiration
            );
            var tokenHandler = new JwtSecurityTokenHandler();
            _logger.LogInformation("JWT Access Token created");

            var refreshTokenExpiration = DateTime.UtcNow.AddDays(RefreshTokenExpirationDays);
            var refreshToken = CreateJwtToken(
                CreateClaims(user, true), // True indicates this is a refresh token
                CreateSigningCredentials(),
                refreshTokenExpiration
            );
            _logger.LogInformation("JWT Refresh Token created");

            return
            [
                tokenHandler.WriteToken(accessToken),
                tokenHandler.WriteToken(refreshToken)
            ];
        }

        private JwtSecurityToken CreateJwtToken(IEnumerable<Claim> claims, SigningCredentials credentials, DateTime expiration)
        {
            return new JwtSecurityToken(
                issuer: _jwtTokenConfig["ValidIssuer"],
                audience: _jwtTokenConfig["ValidAudience"],
                claims: claims,
                expires: expiration,
                signingCredentials: credentials
            );
        }

        private IEnumerable<Claim> CreateClaims(ApplicationUser user, bool isRefreshToken)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, _jwtTokenConfig["JwtRegisteredClaimNamesSub"]!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
            };

            if (isRefreshToken)
            {
                claims.Add(new Claim(ClaimTypes.Role, Role.RefreshToken.ToString()));
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role.ToString()));
            }

            return claims;
        }

        private SigningCredentials CreateSigningCredentials()
        {
            var key = Encoding.UTF8.GetBytes(_jwtTokenConfig["SymmetricSecurityKey"]!);
            var symmetricSecurityKey = new SymmetricSecurityKey(key);
            return new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
        }
    }
}
