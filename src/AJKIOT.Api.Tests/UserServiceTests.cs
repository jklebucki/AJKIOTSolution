using AJKIOT.Api.Models;
using AJKIOT.Api.Services;
using AJKIOT.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace AJKIOT.Api.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<ILogger<UserService>> _loggerMock;
        private readonly Mock<IEmailSender> _emailSenderMock;
        private readonly UserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserServiceTests()
        {
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            _tokenServiceMock = new Mock<ITokenService>();
            _loggerMock = new Mock<ILogger<UserService>>();
            _emailSenderMock = new Mock<IEmailSender>();_httpContextAccessor = new Mock<IHttpContextAccessor>().Object;
            _userService = new UserService(_userManagerMock.Object, _tokenServiceMock.Object, _loggerMock.Object, _emailSenderMock.Object);
        }

        [Fact]
        public async Task AuthenticateUserAsync_UserNotFound_ReturnsUnauthorized()
        {
            // Arrange
            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser)null);

            var request = new AuthRequest { Email = "nonexisting@example.com", Password = "Test1234!" };

            // Act
            var response = await _userService.AuthenticateUserAsync(request);

            // Assert
            Assert.Contains("Unauthorized", response.Errors);
        }

        [Fact]
        public async Task AuthenticateUserAsync_ValidUser_ReturnsAuthResponse()
        {
            // Arrange
            var user = new ApplicationUser { Email = "valid@example.com", UserName = "validUser" };
            _userManagerMock.Setup(x => x.FindByEmailAsync(user.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(true);
            _tokenServiceMock.Setup(x => x.CreateToken(user)).Returns(new string[] { "token" });

            var request = new AuthRequest { Email = user.Email, Password = "ValidPass123!" };

            // Act
            var response = await _userService.AuthenticateUserAsync(request);

            // Assert
            Assert.Empty(response.Errors);
            Assert.NotNull(response.Data);
            Assert.Equal(user.Email, response.Data.Email);
            Assert.Contains("token", response.Data.Tokens);
        }

        [Fact]
        public async Task RegisterUserAsync_NewUser_ReturnsAuthResponse()
        {
            // Arrange
            var request = new RegistrationRequest { Email = "new@example.com", Password = "Test1234!", Username = "newUser" };
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(new ApplicationUser { Email = request.Email, UserName = request.Username });
            _tokenServiceMock.Setup(x => x.CreateToken(It.IsAny<ApplicationUser>())).Returns(new string[] { "token" });

            // Act
            var response = await _userService.RegisterUserAsync(request);

            // Assert
            Assert.Empty(response.Errors);
            Assert.NotNull(response.Data);
            Assert.Equal(request.Email, response.Data.Email);
            Assert.Contains("token", response.Data.Tokens);
        }

        [Fact]
        public async Task RegisterUserAsync_UserAlreadyExists_ReturnsError()
        {
            // Arrange
            var request = new RegistrationRequest { Email = "existing@example.com", Username = "ExistingUser", Password = "Password123!" };
            var existingUser = new ApplicationUser { UserName = request.Username, Email = request.Email };

            // Setup UserManager to simulate that the user already exists
            _userManagerMock.Setup(um => um.FindByEmailAsync(request.Email))
                .ReturnsAsync(existingUser); // Assuming the user is found, simulating an existing user

            // Simulate that creation attempt will fail due to existing user
            _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "User already exists." }));

            // Act
            var result = await _userService.RegisterUserAsync(request);

            // Assert
            Assert.NotEmpty(result.Errors); // Expecting errors
            Assert.Contains("User already exists.", result.Errors); // Specific error message check
        }
    }

}

