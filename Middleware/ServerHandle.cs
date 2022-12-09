using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using WebSocketServer.GameServerLogic;
using WebSocketServer.Middleware;

namespace WebSocketServer.Middleware
{
    class ServerHandle
    {
        public static async Task<Packet> WelcomeReceived(Client fromClient, Packet _packet)
        {
            Console.Write("WelcomeReceived --> ");

            string _connIDCheck = _packet.ReadString();
            if (_connIDCheck.Equals(fromClient.connID))
            {
                string _username = _packet.ReadString();

                //check if name is registered if not, register name
                var clientINFO = await ClientsDataBase.CheckName(_username);

                if (clientINFO == null)
                {
                    //create a new entry for him
                    clientINFO = await ClientsDataBase.AddNewClient(_username);
                }
                fromClient.InstantiatePlayer(_username, clientINFO);

                Packet responsePacket = new Packet((int)ServerPackets.PlayerInfo);
                responsePacket.Write(clientINFO.Games.Count);
                var aux = clientINFO.Games.Values.ToArray();
                for (int i = 0; i < clientINFO.Games.Count; i++)
                {
                    responsePacket.Write(aux[i].ID);
                    responsePacket.Write(aux[i].Crowns);
                    responsePacket.Write(aux[i].max_Score);
                    responsePacket.Write(aux[i].max_Score_Time);
                    responsePacket.Write(aux[i].Owned);
                }
                return responsePacket;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("THE ACTUAL FUCK HAPPEND HERE, WE GOT A DIFFERENT CONNID");
                Console.ForegroundColor = ConsoleColor.White;
                return null;
            }
        }

        public static Task<Packet> UDPTestReceived(Client fromClient, Packet _packet)
        {
            string _msg = _packet.ReadString();

            Console.WriteLine($"UDPTestReceived. Contains message: {_msg}");

            return null;
        }

        public static async Task<Packet> TriRunQueueUp(Client fromClient, Packet _packet)
        {
            int GameNumber = _packet.ReadInt();
            Console.Write($"TriRunQueueUp({GameNumber}) --> ");
            fromClient.player.CurrentSelectedGame = GameNumber;

            //add client to TriRunQueue
            //return await MatchMaking.IncomingClientTri(fromClient, GameNumber);
            return null;
        }

        public static async Task<Packet> SceneWasLoaded(Client fromClient, Packet _packet)
        {
            Console.Write("SceneWasLoaded --> ");
            string _matchID = _packet.ReadString();
            string _sceneName = _packet.ReadString();
            fromClient.player.SceneWasLoaded(_sceneName);

            MatchMaking.OnGoingMatches.TryGetValue(_matchID, out Match match);
            /*
            if(match.player1.player.SceneLoaded && match.player2.player.SceneLoaded)
            {
                await match.SceneLoaded();
            }
            //match.CheckIfOther(fromClient, "SceneLoaded");*/
            //return await match.SceneLoaded(fromClient);
            return null;
        }

        public static async Task<Packet> PlayerWasSpawned(Client fromClient, Packet _packet)
        {
            string _matchID = _packet.ReadString();
            fromClient.player.PlayerSpawned = true;
            //fromClient.player.SyncPlayer = new SyncObject(-255);

            MatchMaking.OnGoingMatches.TryGetValue(_matchID, out Match match);
            /*
            if (match.player1.player.PlayerSpawned && match.player2.player.PlayerSpawned)
            {
                await match.PlayerSpawned();
            }
            //match.CheckIfOther(fromClient, "PlayerSpawned");*/
            //return await match.PlayerSpawned(fromClient);
            return null;
        }

