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

        var (statusCode, message) = exception switch
        {
            TaskNotFoundException => (StatusCodes.Status404NotFound, "Task not found."),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "You do not have permission to perform this action."),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request parameters."),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "The requested resource was not found."),
            InvalidOperationException when !exception.Message.Contains("Unable to resolve service") 
                => (StatusCodes.Status400BadRequest, "This operation cannot be performed at this time."),
            _ => (StatusCodes.Status500InternalServerError, 
                  "An error occurred while processing your request. Please try again later.")
        };

        _logger.LogError(exception, "Exception: {ExceptionType} - {ExceptionMessage}", exception.GetType().Name, exception.Message);

        var response = new
        {
            success = false,
            message = message,
            errors = (object?)null
        };

        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(response);
    }
}