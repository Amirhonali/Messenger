using System.Net;
using System.Text.Json;
using Messenger.Application.Exceptions;

namespace Messenger.Api.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка: {Message}", ex.Message);

                context.Response.ContentType = "application/json";
                var statusCode = HttpStatusCode.InternalServerError;

                switch (ex)
                {
                    case NotFoundException:
                        statusCode = HttpStatusCode.NotFound; // 404
                        break;
                    case ValidationException:
                        statusCode = HttpStatusCode.BadRequest; // 400
                        break;
                    case ForbiddenException:
                        statusCode = HttpStatusCode.Forbidden; // 403
                        break;
                    case UnauthorizedAccessException:
                        statusCode = HttpStatusCode.Unauthorized; // 401
                        break;
                }

                context.Response.StatusCode = (int)statusCode;

                var response = new
                {
                    status = (int)statusCode,
                    error = ex.Message
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}