        public static async Task<Packet> OneGameEnded(Client fromClient, Packet _packet)
        {
            string _matchID = _packet.ReadString();
            int _playerScore = _packet.ReadInt();

            Console.Write($"OneGameEnded {fromClient.connID} ; Score : {_playerScore} --> ");
            fromClient.player.SetScore(_playerScore);

            MatchMaking.OnGoingMatches.TryGetValue(_matchID, out Match match);
            /*
            if (match.player1.player.GameUpdated && match.player2.player.GameUpdated)
            {
                await match.OneGameEnded();
            }
            */
            //return await match.OneGameEnded(fromClient);
            return null;
        }
        /*
        public static Packet MatchEnded(Client fromClient, Packet _packet)
        {
            string _playerScore = _packet.ReadString();

            Console.WriteLine($"{_playerScore} #{fromClient.connID}'s player match");

            //Server.clients[fromClient.connID].player.LeftTheGame();
            return _packet;
        }
        */
        /*
        public static Packet RematchRequest(Client fromClient, Packet _packet)
        {
            string _playerScore = _packet.ReadString();

            Console.WriteLine($"#{fromClient.connID}'s {_playerScore}");

            //ServerSend.RematchQuestion(Server.clients[fromClient.connID].player.CurrentEnemyID);
            return _packet;
        }

        public static Packet RematchResponse(Client fromClient, Packet _packet)
        {
            string _answer = _packet.ReadString();
            //var enemyID = Server.clients[fromClient.connID].player.CurrentEnemyID;

            //Console.WriteLine($"#{fromClient.connID}'s Answer: {_answer}");

            switch (_answer)
            {
                case "yes":
                    Console.WriteLine("Both players agreed on a rematch");

                    //ServerSend.RematchResponse(fromClient.connID, "yes");
                    //ServerSend.RematchResponse(Server.clients[fromClient.connID].player.CurrentEnemyID, "yes");

                   // MatchMaking.CreateMatch(fromClient.connID, Server.clients[fromClient.connID].player.CurrentSelectedGame,
                                      //enemyID, Server.clients[enemyID].player.CurrentSelectedGame);

                    //Server.clients[fromClient.connID].player.ResetAllTriggers();
                    //Server.clients[enemyID].player.ResetAllTriggers();

                    //Server.clients[fromClient.connID].player.ResetGameStats();
                    //Server.clients[enemyID].player.ResetGameStats();

                    //Server.clients[fromClient.connID].player.LeftTheGame();
                   // Server.clients[enemyID].player.LeftTheGame();
                    break;
                case "no":
                    //ServerSend.RematchResponse(enemyID, _answer);
                   // ServerSend.RematchResponse(fromClient.connID, _answer);
                    break;
            }
            return _packet;
        }
        */
        public static Task<Packet> UpdateClientEmotion(Client fromClient, Packet _packet)
        {
            string _emotion = _packet.ReadString();

            fromClient.player.SetEmotion(_emotion);
            return null;
        }
        

        public static Task<Packet> SyncPlayer(Client fromClient, Packet _packet)
        {
            
            Vector3 _playerPosition = _packet.ReadVector3();
            Vector3 _playerVelocity = _packet.ReadVector3();
            Vector3 _playerScale = _packet.ReadVector3();

            if (fromClient.player.SyncPlayer != null) fromClient.player.SyncPlayer.Sync(_playerPosition, _playerVelocity, _playerScale);
            
            return null;
        }
        
