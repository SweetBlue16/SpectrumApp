namespace Spectrum.API.Exceptions
{
    /// <summary>
    /// Exception thrown when a downstream dependency (such as the RAWG external API, Firebase, 
    /// or internal gRPC microservices) is unreachable, times out, or returns a server error.
    /// The GlobalExceptionHandler intercepts this and maps it to an HTTP 503 (Service Unavailable) response.
    /// </summary>
    public class SpectrumServiceUnavailableException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpectrumServiceUnavailableException"/> class 
        /// with a standardized default message.
        /// </summary>
        public SpectrumServiceUnavailableException() 
            : base("The requested service is currently unavailable. Please try again later.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpectrumServiceUnavailableException"/> class.
        /// </summary>
        /// <param name="message">The specific message detailing which service failed.</param>
        public SpectrumServiceUnavailableException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpectrumServiceUnavailableException"/> class 
        /// while preserving the stack trace of the original failing dependency.
        /// </summary>
        /// <param name="message">The specific message detailing which service failed.</param>
        /// <param name="innerException">The original exception thrown by the failing underlying service (e.g., HttpRequestException).</param>
        public SpectrumServiceUnavailableException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
