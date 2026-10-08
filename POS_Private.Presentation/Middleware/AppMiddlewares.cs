using System.Net;
using System.Text.Json;
using POS_Private.Application.DTOs;
using POS_Private.Application.Exceptions;

namespace POS_Private.Presentation.Middleware;

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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

        var response = new APIBaseResponse<object>();
        var statusCode = HttpStatusCode.InternalServerError;

        switch (exception)
        {
            case NotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                response.SetError((int)statusCode, notFoundEx.Message);
                break;

            case BadRequestException badRequestEx:
                statusCode = HttpStatusCode.BadRequest;
                response.SetError((int)statusCode, badRequestEx.Message);
                break;

            case UnauthorizedException unauthEx:
                statusCode = HttpStatusCode.Unauthorized;
                response.SetError((int)statusCode, unauthEx.Message);
                break;

            case CustomValidationException validationEx:
                statusCode = HttpStatusCode.BadRequest;
                response.SetError((int)statusCode, "One or more validation failures have occurred.", validationEx.Errors);
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                response.SetError((int)statusCode, "An unexpected error occurred on the server.");
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        var result = JsonSerializer.Serialize(response, jsonOptions);
        await context.Response.WriteAsync(result);
    }
}

public class StructuredLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<StructuredLoggingMiddleware> _logger;

    public StructuredLoggingMiddleware(RequestDelegate next, ILogger<StructuredLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        var startTime = DateTime.UtcNow;
        _logger.LogInformation("[{CorrelationId}] HTTP {Method} {Path} started", correlationId, context.Request.Method, context.Request.Path);

        await _next(context);

        var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
        _logger.LogInformation("[{CorrelationId}] HTTP {Method} {Path} responded {StatusCode} in {Elapsed:0.00}ms",
            correlationId, context.Request.Method, context.Request.Path, context.Response.StatusCode, elapsedMs);
    }
}

public class LanguageHeaderMiddleware
{
    private readonly RequestDelegate _next;

    public LanguageHeaderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var acceptLanguage = context.Request.Headers["Accept-Language"].ToString();
        var selectedCultureCode = "en";

        if (!string.IsNullOrWhiteSpace(acceptLanguage))
        {
            // Parse comma-separated or primary tag (e.g. "ar", "ar-EG,ar;q=0.9,en;q=0.8")
            var primaryLang = acceptLanguage.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                           .FirstOrDefault()?.Trim().ToLowerInvariant();

            if (primaryLang != null && (primaryLang == "ar" || primaryLang.StartsWith("ar-")))
            {
                selectedCultureCode = "ar";
            }
        }

        var culture = new System.Globalization.CultureInfo(selectedCultureCode);
        System.Globalization.CultureInfo.CurrentCulture = culture;
        System.Globalization.CultureInfo.CurrentUICulture = culture;

        context.Response.Headers["Content-Language"] = selectedCultureCode;

        await _next(context);
    }
}

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }

    public static IApplicationBuilder UseStructuredLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<StructuredLoggingMiddleware>();
    }

    public static IApplicationBuilder UseLanguageHeader(this IApplicationBuilder app)
    {
        return app.UseMiddleware<LanguageHeaderMiddleware>();
    }
}
