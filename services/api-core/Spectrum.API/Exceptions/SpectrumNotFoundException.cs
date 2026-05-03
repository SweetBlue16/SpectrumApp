namespace Spectrum.API.Exceptions
{
    /// <summary>
    /// Exception thrown when a requested domain entity (e.g., User, Game, Review) cannot be found 
    /// in the local database or external catalog. 
    /// The GlobalExceptionHandler intercepts this and maps it to an HTTP 404 (Not Found) response.
    /// </summary>
    public class SpectrumNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpectrumNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The message detailing which resource was not found.</param>
        public SpectrumNotFoundException(string message) : base(message)
        {
        }
    }
}
