using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketServer.Middleware
{
    public class ServerLogic
    {
        public static async Task RunSend()
        {
            while (!ServerMiddleware.sendQueue.IsEmpty)
            {
                if (ServerMiddleware.sendQueue.TryDequeue(out ServerRequest request))
                {
                    var buffer = Encoding.UTF8.GetBytes(request.message);
                    await request.webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("HOW DAFQ THIS HAPPEND, THE SEND QUEUE WAS ALLREADY EMPTY??");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }
    }
}