        public static async Task<Packet> ObjectSpawned(Client fromClient, Packet _packet)
        {
            string _matchID = _packet.ReadString();
            int _Length = _packet.ReadInt();

            int[] _objID = new int[_Length];
            string[] _enemyPrefabName = new string[_Length];
            Vector3[] _position = new Vector3[_Length];

            for(int i = 0; i < _Length; i++)
            {
                _objID[i] = _packet.ReadInt();
                string _prefabName = _packet.ReadString();
                _enemyPrefabName[i] = _prefabName.Replace("Local", "");
                _position[i] = _packet.ReadVector3();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{fromClient.player.username} spawned : Local{_enemyPrefabName[i]}");
                Console.ForegroundColor = ConsoleColor.White;
            }

            MatchMaking.OnGoingMatches.TryGetValue(_matchID, out Match match);
            foreach(int id in _objID)
            {
                //fromClient.player.AddObject(id);
            }
            
            Packet enemyPacket = new Packet((int)ServerPackets.EnemyObjectSpawned);
            enemyPacket.Write(_Length);
            for (int i = 0; i < _Length; i++)
            {
                enemyPacket.Write(_objID[i]);
                enemyPacket.Write(_enemyPrefabName[i]);
                enemyPacket.Write(_position[i]);
            }

            if (match.player1.Equals(fromClient))
            {
                await match.player2.SendPacketAsync(enemyPacket);
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"{match.player2.player.username} will spawn : {_enemyPrefabName}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                await match.player1.SendPacketAsync(enemyPacket);
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"{match.player1.player.username} will spawn : {_enemyPrefabName}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            
            return null;
        }

        public static Task<Packet> SyncObject(Client fromClient, Packet _packet)
        {
            int _id = _packet.ReadInt();
            Vector3 _pos = _packet.ReadVector3();
            Vector3 _vel = _packet.ReadVector3();
            Vector3 _scale = _packet.ReadVector3();

            fromClient.player.SyncedObjects.TryGetValue(_id, out SyncObject obj);
            if(obj != null) obj.Sync(_pos, _vel, _scale);
            /*
            foreach (var obj in fromClient.player.SyncedObjects)
            {
                if (obj.ID == _id) obj.Sync(_pos, _vel, _scale);
            }
            */
            return null;
        }
        
        public static async Task<Packet> DestroyObject(Client fromClient, Packet _packet)
        {
            Console.Write($"DestroyObject {fromClient.connID} --> ");

            string _matchID = _packet.ReadString();
            MatchMaking.OnGoingMatches.TryGetValue(_matchID, out Match match);
            int _id = _packet.ReadInt();
            if (_id == -255) // if it is player that was destroyed
            {
                Packet enemyPacket = new Packet((int)ServerPackets.EnemyObjectDestroy);
                enemyPacket.Write(_id);

                //ServerSend.ObjectDestroyed(Server.clients[fromClient.connID].player.CurrentEnemyID, _id);
                if (match.player1.Equals(fromClient))
                {
                    await match.player2.SendPacketAsync(enemyPacket);
                }
                else
                {
                    await match.player1.SendPacketAsync(enemyPacket);
                }
                fromClient.player.SyncPlayer = null;
            }
            else
            {
                //SyncObject aux = new SyncObject(0);
                //foreach (SyncObject obj in fromClient.player.SyncedObjects)
                //{
                //f (obj.ID == _id)
                //{
                //aux = obj;
                //ServerSend.ObjectDestroyed(Server.clients[fromClient.connID].player.CurrentEnemyID, _id);
                fromClient.player.SyncedObjects.TryRemove(_id, out SyncObject obj);

                Packet enemyPacket = new Packet((int)ServerPackets.EnemyObjectDestroy);
                enemyPacket.Write(_id);
                if (match.player1.Equals(fromClient))
                {
                    await match.player2.SendPacketAsync(enemyPacket);
                }
                else
                {
                    await match.player1.SendPacketAsync(enemyPacket);
                }
                    //}
                //}
                //fromClient.player.SyncedObjects.TryRemove(new KeyValuePair<int, SyncObject>(_id,obj));
            }
            
            return null;
        }

        /*
        public static Packet SyncObjects(Client fromClient, Packet _packet)
        {
            //int _objectsCount = _packet.ReadInt();
            //for(int i = 0; i < _objectsCount; i++)
            //{
                var _index = _packet.ReadInt();
                var _pos = _packet.ReadVector3();
                var _vel = _packet.ReadVector3();
                var _size = _packet.ReadVector3();
                //Console.WriteLine($"{_pos}   {_vel}");
                Server.clients[fromClient.connID].player.SyncObjects[_index].Sync(_pos, _vel, _size);
            //}
        }
        */
        public static Task<Packet> EarlyLoss(Client fromClient, Packet _packet)
        {
            fromClient.player.EarlyLoss = true;
            return null;
        }
        /*
        public static Packet Ready2Shoot(Client fromClient, Packet _packet)
        {
            /*
            var _enemyID = Server.clients[fromClient.connID].player.CurrentEnemyID;
            Console.WriteLine($"{fromClient.connID} said he is ready");
            if (Server.clients[_enemyID].player.Ready == true)
            {
                ServerSend.ShootNow(fromClient.connID, _enemyID);
                Server.clients[fromClient.connID].player.Ready = false;
                Server.clients[_enemyID].player.Ready = false;
                Console.WriteLine("I SAID SHOOT EM BOTH");
            }
            else
            {
                Server.clients[fromClient.connID].player.Ready = true;
            }
            
            return _packet;
        }
        */
    }
}
