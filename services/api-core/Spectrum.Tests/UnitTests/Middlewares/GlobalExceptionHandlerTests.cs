using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Spectrum.API.Exceptions;
using Spectrum.API.Middlewares;
using System.Text.Json;

namespace Spectrum.Tests.UnitTests.Middlewares
{
    public class GlobalExceptionHandlerTests
    {
        private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock;
        private readonly GlobalExceptionHandler _handler;
        
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public GlobalExceptionHandlerTests()
        {
            _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
            _handler = new GlobalExceptionHandler(_loggerMock.Object);
        }

        [Fact]
        public async Task TestTryHandleAsyncWhenSpectrumUnauthorizedExceptionShouldReturn401ProblemDetails()
        {
            var context = SetupHttpContext();
            var exception = new SpectrumUnauthorizedException("Invalid token");

            var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);
            var problemDetails = await GetProblemDetailsFromResponse(context);

            Assert.True(result);
            Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
            Assert.NotNull(problemDetails);
            Assert.Equal("Unauthorized", problemDetails.Title);
            Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails.Status);
            Assert.Equal(exception.Message, problemDetails.Detail);
            Assert.Equal(context.Request.Path, problemDetails.Instance);
        }

        [Fact]
        public async Task TestTryHandleAsyncWhenSpectrumNotFoundExceptionShouldReturn404ProblemDetails()
        {
            var context = SetupHttpContext();
            var exception = new SpectrumNotFoundException("User not found");

            var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);
            var problemDetails = await GetProblemDetailsFromResponse(context);

            Assert.True(result);
            Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
            Assert.NotNull(problemDetails);
            Assert.Equal("Resource not found", problemDetails.Title);
            Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
            Assert.Equal(exception.Message, problemDetails.Detail);
        }

        [Fact]
        public async Task TestTryHandleAsyncWhenSpectrumForbiddenExceptionShouldReturn403ProblemDetails()
        {
            var context = SetupHttpContext();
            var exception = new SpectrumForbiddenException("Forbidden action");

            var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);
            var problemDetails = await GetProblemDetailsFromResponse(context);

            Assert.True(result);
            Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
            Assert.NotNull(problemDetails);
            Assert.Equal("Forbidden", problemDetails.Title);
            Assert.Equal(StatusCodes.Status403Forbidden, problemDetails.Status);
            Assert.Equal(exception.Message, problemDetails.Detail);
        }

        [Fact]
        public async Task TestTryHandleAsyncWhenSpectrumBusinessExceptionShouldReturn400ProblemDetails()
        {
            var context = SetupHttpContext();
            var exception = new SpectrumBusinessException("Email already exists");

            var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);
            var problemDetails = await GetProblemDetailsFromResponse(context);

            Assert.True(result);
            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
            Assert.NotNull(problemDetails);
            Assert.Equal("Business validation error", problemDetails.Title);
            Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
            Assert.Equal(exception.Message, problemDetails.Detail);
        }

        [Fact]
        public async Task TestTryHandleAsyncWhenSpectrumServiceUnavailableExceptionShouldReturn503ProblemDetails()
        {
            var context = SetupHttpContext();
            var exception = new SpectrumServiceUnavailableException("RAWG API is down");

            var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);
            var problemDetails = await GetProblemDetailsFromResponse(context);

            Assert.True(result);
            Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
            Assert.NotNull(problemDetails);
            Assert.Equal("Service unavailable", problemDetails.Title);
            Assert.Equal(StatusCodes.Status503ServiceUnavailable, problemDetails.Status);
            Assert.Equal(exception.Message, problemDetails.Detail);
        }

        [Fact]
        public async Task TestTryHandleAsyncWhenGenericExceptionShouldReturn500ProblemDetails()
        {
            var context = SetupHttpContext();
            var exception = new Exception("A massive database failure occurred");

            var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);
            var problemDetails = await GetProblemDetailsFromResponse(context);

            Assert.True(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
            Assert.NotNull(problemDetails);
            Assert.Equal("Internal server error", problemDetails.Title);
            Assert.Equal(StatusCodes.Status500InternalServerError, problemDetails.Status);
            Assert.Equal("An unexpected error occurred while processing the request.", problemDetails.Detail);
        }

        private static DefaultHttpContext SetupHttpContext()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/test-endpoint";
            context.Response.Body = new MemoryStream();
            return context;
        }

        private static async Task<ProblemDetails?> GetProblemDetailsFromResponse(DefaultHttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var bodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            return JsonSerializer.Deserialize<ProblemDetails>(bodyText, _jsonOptions);
        }
    }
}
