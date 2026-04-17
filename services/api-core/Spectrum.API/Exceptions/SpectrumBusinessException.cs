namespace Spectrum.API.Exceptions
{
    public class SpectrumBusinessException : Exception
    {
        public SpectrumBusinessException(string message) : base(message)
        {
        }
        public SpectrumBusinessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
