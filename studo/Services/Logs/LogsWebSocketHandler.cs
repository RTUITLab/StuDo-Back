using Newtonsoft.Json;
using studo.LogsModel;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace studo.Services.Logs
{
    public class LogsWebSocketHandler : ILogsWebSocketHandler
    {
        private readonly HashSet<WebSocket> webSockets = new HashSet<WebSocket>();

        private static LogsWebSocketHandler instance;
        public static LogsWebSocketHandler Instance => instance ?? (instance = new LogsWebSocketHandler());

        private LogsWebSocketHandler() { }

        public async Task HandleWebSocket(WebSocket webSocket)
        {
            webSockets.Add(webSocket);
            while (webSocket.State == WebSocketState.Open)
            {
                await Task.Delay(10000);
            }
            webSockets.Remove(webSocket);
        }

        public void SendLogMessage(LogMessage logMessage)
        {
            var strMessage = JsonConvert.SerializeObject(logMessage);
            foreach (var socket in webSockets)
            {
                try
                {
                    socket.SendAsync(Encoding.UTF8.GetBytes(strMessage), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (Exception) { }
            }
        }
    }
}
