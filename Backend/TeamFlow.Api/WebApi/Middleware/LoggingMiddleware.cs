using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace App.Api.Middleware;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    // Sensitive fields to redact
    private static readonly HashSet<string> SensitiveFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "password",
        "confirmPassword",
        "oldPassword",
        "newPassword",
        "token",
        "refreshToken",
        "accessToken",
        "apiKey",
        "secret",
    };

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString();
        
        await LogRequest(context, requestId);

        var originalBodyStream = context.Response.Body;

        try
        {
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            stopwatch.Stop();

            await LogResponse(context, stopwatch.ElapsedMilliseconds, requestId);

            await responseBody.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task LogRequest(HttpContext context, string requestId)
    {
        context.Request.EnableBuffering();

        var body = await ReadBodyAsync(context.Request.Body);
        context.Request.Body.Position = 0;

        // Sanitize sensitive data
        var sanitizedBody = SanitizeBody(body);

        _logger.LogInformation(
            "HTTP Request [{RequestId}] {Method} {Path} {QueryString} | UserId: {UserId} | Body: {Body}",
            requestId,
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            context.User?.FindFirst("sub")?.Value ?? "Anonymous",
            sanitizedBody
        );
    }

    private async Task LogResponse(HttpContext context, long elapsedMs, string requestId)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await ReadBodyAsync(context.Response.Body);
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        // Sanitize sensitive data in response too
        var sanitizedBody = SanitizeBody(body);

        var logLevel = context.Response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

        _logger.Log(
            logLevel,
            "HTTP Response [{RequestId}] {StatusCode} | Duration: {ElapsedMs}ms | Body: {Body}",
            requestId,
            context.Response.StatusCode,
            elapsedMs,
            sanitizedBody
        );
    }

    private static async Task<string> ReadBodyAsync(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        return string.IsNullOrEmpty(body) ? "[Empty]" : body;
    }

    private static string SanitizeBody(string body)
    {
        if (string.IsNullOrEmpty(body) || body == "[Empty]")
            return body;

        try
        {
            // Try to parse as JSON
            var jsonDocument = JsonDocument.Parse(body);
            var sanitized = SanitizeJsonElement(jsonDocument.RootElement);
            return JsonSerializer.Serialize(sanitized);
        }
        catch
        {
            // If not JSON, return redacted placeholder
            return "[Non-JSON content]";
        }
    }

    private static object SanitizeJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var obj = new Dictionary<string, object>();
                foreach (var property in element.EnumerateObject())
                {
                    var key = property.Name;
                    var value = property.Value;

                    // Check if field is sensitive
                    if (SensitiveFields.Contains(key))
                    {
                        obj[key] = "***REDACTED***";
                    }
                    else
                    {
                        obj[key] = SanitizeJsonElement(value);
                    }
                }
                return obj;

            case JsonValueKind.Array:
                return element.EnumerateArray()
                    .Select(SanitizeJsonElement)
                    .ToList();

            case JsonValueKind.String:
                return element.GetString() ?? "";

            case JsonValueKind.Number:
                return element.GetDouble();

            case JsonValueKind.True:
                return true;

            case JsonValueKind.False:
                return false;

            case JsonValueKind.Null:
                return null!;

            default:
                return element.ToString();
        }
    }
}