using Microsoft.AspNetCore.Diagnostics;
using Spectrum.API.Exceptions;
using System.Net;

namespace Spectrum.API.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var response = new
            {
                Error = exception is SpectrumBusinessException ||
                        exception is SpectrumNotFoundException ||
                        exception is SpectrumUnauthorizedException
                        ? exception.Message
                        : "An internal server error has occurred."
            };

            httpContext.Response.StatusCode = exception switch
            {
                SpectrumBusinessException => (int)HttpStatusCode.BadRequest,
                SpectrumUnauthorizedException => (int)HttpStatusCode.Unauthorized,
                SpectrumNotFoundException => (int)HttpStatusCode.NotFound,
                _ => (int)HttpStatusCode.InternalServerError
            };

            if (httpContext.Response.StatusCode == (int)HttpStatusCode.InternalServerError)
            {
                _logger.LogError(exception, "Unhandled critical system error.");
            }
            else
            {
                _logger.LogWarning("Violation of a business rule: {Message}", exception.Message);
            }

            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            return true;
        }
    }
}
