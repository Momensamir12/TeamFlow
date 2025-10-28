using App.Application.Exceptions;

namespace App.Api.Middleware;

public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;

    public GlobalExceptionHandler(RequestDelegate next)
    {
        _next = next;
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

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            TaskNotFoundException ex => new
            {
                statusCode = StatusCodes.Status404NotFound,
                message = ex.Message,
                type = "Not Found"
            },
            UnauthorizedAccessException ex => new
            {
                statusCode = StatusCodes.Status403Forbidden,
                message = ex.Message,
                type = "Forbidden"
            },
            ArgumentException ex => new
            {
                statusCode = StatusCodes.Status400BadRequest,
                message = ex.Message,
                type = "Bad Request"
            },
            _ => new
            {
                statusCode = StatusCodes.Status500InternalServerError,
                message = "An unexpected error occurred",
                type = "Internal Server Error"
            }
        };

        context.Response.StatusCode = response.statusCode;
        return context.Response.WriteAsJsonAsync(response);
    }
}