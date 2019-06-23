using Newtonsoft.Json;
using studo.LogsModel;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace studo.LogsViewer
{
    class Program
    {
        private const string secretFileName = "Logs.Secret.txt";
        static async Task Main(string[] args)
        {
            var wsClient = new ClientWebSocket();
            var logsOptions = new LogsOptions(secretFileName);

            wsClient.Options.SetRequestHeader("Authorization", logsOptions.SecretKey);
            await wsClient.ConnectAsync(new Uri(logsOptions.Url), CancellationToken.None);

            var array = new byte[1024];
            Console.WriteLine("Ready to listen\n");
            while(true)
            {
                var result = await wsClient.ReceiveAsync(array, CancellationToken.None);
                if (result.MessageType != WebSocketMessageType.Text)
                {
                    Console.WriteLine("Not text received");
                    continue;
                }
                var text = Encoding.UTF8.GetString(array, 0, result.Count);
                var logMessage = JsonConvert.DeserializeObject<LogMessage>(text);
                WriteReport(logMessage);
            }
        }

        private static void WriteReport(LogMessage logMessage)
        {
            Console.ForegroundColor = logMessage.ForegroundColor;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write($"{logMessage.LogLevel}: ");
            Console.ResetColor();
            Console.WriteLine($"{logMessage.EventId.Name}[{logMessage.EventId.Id}]");
            Console.WriteLine(logMessage.Message);
        }
    }
}
