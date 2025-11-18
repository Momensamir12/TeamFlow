using App.Application.Exceptions;
using Microsoft.Extensions.Logging;

namespace App.Api.Middleware;

public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionHandler(
        RequestDelegate next, 
        ILogger<GlobalExceptionHandler> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, type) = exception switch
        {
            TaskNotFoundException ex => (StatusCodes.Status404NotFound, ex.Message, "Not Found"),
            UnauthorizedAccessException ex => (StatusCodes.Status403Forbidden, ex.Message, "Forbidden"),
            ArgumentException ex => (StatusCodes.Status400BadRequest, ex.Message, "Bad Request"),
            InvalidOperationException ex => (StatusCodes.Status400BadRequest, ex.Message, "Bad Request"),
            KeyNotFoundException ex => (StatusCodes.Status404NotFound, ex.Message, "Not Found"),
            _ => (StatusCodes.Status500InternalServerError, 
                  "An error occurred while processing your request.", 
                  "Internal Server Error")
        };

        // Build response
        var response = new
        {
            statusCode,
            message,
            type,
            details = _env.IsDevelopment() ? exception.StackTrace : null  // Only in dev
        };

        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(response);
    }
}