using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketServer.GameServerLogic;

namespace WebSocketServer.Middleware
{
    public class Client
    {
        public WebSocket webSocket;
        public string connID;

        /// <summary>
        /// UDP
        /// </summary>
        public static int dataBufferSize = 4096;
        public UDP udpSocket;

        public bool IsQueued;
        /// <summary>
        /// In game properties
        /// </summary>
        public Player player;
        public ClientInfo info;

        public Client(WebSocket socket, string id)
        {
            webSocket = socket;
            udpSocket = new UDP();
            connID = id;
            IsQueued = false;
        }

        public class UDP

        {
            public IPEndPoint endPoint = null;

            public void Connect(IPEndPoint _endPoint)
            {
                endPoint = _endPoint;
                using (Packet _packet = new Packet((int)ServerPackets.udpTest))
                {
                    _packet.Write("A test packet for UDP.");

                    ServerMiddleware.SendUDPData(endPoint, _packet);
                }
            }

            public void SendData(Packet _packet)
            {
                ServerMiddleware.SendUDPData(endPoint, _packet);
            }

            public void HandleData(Client client, Packet _packetData)
            {
                int _packetLength = _packetData.ReadInt();
                byte[] _packetBytes = _packetData.ReadBytes(_packetLength);

                ThreadManager.ExecuteOnMainThread(() =>  //BECAUSE ITS UPDATING THE GAME
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        ServerMiddleware.packetHandlers[_packetId](client, _packet);
                    }
                });
            }

            public void Disconnect()
            {
                endPoint = null;
            }
        }

        public async Task SendMessageAsync(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task SendPacketAsync(Packet packet)
        {
            packet.WriteLength();
            var buffer = packet.ToArray();
            await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public void InstantiatePlayer(string _playerName, ClientInfo _info)
        {
            player = new Player(connID, _playerName);
            info = _info;
            //ServerSend.PlayerInfo(connID, info);
        }
    }
}
