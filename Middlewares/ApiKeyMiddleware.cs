using Microsoft.AspNetCore.Http;

namespace backend.Middlewares
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;
        private readonly ILogger<ApiKeyMiddleware> _logger;
        public ApiKeyMiddleware(RequestDelegate next, IConfiguration config, ILogger<ApiKeyMiddleware> logger)
        {
            _next = next;
            _config = config;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            if (context.Request.Path.StartsWithSegments("/auth"))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue("x-api-key", out var apiKey))
            {
                _logger.LogWarning("🚨 CẢNH BÁO: Phát hiện request KHÔNG CÓ API Key truy cập vào endpoint {Path}", context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "Missing API Key",
                });
                return;
            }

            if (apiKey != _config["API_KEY"])
            {
                _logger.LogWarning("🚨 CẢNH BÁO: Bắt quả tang request dùng API Key SAI (Key: {ApiKey}) truy cập vào {Path}", apiKey, context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "Invalid API Key",
                });
                return;
            }

            await _next(context);
        }
    }
}
