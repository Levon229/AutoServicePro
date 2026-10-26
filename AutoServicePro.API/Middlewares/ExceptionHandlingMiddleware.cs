using System.Net;
using System.Text.Json;

namespace AutoServicePro.API.Middlewares;

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
            _logger.LogError(ex, ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = exception switch
        {
            KeyNotFoundException => HttpStatusCode.NotFound,          // 404
            ArgumentException => HttpStatusCode.BadRequest,           // 400
            InvalidOperationException => HttpStatusCode.BadRequest,   // 400
            _ => HttpStatusCode.InternalServerError                   // 500
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = context.Response.StatusCode,
            error = exception.GetType().Name,
            message = exception.Message
        };

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}