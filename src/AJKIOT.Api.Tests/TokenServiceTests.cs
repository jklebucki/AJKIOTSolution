using AJKIOT.Api.Models;
using AJKIOT.Api.Services;
using AJKIOT.Shared.Enums;
using Microsoft.Extensions.Logging;
using Moq;


namespace AJKIOT.Api.Tests
{
    public class TokenServiceTests
    {
        private readonly Mock<ILogger<TokenService>> _loggerMock;
        private readonly TokenService _tokenService;
        private readonly ApplicationUser _testUser;

        public TokenServiceTests()
        {
            _loggerMock = new Mock<ILogger<TokenService>>();
            _tokenService = new TokenService(_loggerMock.Object);

            _testUser = new ApplicationUser
            {
                Id = "UserId",
                UserName = "TestUser",
                Email = "test@example.com",
                Role = Role.User
            };
        }

        [Fact]
        public void CreateToken_ReturnsTwoTokens()
        {
            // Act
            var tokens = _tokenService.CreateToken(_testUser);

            // Assert
            Assert.NotNull(tokens);
            Assert.Equal(2, tokens.Length);
            Assert.NotEmpty(tokens[0]);
            Assert.NotEmpty(tokens[1]);
        }

    }
}
