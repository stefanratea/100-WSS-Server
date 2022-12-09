using System;
namespace WebSocketServer.Middleware
{
    class ServerSend
    {
        #region TCP/UDP
        private static void SendTCPData(string clientID, Packet _packet)
        {
            _packet.WriteLength();
            //Server.clients[clientID].tcp.SendData(_packet);
        }
        /*
        private static void SendTCPData(string clientID1, string clientID2, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[clientID1].tcp.SendData(_packet);
            Server.clients[clientID2].tcp.SendData(_packet);
        }
        */
        private static void SendUDPData(string clientID, Packet _packet)
        {
            _packet.WriteLength();
            //Server.clients[clientID].udp.SendData(_packet);
            
        }

        /*
        private static void SendTCPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }
        private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].tcp.SendData(_packet);
                }
            }
        }

        private static void SendUDPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }

        private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].udp.SendData(_packet);
                }
            }
        }*/
        #endregion

        #region Packets
        public static void Welcome(string clientID, string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.welcome))
            {
                _packet.Write(_msg);
                _packet.Write(clientID);

                SendTCPData(clientID, _packet);
            }
        }

        public static void UDPTest(string clientID)
        {
            using (Packet _packet = new Packet((int)ServerPackets.udpTest))
            {
                _packet.Write("A test packet for UDP.");

                //ServerMiddleware.SendUDPData(clientID, _packet);
            }
        }
        /*
        public static void PlayerInfo(string clientID, ClientInfo _info)
        {
            using (Packet _packet = new Packet((int)ServerPackets.PlayerInfo))
            {
                _packet.Write(_info.Games.Count);
                var aux = _info.Games.Values.ToArray();
                for (int i = 0; i < _info.Games.Count; i++)
                {
                    _packet.Write(aux[i].ID);
                    _packet.Write(aux[i].Crowns);
                    _packet.Write(aux[i].max_Score);
                    _packet.Write(aux[i].max_Score_Time);
                    _packet.Write(aux[i].Owned);
                }

                SendTCPData(clientID, _packet);
            }
        }

        public static void MatchFound(Match _match)
        {
            using (Packet _packet = new Packet((int)ServerPackets.MatchFound))
            {
                _packet.Write("Match found!");
                _packet.Write(_match.ID);
                _packet.Write(_match.Games.Length);
                for (int i = 0; i < _match.Games.Length; i++)
                {
                    _packet.Write(_match.Games[i]);
                }

                SendTCPData(_match.P1.id, _match.P2.id, _packet);
            }
        }

        public static void RematchQuestion(string clientID)
        {
            using (Packet _packet = new Packet((int)ServerPackets.RematchQuestion))
            {
                _packet.Write(Server.clients[clientID].player.username);

                SendTCPData(clientID, _packet);
            }
        }

        public static void RematchResponse(string clientID, string _answer)
        {
            using (Packet _packet = new Packet((int)ServerPackets.RematchRequestResponse))
            {
                _packet.Write(_answer);

                SendTCPData(clientID, _packet);
            }
        }

        public static void EndGameStatsCheck(string clientID, int _matchID, int _playerCrowns, int _enemyCrowns)
        {
            using (Packet _packet = new Packet((int)ServerPackets.EndMatchStats))
            {
                _packet.Write(_matchID);
                _packet.Write(_playerCrowns);
                _packet.Write(_enemyCrowns);

                SendTCPData(clientID, _packet);
            }
        }

        public static void LoadScene(string clientID, string _sceneName, int[] clientIDScore, int[] _enemyScore)
        {
            using (Packet _packet = new Packet((int)ServerPackets.LoadScene))
            {
                _packet.Write(_sceneName);
                _packet.Write(clientIDScore.Length);

                for (int i = 0; i < _enemyScore.Length; i++)
                {
                    _packet.Write(clientIDScore[i]);
                    _packet.Write(_enemyScore[i]);
                }

                SendTCPData(clientID, _packet);
            }
        }

        public static void StartGame(Match _match)
        {
            using (Packet _packet = new Packet((int)ServerPackets.StartGame))
            {
                SendTCPData(_match.P1.id, _match.P2.id, _packet);
            }
        }

        public static void SpawnPlayer(string clientID, Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.SpawnPlayer))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.username);

                SendTCPData(clientID, _packet);
            }
        }

        public static void GameEndsEarly(Match _match)
        {
            using (Packet _packet = new Packet((int)ServerPackets.GameEndsEarly))
            {
                SendTCPData(_match.P1.id, _match.P2.id, _packet);
            }
        }

        public static void PlayerEmotion(string clientID, Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.Emotion))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.Emotion);

                SendUDPData(clientID, _packet);
            }
        }

        public static void PlayerPosition(string clientID, Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.EnemyPosition))
            {
                if (_player.SyncPlayer != null)
                {
                    _packet.Write(_player.id);
                    _packet.Write(_player.SyncPlayer.position);
                    _packet.Write(_player.SyncPlayer.velocity);
                    _packet.Write(_player.SyncPlayer.localScale);
                    //_packet.Write(_player.ballPosition);
                    //_packet.Write(_player.ballVelocity);
                    //_packet.Write(_player.auxPosition);
                    //_packet.Write(_player.auxVelocity);

                    SendUDPData(clientID, _packet);
                }
            }
        }

        public static void ObjectSpawned(string clientID, int _objID, string _prefabName, Vector3 _position)
        {
            using (Packet _packet = new Packet((int)ServerPackets.EnemyObjectSpawned))
            {
                _packet.Write(_objID);
                _packet.Write(_prefabName);
                _packet.Write(_position);

                SendTCPData(clientID, _packet);
            }
        }
        public static void ObjectsPositions(string clientID, Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.EnemyObjectPosition))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.SyncedObjects.Count);

                for (int i = 0; i < _player.SyncedObjects.Count; i++)
                {
                    _packet.Write(_player.SyncedObjects[i].ID);
                    _packet.Write(_player.SyncedObjects[i].position);
                    _packet.Write(_player.SyncedObjects[i].velocity);
                    _packet.Write(_player.SyncedObjects[i].localScale);
                }

                SendUDPData(clientID, _packet);
            }
        }
        public static void ObjectDestroyed(string clientID, int _objID)
        {
            using (Packet _packet = new Packet((int)ServerPackets.EnemyObjectDestroy))
            {
                _packet.Write(_objID);

                SendTCPData(clientID, _packet);
            }
        }
        /*
        public static void ObjectsPositions(string clientID, Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.EnemyObjectsPositions))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.SyncObjects.Length);
                for(int i = 0; i < _player.SyncObjects.Length; i++)
                {
                    _packet.Write(_player.SyncObjects[i].position);
                    _packet.Write(_player.SyncObjects[i].velocity);
                    _packet.Write(_player.SyncObjects[i].localScale);
                }

                SendUDPData(clientID, _packet);
            }
        }
        */
        /*
        public static void SpawnBallsGame14(Match _match, Vector3 _pos)
        {
            using (Packet _packet = new Packet((int)ServerPackets.Game14))
            {
                //_packet.Write(_match.Games[_match.CurrentGameIndex]);
                _packet.Write(_pos);

                SendTCPData(_match.P1.id, _match.P2.id, _packet);
            }
        }

        public static void ShootBallsGame21(Match _match, int _pos)
        {
            using (Packet _packet = new Packet((int)ServerPackets.Game21))
            {

                _packet.Write(_pos);

                SendTCPData(_match.P1.id, _match.P2.id, _packet);
            }
        }

        public static void ShootBallsGame22(string clientID, Vector3 _posToClient, Vector3 _posEnemy)
        {
            using (Packet _packet = new Packet((int)ServerPackets.Game22))
            {
                _packet.Write(_posToClient);
                _packet.Write(_posEnemy);

                SendTCPData(clientID, _packet);
            }
        }

        public static void Game2X(string clientID, int _GameID, float[] _positions)
        {
            using (Packet _packet = new Packet((int)ServerPackets.Game2X))
            {
                _packet.Write(_GameID);
                _packet.Write(_positions.Length);

                for (int i = 0; i < _positions.Length; i++)
                {
                    _packet.Write(_positions[i]);
                }
                SendTCPData(clientID, _packet);
            }
        }

        public static void Game3X(string clientID, int _GameID, float[] _positions)
        {
            using (Packet _packet = new Packet((int)ServerPackets.Game3X))
            {
                _packet.Write(_GameID);
                _packet.Write(_positions.Length);

                for (int i = 0; i < _positions.Length; i++)
                {
                    _packet.Write(_positions[i]);
                }
                SendTCPData(clientID, _packet);
            }
        }

        public static void ShootNow(string clientID1, string clientID2)
        {
            using (Packet _packet = new Packet((int)ServerPackets.ShootNow))
            {
                SendTCPData(clientID1, clientID2, _packet);
            }
        }

        public static void PlayerDisconnected(int _playerId)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerDisconnected))
            {
                _packet.Write(_playerId);

                SendTCPDataToAll(_packet);
            }
        }
        */
        #endregion
    }
}
