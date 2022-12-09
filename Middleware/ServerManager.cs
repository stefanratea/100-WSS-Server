using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;

namespace WebSocketServer.Middleware
{
    public class ServerManager
    {
        private ConcurrentDictionary<string, Client> clients = new ConcurrentDictionary<string, Client>();

        public ConcurrentDictionary<string, Client> GetAllSockets()
        {
            return clients;
        }
        public Client AddSocket(WebSocket socket)
        {
            string connID = Guid.NewGuid().ToString();
            var client = new Client(socket, connID);
            clients.TryAdd(connID, client);
            Console.WriteLine($"Connection added : {connID}");
            return client;
        }
    }
}
