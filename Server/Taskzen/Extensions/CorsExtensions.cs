namespace Taskzen.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddCorsOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var corsOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                // policy.WithOrigins(corsOrigins)
                policy.AllowAnyOrigin()
                    .AllowAnyMethod();
            });
        });
        
        return services;
    }
}