using System;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebSocketServer.Middleware;

namespace WebSocketServer.GameServerLogic
{
    public class Match
    {
        public string ID;

        public Client player1;
        public Client player2;

        public int[] Games;
        public int CurrentGameIndex;

        public GameManager CurrentGame;

        public bool GameON;
        public bool PlayerON;
        public bool EndSceneON;

        public bool MatchEnded;

        //DateTime GameON;

        /// Temp
        public bool player1Disconnected = false;
        public bool player2Disconnected = false;
        ///

        public Match(int _numGames, Client p1, int game0)
        {
            player1 = p1;
            player2 = null;

            Games = new int[_numGames];
            Games[0] = game0;
            CurrentGameIndex = 0;
            GameON = false;
            PlayerON = false;
            EndSceneON = false;

            MatchEnded = false;
        }
        
        public void FoundMatch(Client p2, int _game1)
        {
            player2 = p2;
            player2.player.CurrentEnemyID = player1.connID;
            player1.player.CurrentEnemyID = player2.connID;

            Games[1] = _game1;
            Games[2] = ReturnRandomGame(Games[0], Games[1]);

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Start a match ID: {ID}\nWith {player1.player.id} and {player2.player.id}\nGames : {Games[0]}, {Games[1]}, {Games[2]}");
            Console.ForegroundColor = ConsoleColor.White;

            player1.player.ResetAllTriggers();
            player2.player.ResetAllTriggers();
            player1.player.ResetGameStats();
            player2.player.ResetGameStats();
            player1.player.SetEmotion(null);
            player2.player.SetEmotion(null);

            //Task.Delay(2000).ContinueWith(async o => { await LoadNextScene($"Game{Games[CurrentGameIndex]}"); });
        }

