using System.Net;
using System.Text.Json;
using Application.Common.Exceptions;

namespace API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unhandled exception occurred. TraceId: {TraceId}", context.TraceIdentifier);
                await HandleExceptionAsync(context, exception);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, code, message) = exception switch
            {
                ValidationException validationException => (
                    (int)HttpStatusCode.BadRequest,
                    "validation_error",
                    validationException.Message),
                NotFoundException notFoundException => (
                    (int)HttpStatusCode.NotFound,
                    "not_found",
                    notFoundException.Message),
                ConflictException conflictException => (
                    (int)HttpStatusCode.Conflict,
                    "conflict",
                    conflictException.Message),
                _ => (
                    (int)HttpStatusCode.InternalServerError,
                    "server_error",
                    "An unexpected error occurred.")
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var payload = new
            {
                error = new
                {
                    code,
                    message,
                    traceId = context.TraceIdentifier
                }
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}
