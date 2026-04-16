namespace Spectrum.API.Exceptions
{
    public class SpectrumNotFoundException : Exception
    {
        public SpectrumNotFoundException(string entityName, object key)
            : base($"Could not find '{entityName}' with identifier ({key}).")
        {
        }
    }
}
