using System;

namespace Spectrum.API.Exceptions
{
    /// <summary>
    /// Exception thrown when a file uploaded by the user does not meet the validation criteria (e.g., size, format, duration).
    /// </summary>
    public class SpectrumFileValidationException : SpectrumBusinessException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpectrumFileValidationException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SpectrumFileValidationException(string message) : base(message)
        {
        }
    }
}