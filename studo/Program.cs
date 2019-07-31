using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using studo.Services.Logs;

namespace studo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();
            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(cfg => cfg.AddJsonFile("appsettings.Secret.json"))
                .ConfigureLogging(cfg =>
                {
                    //cfg.ClearProviders();
                    cfg.AddProvider(new WebSocketLoggerProvider());
                })
                .UseStartup<Startup>()
                .UseSerilog((context, configuration) =>
                {
                    configuration
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                        .Enrich.FromLogContext()
                        .WriteTo.File(path: Path.Combine("Logs", "log-.txt"),
                            rollingInterval: RollingInterval.Month,
                            outputTemplate: "{Timestamp:d MMM HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
                        .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} {Level:w3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                            theme: AnsiConsoleTheme.Literate);
                        //.WriteTo.Providers(new WebSocketLoggerProvider());
                });
    }
}
