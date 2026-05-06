namespace Spectrum.API.Exceptions
{
    /// <summary>
    /// Exception thrown when an authenticated user is not allowed to perform an action.
    /// The global exception handler maps this to HTTP 403 Forbidden.
    /// </summary>
    public class SpectrumForbiddenException : Exception
    {
        public SpectrumForbiddenException(string message) : base(message)
        {
        }
    }
}
