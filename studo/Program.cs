using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Sinks.SystemConsole.Themes;
using studo.Services.Logs;

namespace studo
{
    public class Program
    {
        private static readonly LoggerProviderCollection Providers = new LoggerProviderCollection();

        public static void Main(string[] args)
        {
            Providers.AddProvider(new WebSocketLoggerProvider());
            var host = CreateWebHostBuilder(args).Build();
            host.Run();
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureAppConfiguration(cfg => cfg.AddJsonFile("appsettings.Secret.json", true));
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseSerilog((context, configuration) =>
                    {
                        configuration
                            .MinimumLevel.Information()
                            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                            .Enrich.FromLogContext()
                            .WriteTo.File(path: Path.Combine("Logs", "log-.txt"),
                                rollingInterval: RollingInterval.Day,
                                outputTemplate: "{Timestamp:d MMM yyyy HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                                restrictedToMinimumLevel: LogEventLevel.Information)
                            .WriteTo.Console(outputTemplate: "{Timestamp:d MMM HH:mm:ss} {Level:w3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                                theme: AnsiConsoleTheme.Literate)
                                .WriteTo.Providers(Providers);
                    });
                });
    }
}
