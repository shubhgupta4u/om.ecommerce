using Microsoft.AspNetCore.Builder;

namespace om.shared.api.middlewares
{
    public static class LoggerMiddlewareExtension
    {
        public static IApplicationBuilder UseLoggerMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggerMiddleware>();
        }
    }
}
