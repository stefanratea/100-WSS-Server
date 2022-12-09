using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace WebSocketServer.Middleware
{
    public static class ServerMiddlewareExtentions
    {
        public static IApplicationBuilder UseWebSocketServer(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ServerMiddleware>();
        }

        public static IServiceCollection AddServerManager(this IServiceCollection services)
        {
            services.AddSingleton<ServerManager>();
            return services;
        }
    }
}
