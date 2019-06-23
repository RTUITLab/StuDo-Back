using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace studo.Services.Logs
{
    public class WebSocketLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new WebSocketLogger(categoryName);
        }

        public void Dispose()
        {
        }
    }
}
