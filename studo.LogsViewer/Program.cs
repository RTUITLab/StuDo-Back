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

            var array = new byte[1024];
            await wsClient.ConnectAsync(new Uri(logsOptions.Url), CancellationToken.None);
            while(true)
            {
                var result = await wsClient.ReceiveAsync(array, CancellationToken.None);
                if (result.MessageType != WebSocketMessageType.Text)
                {
                    Console.WriteLine("Not text received");
                    continue;
                }
                var text = Encoding.UTF8.GetString(array, 0, result.Count);
                Console.WriteLine(text);
            }
        }
    }
}
