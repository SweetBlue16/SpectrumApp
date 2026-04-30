using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Spectrum.API.Exceptions;

namespace Spectrum.API.Middlewares
{
    /// <summary>
    /// Centralized middleware for catching and processing all unhandled exceptions.
    /// Formats error responses into a standardized ProblemDetails JSON.
    /// </summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        /// <summary>
        /// Initializes the handler with logging capabilities.
        /// </summary>
        /// <param name="logger">Logger instance to record error details.</param>
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handles the exception by determining the appropriate HTTP status code and response body.
        /// </summary>
        /// <param name="httpContext">Current HTTP context.</param>
        /// <param name="exception">The intercepted exception.</param>
        /// <param name="cancellationToken">Operation cancellation token.</param>
        /// <returns>True if the exception was handled, false otherwise.</returns>
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An exception occurred: {Message}", exception.Message);

            var problemDetails = new ProblemDetails
            {
                Instance = httpContext.Request.Path
            };

            switch (exception)
            {
                case SpectrumUnauthorizedException ex:
                    httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    problemDetails.Title = "Unauthorized";
                    problemDetails.Status = StatusCodes.Status401Unauthorized;
                    problemDetails.Detail = ex.Message;
                    break;

                case SpectrumNotFoundException ex:
                    httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                    problemDetails.Title = "Resource not found";
                    problemDetails.Status = StatusCodes.Status404NotFound;
                    problemDetails.Detail = ex.Message;
                    break;

                case SpectrumBusinessException ex:
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    problemDetails.Title = "Business validation error";
                    problemDetails.Status = StatusCodes.Status400BadRequest;
                    problemDetails.Detail = ex.Message;
                    break;

                case SpectrumServiceUnavailableException ex:
                    httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                    problemDetails.Title = "Service unavailable";
                    problemDetails.Status = StatusCodes.Status503ServiceUnavailable;
                    problemDetails.Detail = ex.Message;
                    break;

                default: 
                    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    problemDetails.Title = "Internal server error";
                    problemDetails.Status = StatusCodes.Status500InternalServerError;
                    problemDetails.Detail = "An unexpected error occurred while processing the request.";
                    break;
            }

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }
    }
}
