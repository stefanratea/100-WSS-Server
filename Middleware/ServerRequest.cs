using System;
using System.Net.WebSockets;

namespace WebSocketServer.Middleware
{
    public class ServerRequest
    {
        public WebSocket webSocket;
        public string message;

        public ServerRequest(WebSocket client, string jsonMessage)
        {
            webSocket = client;
            message = jsonMessage;
        }
    }
}
