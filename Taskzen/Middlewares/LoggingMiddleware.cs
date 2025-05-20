namespace Taskzen.API.Middlewares;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;
    
    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger){
    {
        _next = next;
        _logger = logger;
    }}
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            _logger.LogInformation("Handling request: {method} {url}",
                context.Request.Method,
                context.Request.Path);

            await _next(context);

            _logger.LogInformation("Finished handling request. Status Code: {statusCode}.",
                context.Response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while handling the request.");
            throw;
        }
    }
}

public static class LoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<LoggingMiddleware>();
    }
}