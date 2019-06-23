using Microsoft.Extensions.Logging;
using System;

namespace studo.LogsModel
{
    public class LogMessage
    {
        public LogLevel LogLevel { get; set; }
        public EventId EventId { get; set; }
        public string Category { get; set; }
        public string Message { get; set; }
        public DateTime DateTime { get; set; }
        public string DateTimeNormalized =>
            DateTime.ToLocalTime().ToString("d MMM HH:mm:ss");
        public ConsoleColor ForegroundColor
        {
            get
            {
                switch (LogLevel)
                {
                    case LogLevel.Trace:
                        return ConsoleColor.DarkMagenta;
                    case LogLevel.Debug:
                        return ConsoleColor.DarkGray;
                    case LogLevel.Information:
                        return ConsoleColor.DarkGreen;
                    case LogLevel.Warning:
                        return ConsoleColor.Yellow;
                    case LogLevel.Error:
                        return ConsoleColor.Red;
                    case LogLevel.Critical:
                        return ConsoleColor.Red;
                    case LogLevel.None:
                        return ConsoleColor.DarkMagenta;
                    default:
                        return ConsoleColor.DarkMagenta;
                }
            }
        }
    }
}
