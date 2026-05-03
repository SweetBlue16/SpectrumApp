namespace Spectrum.API.Exceptions
{
    /// <summary>
    /// Exception thrown when a specific business rule or domain constraint is violated 
    /// (e.g., "Email is already registered" or "Missing required parameters").
    /// The GlobalExceptionHandler intercepts this and maps it to an HTTP 400 (Bad Request) response.
    /// </summary>
    public class SpectrumBusinessException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpectrumBusinessException"/> class.
        /// </summary>
        /// <param name="message">The message describing the business rule that was violated.</param>
        public SpectrumBusinessException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpectrumBusinessException"/> class 
        /// along with the underlying exception that caused the business rule failure.
        /// </summary>
        /// <param name="message">The message describing the business rule that was violated.</param>
        /// <param name="innerException">The underlying exception that triggered this business validation failure.</param>
        public SpectrumBusinessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
