using Microsoft.Extensions.Logging;
using System;

namespace studo.LogsModel
{
    public class LogMessage
    {
        public LogLevel LogLevel { get; set; }
        public EventId EventId { get; set; }
        public string Message { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime DateTimeNormalized =>
            DateTime.ToLocalTime();
        public ConsoleColor ForegroundColor
        {
            get
            {
                switch (LogLevel)
                {
                    case LogLevel.Trace:
                        return ConsoleColor.Gray;
                    case LogLevel.Debug:
                        return ConsoleColor.Gray;
                    case LogLevel.Information:
                        return ConsoleColor.Green;
                    case LogLevel.Warning:
                        return ConsoleColor.Yellow;
                    case LogLevel.Error:
                        return ConsoleColor.Red;
                    case LogLevel.Critical:
                        return ConsoleColor.Red;
                    case LogLevel.None:
                        return ConsoleColor.Gray;
                    default:
                        return ConsoleColor.Gray;
                }
            }
        }
    }
}
