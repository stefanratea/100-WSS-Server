using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using WebSocketServer.GameServerLogic;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace WebSocketServer.Middleware
{
    public class ServerMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ServerManager _manager;

        private static UdpClient udpListener;

        public delegate Task<Packet> PacketHandler(Client fromClient, Packet packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        public delegate Task<string> MessageHandler(Client fromClient, string content);
        public static Dictionary<int, MessageHandler> Handlers;

        public static ConcurrentQueue<ServerRequest> sendQueue { get; set; }

        public ServerMiddleware(RequestDelegate next, ServerManager manager)
        {
            Console.WriteLine("/////INSTANTIATED SERVERMIDDLEWARE//////");
            _next = next;
            _manager = manager;

            udpListener = new UdpClient(3998);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            sendQueue = new ConcurrentQueue<ServerRequest>();
            InitializeServerData();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            PrintRequestParameters(context);
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket ws = await context.WebSockets.AcceptWebSocketAsync();
                Console.WriteLine("Websocket connected");

                Client requestClient = _manager.AddSocket(ws);

                ServerWelcome welcome = new ServerWelcome
                {
                    welcomeMessage = "ayyy ma boy! welcome to the server!",
                    connID = requestClient.connID,
                };

                await SendMessageAsync(ws, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.welcome, welcome)));

                await ReceivedMessage(ws, async (result, buffer) =>{
                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Binary:
                            break;

                        case WebSocketMessageType.Text:
                            Console.WriteLine($"{requestClient.connID.Remove(4, requestClient.connID.Length - 5)} : " + Encoding.UTF8.GetString(buffer, 0, result.Count));
                            MessageWrap message = JsonConvert.DeserializeObject<MessageWrap>(Encoding.UTF8.GetString(buffer, 0, result.Count));
                            string responseMessage = await Handlers[message.requestID](requestClient, message.jsonContent);
                            if(responseMessage != null) await SendMessageAsync(ws, responseMessage);
                            else Console.WriteLine("NULL RESPONSE\n");
                            break;

                        case WebSocketMessageType.Close:
                            string closeID = _manager.GetAllSockets().FirstOrDefault(s => s.Value.webSocket == ws).Key;
                            Console.WriteLine($"Close with {result.CloseStatusDescription} from {closeID}");
                            if (_manager.GetAllSockets().TryRemove(closeID, out Client client))
                            {
                                foreach(KeyValuePair<string, Match> match in MatchMaking.OnGoingMatches)
                                {
                                    if (match.Value.player1.Equals(client)) match.Value.player1Disconnected = true;
                                    if (match.Value.player2.Equals(client)) match.Value.player2Disconnected = true;
                                }
                                await client.webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("dafq happend bro, you try to close a socket that doesn't exist");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            break;
                    }
                });
            }
            else
            {
                Console.WriteLine($"Not a websocket connecttion {context.Request.Protocol}");
                await _next(context);
            }
        }
        private class MessageWrap
        {
            public int requestID;
            public string jsonContent;

            public MessageWrap(int ID, object content)
            {
                requestID = ID;
                jsonContent = JsonConvert.SerializeObject(content);
            }
        }
        private class ServerWelcome
        {
            public string welcomeMessage;
            public string connID;
        }
        #region WS
        /// <summary>
        /// Send & Receive WS
        /// </summary>
        private async Task SendMessageAsync(WebSocket socket, string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        private async Task SendPacketAsync(WebSocket socket, Packet packet)
        {
            packet.WriteLength();
            var buffer = packet.ToArray();
            await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        public static void AddMessageToQueue(WebSocket client, string message)
        {
            sendQueue.Enqueue(new ServerRequest(client, message));
        }
        private async Task ReceivedMessage(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];

            while (socket.State == WebSocketState.Open)
            {
                try
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    handleMessage(result, buffer);
                }catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        private async Task HandleData(byte[] _data, Client _client, Action<Packet> response)
        {
            int _packetLength = 0;

            Packet receivedData = new Packet();
            receivedData.SetBytes(_data);

            if (receivedData.UnreadLength() >= 4)
            {
                _packetLength = receivedData.ReadInt();
                if (_packetLength <= 0)
                {
                    return; //ex true
                }
            }

            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
            {
                byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                using (Packet _packet = new Packet(_packetBytes))
                {
                    int _packetId = _packet.ReadInt();
                    response(await packetHandlers[_packetId](_client, _packet));
                    /*if (aux != null) response(aux);/* else Console.WriteLine("THE ACTUAL FUCK, NULL PACKET CAME BACK");*/
                    
                }

                _packetLength = 0;
                /*
                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }
                */
            }

            if (_packetLength <= 1)
            {
                return; //ex true
            }
            return; //ex false
        }
        public void PrintRequestParameters(HttpContext context)
        {
            Console.WriteLine("Req Method: " + context.Request.Method);
            Console.WriteLine("Req isHttps: " + context.Request.IsHttps);
            foreach (var x in context.Request.Headers)
            {
                Console.WriteLine("--> " + x.Key + " : " + x.Value);
            }
            Console.WriteLine("\n");
        }
        #endregion
        #region UDP
        /// <summary>
        /// Send & Receive UDP
        /// </summary>
        public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
        {
            _packet.WriteLength();
            try
            {
                if (_clientEndPoint != null)
                {
                    udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error sending data to {_clientEndPoint} via UDP: {_ex}");
            }
        }
        private void UDPReceiveCallback(IAsyncResult _result)
        {
            try
            {
                IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (_data.Length < 4)
                {
                    return;
                }
                using (Packet _packet = new Packet(_data))
                {
                    string _clientId = _packet.ReadString();
                    //Console.WriteLine($"Received something UDP : {_clientId}");
                    if (_clientId == null)
                    {
                        Console.WriteLine("WTF EMPTY UDP CALLBACK");
                        return;
                    }
                    Client client = _manager.GetAllSockets().FirstOrDefault(s => s.Key == _clientId).Value;
                    if (client.udpSocket.endPoint == null)
                    {
                        client.udpSocket.Connect(_clientEndPoint);
                        return;
                    }

                    if (client.udpSocket.endPoint.ToString() == _clientEndPoint.ToString())
                    {
                        client.udpSocket.HandleData(client, _packet);
                    }
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error receiving UDP data: {_ex}");
            }
        }
        #endregion
        private static void InitializeServerData()
        {
            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.updTestReceived, ServerHandle.UDPTestReceived },
                { (int)ClientPackets.Emotion, ServerHandle.UpdateClientEmotion },
                { (int)ClientPackets.PlayerPosition, ServerHandle.SyncPlayer },
                { (int)ClientPackets.ObjectPosition, ServerHandle.SyncObject }
            };
            Console.WriteLine("Initialized packets.");
            Handlers = new Dictionary<int, MessageHandler>()
            {
                { (int)ClientPackets.welcomeReceived, ServerRequestHandle.WelcomeReceived },
                { (int)ClientPackets.QueueUp, ServerRequestHandle.QueueUp },
                { (int)ClientPackets.SceneWasLoaded, ServerRequestHandle.SceneWasLoaded },
                { (int)ClientPackets.PlayerSpawned, ServerRequestHandle.PlayerWasSpawned },
                { (int)ClientPackets.GameEnded, ServerRequestHandle.OneGameEnded },
                { (int)ClientPackets.ObjectSpawned, ServerRequestHandle.ObjectSpawned },
                { (int)ClientPackets.ObjectDestroyed, ServerRequestHandle.DestroyObject },
                { (int)ClientPackets.Ready2Shoot, ServerRequestHandle.Ready },
                { (int)ClientPackets.EarlyLoss, ServerRequestHandle.EarlyLoss },
                { (int)ClientPackets.MatchEnded, ServerRequestHandle.PlayerLeftEndScreen },
               // { (int)ClientPackets.RematchRequest, ServerHandle.RematchRequest },
               // { (int)ClientPackets.RematchQuestionResponse, ServerHandle.RematchResponse },
            };
            Console.WriteLine("Initialized packets.");
        }
    }
}
