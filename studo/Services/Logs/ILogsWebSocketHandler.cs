using System.Net.WebSockets;
using System.Threading.Tasks;

namespace studo.Services.Logs
{
    public interface ILogsWebSocketHandler
    {
        Task HandleWebSocket(WebSocket webSocket);
    }
}
