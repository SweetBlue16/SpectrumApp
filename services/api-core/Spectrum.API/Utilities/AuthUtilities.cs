using Microsoft.IdentityModel.Tokens;
using Spectrum.API.Dtos.Auth;
using Spectrum.API.Exceptions;
using Spectrum.API.Models;
using Spectrum.API.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Spectrum.API.Utilities
{
    /// <summary>
    /// Static utility class that provides helper methods for authentication, 
    /// input validation, and security token generation.
    /// </summary>
    public static class AuthUtilities
    {
        /// <summary>
        /// Generates a signed JSON Web Token (JWT) for a specific user.
        /// </summary>
        /// <param name="user">The user entity containing identity and role data.</param>
        /// <param name="configuration">Application configuration to retrieve secrets and settings.</param>
        /// <returns>A string representing the encoded JWT.</returns>
        /// <remarks>
        /// The token includes claims for ID, Email, Username, and Role to support RBAC.
        /// </remarks>
        public static string GenerateJwtToken(User user, IConfiguration configuration)
        {
            var secretKey = configuration["JwtSettings:Secret"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: configuration["JwtSettings:Issuer"],
                audience: configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Validates if the registration data conflicts with existing users in the repository.
        /// </summary>
        /// <param name="registerDto">The registration data to validate.</param>
        /// <param name="userRepository">The repository instance for data access.</param>
        /// <returns>A task representing the asynchronous validation operation.</returns>
        /// <exception cref="SpectrumBusinessException">Thrown if email or username is already taken.</exception>
        public static async Task ValidateRegisterInput(RegisterDto registerDto, IUserRepository userRepository)
        {
            if (await userRepository.EmailExistsAsync(registerDto.Email))
            {
                throw new SpectrumBusinessException("emailAlreadyRegistered");
            }

            if (await userRepository.UsernameExistsAsync(registerDto.Username))
            {
                throw new SpectrumBusinessException("usernameAlreadyTaken");
            }
        }

        /// <summary>
        /// Validates login credentials and checks for account status.
        /// </summary>
        /// <param name="user">The user found in the database (may be null).</param>
        /// <param name="loginDto">The login attempt data containing the plain-text password.</param>
        /// <exception cref="SpectrumUnauthorizedException">Thrown if credentials match fails.</exception>
        /// <exception cref="SpectrumBusinessException">Thrown if the account is currently suspended.</exception>
        public static void ValidateLoginInput(User user, LoginDto loginDto)
        {
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                throw new SpectrumUnauthorizedException("invalidCredentials");
            }

            if (user.IsSuspended)
            {
                throw new SpectrumUnauthorizedException("accountSuspended");
            }
        }
    }
}
