using Microsoft.AspNetCore.Mvc;
using Spectrum.API.Dtos.Auth;
using Spectrum.API.Services.Auth;

namespace Spectrum.API.Controllers
{
    /// <summary>
    /// Controller responsible for handling user authentication, registration,
    /// and administrative account creation.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// Service handling the business logic for authentication.
        /// </summary>
        private readonly IAuthService _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="authService">The authentication service implementation.</param>
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new standard user (Reviewer) in the system.
        /// </summary>
        /// <param name="registerDto">Data transfer object containing registration details.</param>
        /// <returns>An <see cref="IActionResult"/> containing the authentication response with a JWT.</returns>
        /// <response code="200">Returns the user info and access token.</response>
        /// <response code="400">If the input data fails validation rules.</response>
        /// <response code="409">If the email or username is already registered.</response>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var response = await _authService.RegisterAsync(registerDto);
            return Ok(response);
        }

        /// <summary>
        /// Authenticates an existing user and provides an access token.
        /// </summary>
        /// <param name="loginDto">Data transfer object containing login credentials.</param>
        /// <returns>An <see cref="IActionResult"/> containing the JWT for authorized requests.</returns>
        /// <response code="200">Successful authentication.</response>
        /// <response code="401">If credentials are invalid or the account is suspended.</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var response = await _authService.LoginAsync(loginDto);
            return Ok(response);
        }

        /// <summary>
        /// Authenticates a user using a Google-provided identity token.
        /// </summary>
        /// <param name="googleAuthDto">The token received from Google Client SDK.</param>
        /// <returns>A Spectrum API JWT exchanged for the Google identity.</returns>
        /// <response code="200">Successful authentication with Google.</response>
        /// <response code="400">If the Google token is invalid or expired.</response>
        /// <response code="500">If an error occurs while processing the Google authentication.</response>
        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleAuthDto googleAuthDto)
        {
            var response = await _authService.GoogleLoginAsync(googleAuthDto);
            return Ok(response);
        }

        /// <summary>
        /// Registers a new administrative user using a secure system master key.
        /// </summary>
        /// <param name="dto">Data transfer object including admin details and the secret master key.</param>
        /// <returns>An <see cref="IActionResult"/> with the admin's JWT.</returns>
        /// <exception cref="SpectrumUnauthorizedException">Thrown when the master key is incorrect.</exception>
        /// <response code="200">Admin registration successful.</response>
        /// <response code="400">If the input data fails validation rules.</response>
        /// <response code="401">If the master key is invalid.</response>
        /// <response code="409">If the email or username is already registered.</response>
        /// <response code="500">If an error occurs during admin registration.</response>
        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminDto registerAdminDto)
        {
            var response = await _authService.RegisterAdminAsync(registerAdminDto);
            return Ok(response);
        }
    }
}