        public void Update()
        {
            if (PlayerON)
            {
                if (player1.player.SyncPlayer != null)
                {
                    Packet packetPlayer2 = new Packet((int)ServerPackets.EnemyPosition);
                    packetPlayer2.Write(player1.player.id);
                    packetPlayer2.Write(player1.player.SyncPlayer.position);
                    packetPlayer2.Write(player1.player.SyncPlayer.velocity);
                    packetPlayer2.Write(player1.player.SyncPlayer.localScale);

                    player2.udpSocket.SendData(packetPlayer2);
                }
                if (player2.player.SyncPlayer != null)
                {
                    Packet packetPlayer1 = new Packet((int)ServerPackets.EnemyPosition);
                    packetPlayer1.Write(player2.player.id);
                    packetPlayer1.Write(player2.player.SyncPlayer.position);
                    packetPlayer1.Write(player2.player.SyncPlayer.velocity);
                    packetPlayer1.Write(player2.player.SyncPlayer.localScale);
                    player1.udpSocket.SendData(packetPlayer1);
                }
            }
            if (GameON && !EndSceneON)
            {
                CurrentGame.RunGame(player1, player2);

                if (player1.player.SyncedObjects.Count > 0)
                {
                    Packet packetObjects2 = new Packet((int)ServerPackets.EnemyObjectPosition);
                    var SyncedObjects = player1.player.SyncedObjects.Values.ToArray();

                    packetObjects2.Write(player1.player.id);
                    packetObjects2.Write(SyncedObjects.Length);

                    for (int i = 0; i < player1.player.SyncedObjects.Count; i++)
                    {
                        packetObjects2.Write(SyncedObjects[i].ID);
                        packetObjects2.Write(SyncedObjects[i].position);
                        packetObjects2.Write(SyncedObjects[i].velocity);
                        packetObjects2.Write(SyncedObjects[i].localScale);
                    }
                    player2.udpSocket.SendData(packetObjects2);
                }
                if (player2.player.SyncedObjects.Count > 0)
                {
                    Packet packetObjects1 = new Packet((int)ServerPackets.EnemyObjectPosition);
                    var SyncedObjects = player2.player.SyncedObjects.Values.ToArray();

                    packetObjects1.Write(player2.player.id);
                    packetObjects1.Write(SyncedObjects.Length);

                    for (int i = 0; i < player2.player.SyncedObjects.Count; i++)
                    {
                        packetObjects1.Write(SyncedObjects[i].ID);
                        packetObjects1.Write(SyncedObjects[i].position);
                        packetObjects1.Write(SyncedObjects[i].velocity);
                        packetObjects1.Write(SyncedObjects[i].localScale);
                    }
                    player1.udpSocket.SendData(packetObjects1);
                }
            }
            if(GameON && EndSceneON)
            {
                if (player1.player.Emotion != null && player2.player.Emotion != null)
                {
                    //Console.WriteLine("EndScene");

                    Packet packet2 = new Packet((int)ServerPackets.Emotion);
                    packet2.Write(player2.connID);
                    packet2.Write(player1.player.Emotion);
                    player2.udpSocket.SendData(packet2);

                    Packet packet1 = new Packet((int)ServerPackets.Emotion);
                    packet1.Write(player1.connID);
                    packet1.Write(player2.player.Emotion);
                    player1.udpSocket.SendData(packet1);
                }
            }
            if (player1.player.EarlyLoss && player2.player.EarlyLoss)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(" \n-- GAME ENDED EARLY --\n");
                Console.ForegroundColor = ConsoleColor.White;

                player1.player.EarlyLoss = false;
                player1.player.EarlyLoss = false;
                ServerMiddleware.AddMessageToQueue(player1.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameEndsEarly, null)));
                ServerMiddleware.AddMessageToQueue(player2.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameEndsEarly, null)));
            }
            if (player1Disconnected && player2Disconnected)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Removed match {ID} from List");
                Console.ForegroundColor = ConsoleColor.White;
                MatchMaking.OnGoingMatches.TryRemove(ID, out Match match);
            }
            if (player1.player.LeftTheEndScreen || player2.player.LeftTheEndScreen)
            {
                player1.player.ResetAllTriggers();
                player2.player.ResetAllTriggers();
                player1.player.ResetGameStats();
                player2.player.ResetGameStats();
                MatchMaking.OnGoingMatches.TryRemove(ID, out Match match);
            }
        }
        private class EndMatchObj
        {
            public string matchID;
            public int playerCrowns;
            public int enemyCrowns;
        }
        public void EndMatch(Client player)
        {
            int player1GamesWon = 0;
            int player2GamesWon = 0;

            for (int i = 0; i < Games.Length; i++)
            {
                if (player1.player.Score[i] > player2.player.Score[i]) player1GamesWon++;
                if (player1.player.Score[i] < player2.player.Score[i]) player2GamesWon++;
            }

            if (player1.player.TotalScore() > player2.player.TotalScore())
            {
                player1GamesWon += 2;
                player1.info.UpdateGame(Games[0], 2, player1.player.Score[0]); // pentru ca player 1 da jocul 0
            }
            if (player1.player.TotalScore() < player2.player.TotalScore())
            {
                player2GamesWon += 2;
                player2.info.UpdateGame(Games[0], 2, player1.player.Score[0]);
            }

            if (player.Equals(player1))
            {
                Console.WriteLine($"--> {player.player.username} Score : {player1GamesWon} ");
                EndMatchObj endMatch = new EndMatchObj
                {
                    matchID = ID,
                    playerCrowns = player1GamesWon,
                    enemyCrowns = player2GamesWon,

                };
                ServerMiddleware.AddMessageToQueue(player.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.EndMatchStats, endMatch)));
            }
            else
            {
                Console.WriteLine($"--> {player.player.username} Score : {player2GamesWon} ");
                EndMatchObj endMatch = new EndMatchObj
                {
                    matchID = ID,
                    playerCrowns = player2GamesWon,
                    enemyCrowns = player1GamesWon,

                };
                ServerMiddleware.AddMessageToQueue(player.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.EndMatchStats, endMatch)));
            }
            //ClientsDataBase.UpdateINFO(player);
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

        private class LoadScene
        {
            public string SceneName;
            public int[] playerScore;
            public int[] enemyScore;
        }
        public string LoadNextScene(Client player)
        {
            Console.WriteLine($"{player.player.username} will load scene Game{Games[CurrentGameIndex]}");

            if (player.Equals(player1))
            {
                var type = Type.GetType($"WebSocketServer.GameServerLogic.GameManager{Games[CurrentGameIndex]}");
                CurrentGame = (GameManager)Activator.CreateInstance(type);

                LoadScene loadScene = new LoadScene
                {
                    SceneName = $"Game{Games[CurrentGameIndex]}",
                    playerScore = player1.player.Score,
                    enemyScore = player2.player.Score,

                };
                player1.player.CurrentSceneName = $"Game{Games[CurrentGameIndex]}";
                return JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.LoadScene, loadScene));
            }

            if (player.Equals(player2))
            {
                LoadScene loadScene = new LoadScene
                {
                    SceneName = $"Game{Games[CurrentGameIndex]}",
                    playerScore = player2.player.Score,
                    enemyScore = player1.player.Score,

                };
                player2.player.CurrentSceneName = $"Game{Games[CurrentGameIndex]}";
                return JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.LoadScene, loadScene));
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("HOW DAFQ THIS HAPPEND, THE ACTUAL FUCKING PLAYER WAS NOT A FUCKING PART OF THIS MATCH");
            Console.ForegroundColor = ConsoleColor.White;
            return null;
        }
        public string LoadNextScene(Client player, string sceneName)
        {
            Console.WriteLine($"{player.player.username} will load scene {sceneName}");

            if (player.Equals(player1))
            {
                EndSceneON = true;
                LoadScene loadScene = new LoadScene
                {
                    SceneName = sceneName,
                    playerScore = player1.player.Score,
                    enemyScore = player2.player.Score,

                };
                player1.player.CurrentSceneName = sceneName;
                return JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.LoadScene, loadScene));
            }

            if (player.Equals(player2))
            {
                EndSceneON = true;
                LoadScene loadScene = new LoadScene
                {
                    SceneName = sceneName,
                    playerScore = player2.player.Score,
                    enemyScore = player1.player.Score,

                };
                player2.player.CurrentSceneName = sceneName;
                return JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.LoadScene, loadScene));
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("HOW DAFQ THIS HAPPEND, THE ACTUAL FUCKING PLAYER WAS NOT A FUCKING PART OF THIS MATCH (LOADNEXTSCENE)");
            Console.ForegroundColor = ConsoleColor.White;
            return null;
        }

        private class SpawnPlayerObject
        {
            public string playerID;
            public string enemyID;
            public string enemyName;
        }
        public async Task<string> SceneLoaded(Client player)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"SceneLoaded");

            if (player.Equals(player1))
            {
                if (player2.player.SceneLoaded) // if second player loaded scene aswell
                {
                    SpawnPlayerObject spawnPlayer = new SpawnPlayerObject
                    {
                        playerID = player1.connID,
                        enemyID = player2.connID,
                        enemyName = player2.player.username,
                    };
                    return JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.SpawnPlayer, spawnPlayer));
                }
                else
                {
                    while (!player2.player.SceneLoaded)
                    {
                        Console.WriteLine($"waiting for {player2.player.username} to load scene");
                        await Task.Delay(50);
                    }
                    player1.player.SceneLoaded = false;
                    player2.player.SceneLoaded = false;
                    SpawnPlayerObject spawnPlayer = new SpawnPlayerObject
                    {
                        playerID = player1.connID,
                        enemyID = player2.connID,
                        enemyName = player2.player.username,
                    };
                    return JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.SpawnPlayer, spawnPlayer));
                }
            }

            if (player.Equals(player2))
            {
                if (player1.player.SceneLoaded) // if second player loaded scene aswell
                {
                    SpawnPlayerObject spawnPlayer = new SpawnPlayerObject
                    {
                        playerID = player2.connID,
                        enemyID = player1.connID,
                        enemyName = player1.player.username,
                    };
                    return JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.SpawnPlayer, spawnPlayer));
                }
                else
                {
                    while (!player1.player.SceneLoaded)
                    {
                        Console.WriteLine($"waiting for {player1.player.username} to load scene");
                        await Task.Delay(50);
                    }
                    player1.player.SceneLoaded = false;
                    player2.player.SceneLoaded = false;
                    SpawnPlayerObject spawnPlayer = new SpawnPlayerObject
                    {
                        playerID = player2.connID,
                        enemyID = player1.connID,
                        enemyName = player1.player.username,
                    };
                    return JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.SpawnPlayer, spawnPlayer));
                }
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("HOW DAFQ THIS HAPPEND, THE ACTUAL FUCKING PLAYER WAS NOT A FUCKING PART OF THIS MATCH");
            Console.ForegroundColor = ConsoleColor.White;
            return null;
        }

        private class StartGameObject
        {

        }
        public async Task<string> PlayerSpawned(Client player)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"PlayerSpawned");

            if (player.Equals(player1))
            {
                if (player2.player.PlayerSpawned) // if second player loaded scene aswell
                {
                    return await Task.Delay(2000).ContinueWith(o => {
                        return JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.StartGame, null)); });
                }
                else
                {
                    while (!player2.player.PlayerSpawned)
                    {
                        Console.WriteLine($"waiting for {player2.player.username} to be spawned");
                        await Task.Delay(50);
                    }
                    player1.player.PlayerSpawned = false;
                    player2.player.PlayerSpawned = false;
                    PlayerON = true;
                    return await Task.Delay(2000).ContinueWith(o => {
                        GameON = true;
                        return JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.StartGame, null));
                    });
                }
            }

            if (player.Equals(player2))
            {
                if (player1.player.PlayerSpawned) // if second player loaded scene aswell
                {
                    return await Task.Delay(2000).ContinueWith(o => {
                        return JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.StartGame, null));
                    });
                }
                else
                {
                    while (!player1.player.PlayerSpawned)
                    {
                        Console.WriteLine($"waiting for {player1.player.username} to be spawned");
                        await Task.Delay(50);
                    }
                    player1.player.PlayerSpawned = false;
                    player2.player.PlayerSpawned = false;
                    PlayerON = true;
                    return await Task.Delay(2000).ContinueWith(o => {
                        GameON = true;
                        return JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.StartGame, null));
                    });
                }
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("HOW DAFQ THIS HAPPEND, THE ACTUAL FUCKING PLAYER WAS NOT A FUCKING PART OF THIS MATCH");
            Console.ForegroundColor = ConsoleColor.White;
            return null;
        }
        public async Task<string> OneGameEnded(Client player)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine($"OneGameEnded from {player.player.username} ");

            if (player.Equals(player1))
            {
                if (CurrentGameIndex < Games.Length - 1)
                {
                    if (player2.player.GameUpdated) // if second player updated game aswell
                    {
                        CurrentGameIndex++;
                        return await Task.Delay(2000).ContinueWith(o =>
                        {
                            return LoadNextScene(player);
                        });
                    }
                    else
                    {
                        while (!player2.player.GameUpdated)
                        {
                            Console.WriteLine($"waiting for {player2.player.username} to end game");
                            await Task.Delay(50);
                        }
                        player1.player.SyncedObjects.Clear();
                        player2.player.SyncedObjects.Clear();
                        player1.player.EarlyLoss = false;
                        player2.player.EarlyLoss = false;
                        player1.player.GameUpdated = false;
                        player2.player.GameUpdated = false;
                        return await Task.Delay(2000).ContinueWith(o =>
                        {
                            GameON = false;
                            PlayerON = false;
                            return LoadNextScene(player);
                        });
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"Game's all done for {player.player.username}");
                    player1.player.SyncedObjects.Clear();

                    return await Task.Delay(2000).ContinueWith(o =>
                    {
                        //return EndMatch(player);
                        GameON = false;
                        PlayerON = false;
                        EndMatch(player);
                        return LoadNextScene(player, "EndGame");
                    });
                }
            }

            if (player.Equals(player2))
            {
                if (CurrentGameIndex < Games.Length - 1)
                {
                    if (player1.player.GameUpdated) // if second player loaded scene aswell
                    {
                        CurrentGameIndex++;
                        return await Task.Delay(2000).ContinueWith(o =>
                        {
                            return LoadNextScene(player);
                        });
                    }
                    else
                    {
                        while (!player1.player.GameUpdated)
                        {
                            Console.WriteLine($"waiting for {player1.player.username} to end game");
                            await Task.Delay(50);
                        }
                        player1.player.SyncedObjects.Clear();
                        player2.player.SyncedObjects.Clear();
                        player1.player.EarlyLoss = false;
                        player2.player.EarlyLoss = false;
                        player1.player.GameUpdated = false;
                        player2.player.GameUpdated = false;
                        return await Task.Delay(2000).ContinueWith(o =>
                        {
                            GameON = false;
                            PlayerON = false;
                            return LoadNextScene(player);
                        });
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"Game's all done for {player.player.username}");
                    player2.player.SyncedObjects.Clear();// Go to game endscreen

                    return await Task.Delay(2000).ContinueWith(o =>
                    {
                        //return EndMatch(player);
                        GameON = false;
                        PlayerON = false;
                        EndMatch(player);
                        return LoadNextScene(player, "EndGame");
                    });
                }
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("HOW DAFQ THIS HAPPEND, THE ACTUAL FUCKING PLAYER WAS NOT A FUCKING PART OF THIS MATCH (ON GAME ENDED)");
            Console.ForegroundColor = ConsoleColor.White;
            return null;
        }
        public async Task<string> PlayerReady(Client player)
        {
            if (player.Equals(player1))
            {
                if (player2.player.Ready) // if second player is ready aswell
                {
                    return JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.ShootNow, null));
                }
                else
                {
                    while (!player2.player.Ready)
                    {
                        Console.WriteLine($"waiting for {player2.player.username} to be ready aswell");
                        await Task.Delay(50);
                    }
                    return JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.ShootNow, null));
                }
            }

            if (player.Equals(player2))
            {
                if (player1.player.Ready) // if second player is ready aswell
                {
                    return JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.ShootNow, null));
                }
                else
                {
                    while (!player1.player.Ready)
                    {
                        Console.WriteLine($"waiting for {player1.player.username} to be ready aswell");
                        await Task.Delay(50);
                    }
                    return JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.ShootNow, null));
                }
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("HOW DAFQ THIS HAPPEND, THE ACTUAL FUCKING PLAYER WAS NOT A FUCKING PART OF THIS MATCH (PlayerReady)");
            Console.ForegroundColor = ConsoleColor.White;
            return null;
        }

        private int ReturnRandomGame(int A, int B)
        {
            Random r = new Random();
        retry:

            var gameType = 2;
            var gameIndex = r.Next(1, 5);

            var Game = gameType * 10 + gameIndex;

            if (Game != A && Game != B)
            {
                return gameType * 10 + gameIndex;
            }
            else
            {
                goto retry;
            }
        }

        public Vector3 RandVect3(float minX, float maxX, float minY, float maxY)
        {
            Random r = new Random();
            var x = r.NextDouble() * (maxX - minX) + minX;
            var y = r.NextDouble() * (maxY - minY) + minY;
            return new Vector3((float)x, (float)y, 0);
        }
        public float FloatRandRange(float min, float max)
        {
            Random r = new Random();
            var x = r.NextDouble() * (max - min) + min;
            return (float)x;
        }
        public int IntRandRange(int min, int max)
        {
            Random r = new Random();
            return r.Next(min, max);
        }
    }
}
