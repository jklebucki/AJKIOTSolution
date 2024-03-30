using AJKIOT.Api.Models;
using AJKIOT.Api.Services;
using AJKIOT.Shared.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
namespace AJKIOT.Api.Tests
{
    public class TokenServiceTests
    {
        private readonly Mock<ILogger<TokenService>> _loggerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly IConfigurationSection _jwtTokenConfigMock;
        private readonly TokenService _tokenService;

        public TokenServiceTests()
        {
            _loggerMock = new Mock<ILogger<TokenService>>();
            _configurationMock = new Mock<IConfiguration>();
            _jwtTokenConfigMock = new Mock<IConfigurationSection>().Object;

            // Setup JWT Token Config Mock
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x["ValidIssuer"]).Returns("TestIssuer");
            configSectionMock.Setup(x => x["ValidAudience"]).Returns("TestAudience");
            configSectionMock.Setup(x => x["SymmetricSecurityKey"]).Returns("YourVerySecureAndSufficientlyLongKey");
            configSectionMock.Setup(s => s["JwtRegisteredClaimNamesSub"]).Returns("user_id");
            _configurationMock.Setup(x => x.GetSection("JwtTokenSettings")).Returns(configSectionMock.Object);

            _tokenService = new TokenService(_loggerMock.Object, _configurationMock.Object);
        }

        [Fact]
        public void CreateToken_ShouldGenerateTwoTokens()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "UserId",
                UserName = "TestUser",
                Email = "testuser@example.com",
                Role = Role.User
            };

            // Act
            var tokens = _tokenService.CreateToken(user);

            // Assert
            Assert.Equal(2, tokens.Length);
        }

        [Fact]
        public void AccessToken_ShouldContainExpectedClaims()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "UserId1",
                UserName = "TestUser1",
                Email = "testuser1@example.com",
                Role = Role.User
            };

            // Act
            var tokens = _tokenService.CreateToken(user);
            var accessToken = new JwtSecurityTokenHandler().ReadJwtToken(tokens[0]);

            // Assert
            Assert.Contains(accessToken.Claims, claim => claim.Type == "sub" && claim.Value == "user_id");
            Assert.Contains(accessToken.Claims, claim => claim.Type == ClaimTypes.Email && claim.Value == user.Email);
            Assert.Contains(accessToken.Claims, claim => claim.Type == ClaimTypes.Role && claim.Value == Role.User.ToString());
        }

        [Fact]
        public void RefreshToken_ShouldContainRefreshTokenRole()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "UserId2",
                UserName = "TestUser2",
                Email = "testuser2@example.com",
                Role = Role.User
            };

            // Act
            var tokens = _tokenService.CreateToken(user);
            var refreshToken = new JwtSecurityTokenHandler().ReadJwtToken(tokens[1]);

            // Assert
            Assert.Contains(refreshToken.Claims, claim => claim.Type == ClaimTypes.Role && claim.Value == Role.RefreshToken.ToString());

        }

        [Fact]
        public void TokenService_WithInvalidConfig_ShouldThrowException()
        {
            // Arrange
            var invalidConfig = new Mock<IConfiguration>();
            var configSectionMock = new Mock<IConfigurationSection>();
            configSectionMock.Setup(x => x["SymmetricSecurityKey"]).Returns((string)null!); // No key provided
            invalidConfig.Setup(x => x.GetSection("JwtTokenSettings")).Returns(configSectionMock.Object);

            // Assert
            Assert.Throws<InvalidOperationException>(() => new TokenService(_loggerMock.Object, invalidConfig.Object));
        }
    }
}
