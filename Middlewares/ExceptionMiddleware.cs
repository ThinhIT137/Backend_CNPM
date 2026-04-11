using backend.Exceptions;
using System;
using System.Net;
using System.Text.Json;

namespace backend.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
                _logger.LogError(ex, $"Có lỗi văng ra kìa Dev ơi: {ex.Message}");
                await HandleExceptionAsync(context, ex);
            }
        }

        public static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var message = "Lỗi hệ thống, vui lòng thử lại sau!";
            // 400 (Dữ liệu gửi lên sai/không hợp lệ)
            if (exception is BadRequestException badRequestEx)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                message = badRequestEx.Message;
            }
            // 401
            else if (exception is UnauthorizedException UnauthorizedEx)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                message = UnauthorizedEx.Message;
            }
            // 403
            else if (exception is ForbiddenException ForbiddenEx)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                message = ForbiddenEx.Message;
            }
            // 404 (Không tìm thấy dữ liệu)
            else if (exception is NotFoundException NotFoundEx)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                message = NotFoundEx.Message;
            }
            // 409
            else if (exception is ConflictException conflictEx)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                message = conflictEx.Message;
            }

            var response = new
            {
                success = false,
                message = message,
            };

            var jsonResponse = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(jsonResponse);
        }

    }
}
