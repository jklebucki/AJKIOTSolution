using AJKIOT.Api.Models;
using AJKIOT.Api.Services;
using AJKIOT.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace AJKIOT.Api.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<ITokenService> _mockTokenService = new Mock<ITokenService>();
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            _mockTokenService.Setup(x => x.CreateToken(It.IsAny<ApplicationUser>())).Returns(["MockedToken"]);

            _userService = new UserService(_mockUserManager.Object, null, _mockTokenService.Object);
        }

        [Fact]
        public async Task AuthenticateUserAsync_ReturnsUnauthorized_WhenUserNotFound()
        {
            // Arrange
            _mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _userService.AuthenticateUserAsync(new AuthRequest { Email = "user@example.com", Password = "Password123!" });

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Unauthorized", result.Errors);
        }

        [Fact]
        public async Task AuthenticateUserAsync_ReturnsSuccess_WhenCredentialsAreValid()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "user@example.com", Email = "user@example.com" };
            _mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(true);
            _mockTokenService.Setup(x => x.CreateToken(It.IsAny<ApplicationUser>())).Returns(["MockedToken"]);

            // Act
            var result = await _userService.AuthenticateUserAsync(new AuthRequest { Email = "user@example.com", Password = "Password123!" });

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal("MockedToken", result.Data.Token[0]);
        }

        [Fact]
        public async Task RegisterUserAsync_ReturnsErrors_WhenUserCreationFails()
        {
            // Arrange
            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Test error" }));

            // Act
            var result = await _userService.RegisterUserAsync(new RegistrationRequest { Email = "user@example.com", Password = "Password123!", Username = "user" });

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Test error", result.Errors);
        }

        [Fact]
        public async Task RegisterUserAsync_ReturnsSuccess_WhenUserIsCreated()
        {
            // Arrange
            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.RegisterUserAsync(new RegistrationRequest { Email = "user@example.com", Password = "Password123!", Username = "user" });

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("user@example.com", result.Data.Email);
        }
    }
}
