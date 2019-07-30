using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using studo.Services.Logs;

namespace studo.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class LogsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogsWebSocketHandler logsHandler;
        private readonly string path;
        private readonly string secret;

        public LogsMiddleware(RequestDelegate next, ILogsWebSocketHandler logsHandler,
            string path, string secret)
        {
            _next = next;
            this.logsHandler = logsHandler;
            this.path = path;
            this.secret = secret;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.WebSockets.IsWebSocketRequest)
            {
                if (httpContext.Request.Path.StartsWithSegments(path) && httpContext.Request.Headers.TryGetValue("Authorization", out var authValue) && authValue == secret)
                {
                    await logsHandler.HandleWebSocket(await httpContext.WebSockets.AcceptWebSocketAsync());
                }
            }
            await  _next(httpContext);
        }
    }

    // Extension method used to add the LogsMiddleware to the HTTP request pipeline.
    public static class LogsMiddlewareExtensions
    {
        public static IApplicationBuilder UseLogsMiddleware(this IApplicationBuilder builder, string path, string secret)
        {
            return builder.UseMiddleware<LogsMiddleware>(path, secret);
        }
    }
}
