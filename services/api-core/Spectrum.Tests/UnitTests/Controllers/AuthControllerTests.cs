using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Spectrum.API.Controllers;
using Spectrum.API.Dtos.Auth;
using Spectrum.API.Services.Auth;

namespace Spectrum.Tests.UnitTests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AuthController _authController;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _authController = new AuthController(_authServiceMock.Object);
        }

        [Fact]
        public async Task TestRegisterWhenValidPayloadShouldReturnCreatedAtAction()
        {
            var registerDto = new RegisterDto { Username = "test", Email = "test@test.com", Password = "Password123!" };
            var expectedResponse = new RegisterResponseDto
            {
                Email = "test@test.com",
                RequiresVerification = true,
                Message = "verificationCodeSent"
            };

            _authServiceMock.Setup(s => s.RegisterAsync(registerDto)).ReturnsAsync(expectedResponse);

            var result = await _authController.Register(registerDto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_authController.Login), createdResult.ActionName);
            Assert.Equal(expectedResponse.Email, createdResult.RouteValues?["email"]);
            Assert.Equal(expectedResponse, createdResult.Value);

            _authServiceMock.Verify(s => s.RegisterAsync(registerDto), Times.Once);
        }

        [Fact]
        public async Task TestLoginWhenValidCredentialsShouldReturnOkObjectResult()
        {
            var loginDto = new LoginDto { Email = "test@test.com", Password = "Password123!" };
            var expectedResponse = new AuthResponseDto { Token = "jwt_token_here", Username = "test", Email = "test@test.com" };

            _authServiceMock.Setup(s => s.LoginAsync(loginDto)).ReturnsAsync(expectedResponse);

            var result = await _authController.Login(loginDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.Equal(expectedResponse, okResult.Value);

            _authServiceMock.Verify(s => s.LoginAsync(loginDto), Times.Once);
        }

        [Fact]
        public async Task TestGoogleLoginWhenValidTokenShouldReturnOkObjectResult()
        {
            var googleAuthDto = new GoogleAuthDto { Credential = "google_jwt_token" };
            var expectedResponse = new AuthResponseDto { Token = "spectrum_jwt_token", Username = "google_user", Email = "user@gmail.com" };

            _authServiceMock.Setup(s => s.GoogleLoginAsync(googleAuthDto)).ReturnsAsync(expectedResponse);

            var result = await _authController.GoogleLogin(googleAuthDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.Equal(expectedResponse, okResult.Value);

            _authServiceMock.Verify(s => s.GoogleLoginAsync(googleAuthDto), Times.Once);
        }

        [Fact]
        public async Task TestRegisterAdminWhenValidPayloadShouldReturnCreatedAtAction()
        {
            var registerAdminDto = new RegisterAdminDto
            {
                Username = "admin",
                Email = "admin@test.com",
                Password = "Password123!",
                AdminSecretKey = "MasterKey"
            };
            var expectedResponse = new AuthResponseDto { Token = "admin_jwt_token", Username = "admin", Email = "admin@test.com" };

            _authServiceMock.Setup(s => s.RegisterAdminAsync(registerAdminDto)).ReturnsAsync(expectedResponse);

            var result = await _authController.RegisterAdmin(registerAdminDto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_authController.Login), createdResult.ActionName);
            Assert.Equal(expectedResponse.Token, createdResult.RouteValues?["id"]);
            Assert.Equal(expectedResponse, createdResult.Value);

            _authServiceMock.Verify(s => s.RegisterAdminAsync(registerAdminDto), Times.Once);
        }
    }
}
