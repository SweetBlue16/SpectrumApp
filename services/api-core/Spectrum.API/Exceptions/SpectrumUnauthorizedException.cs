namespace Spectrum.API.Exceptions
{
    /// <summary>
    /// Exception thrown when an action requires authentication but the user is not authenticated, 
    /// has provided an invalid/expired token, or their account is suspended. 
    /// The GlobalExceptionHandler intercepts this and maps it to an HTTP 401 (Unauthorized) response.
    /// </summary>
    public class SpectrumUnauthorizedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpectrumUnauthorizedException"/> class.
        /// </summary>
        /// <param name="message">The specific reason for the authorization failure (e.g., "invalid credentials").</param>
        public SpectrumUnauthorizedException(string message) : base(message)
        {
        }
    }
}
