using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Spectrum.API.Exceptions;

namespace Spectrum.API.Middlewares
{
    /// <summary>
    /// Acts as the global safety net for the API by intercepting unhandled exceptions across all controllers and services. 
    /// It implements the .NET 8 <see cref="IExceptionHandler"/> interface to map domain-specific exceptions 
    /// to standardized RFC 7807 Problem Details responses. This ensures consistent, predictable, and secure 
    /// error payloads for client applications without leaking sensitive stack traces or internal mechanics.
    /// </summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalExceptionHandler"/> class.
        /// </summary>
        /// <param name="logger">The telemetry and logging service used to safely record exception details, 
        /// stack traces, and internal server errors for monitoring and alerting purposes.</param>
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Intercepts the exception pipeline to evaluate the thrown error. It uses pattern matching to translate 
        /// custom domain exceptions into their corresponding HTTP status codes (e.g., 400 Bad Request, 404 Not Found). 
        /// Unrecognized exceptions default to a secure 500 Internal Server Error payload.
        /// </summary>
        /// <param name="httpContext">The encapsulated HTTP context containing the current request and response streams.</param>
        /// <param name="exception">The unhandled exception that bubbled up from the application pipeline.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests from the client.</param>
        /// <returns>A <see cref="ValueTask{Boolean}"/> containing <c>true</c> to indicate the exception was successfully 
        /// mapped, formatted, and written to the response stream, thereby short-circuiting any further exception-handling middleware.</returns>
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

                case SpectrumForbiddenException ex:
                    httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                    problemDetails.Title = "Forbidden";
                    problemDetails.Status = StatusCodes.Status403Forbidden;
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
