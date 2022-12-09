using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebSocketServer.GameServerLogic;

namespace WebSocketServer.Middleware
{
    public class ServerRequestHandle
    {
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

        private class WelcomeBack
        {
            public string connID;
            public string playerName;
        }
        private class PlayerData
        {
            public Game[] GamesData;

            public PlayerData(Dictionary<int, Game> playerData)
            {
                GamesData = playerData.Values.ToArray();
            }
        }
        public static async Task<string> WelcomeReceived(Client fromClient, string content)
        {
            //Console.WriteLine("WelcomeReceived --> ");
            WelcomeBack message = JsonConvert.DeserializeObject<WelcomeBack>(content);

            if (message.connID.Equals(fromClient.connID))
            {
                //check if name is registered if not, register name
                var clientINFO = await ClientsDataBase.CheckName(message.playerName);

                if (clientINFO == null)
                {
                    //create a new entry for him
                    clientINFO = await ClientsDataBase.AddNewClient(message.playerName);
                }
                fromClient.InstantiatePlayer(message.playerName, clientINFO);
                PlayerData playerData = new PlayerData(clientINFO.Games);
                return JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.PlayerInfo, playerData));
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("THE ACTUAL FUCK HAPPEND HERE, WE GOT A DIFFERENT CONNID");
                Console.ForegroundColor = ConsoleColor.White;
                return null;
            }
        }

        private class QueueRequest
        {
            public string GameMode;
            public int[] ChosenGames;
        }
        public static async Task<string> QueueUp(Client fromClient, string content)
        {
            QueueRequest request = JsonConvert.DeserializeObject<QueueRequest>(content);

            switch (request.GameMode)
            {
                case "TriRun":
                    int GameNumber = request.ChosenGames[0];
                    Console.Write($"TriRunQueueUp({GameNumber}) --> ");
                    fromClient.player.CurrentSelectedGame = GameNumber;
                    //add client to TriRunQueue
                    return await MatchMaking.IncomingTriRun(fromClient, GameNumber);

                case "QuadRun":
                    int GameNumber1 = request.ChosenGames[0];
                    int GameNumber2 = request.ChosenGames[1];
                    Console.Write($"QuadRunQueueUp({GameNumber1} , {GameNumber2}) --> ");
                    return await MatchMaking.IncomingQuadRun(fromClient, GameNumber1, GameNumber2);
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("THE ACTUAL FUCK HAPPEND HERE, WE GOT A DIFFERENT GAMEMODE REQUEST");
            Console.ForegroundColor = ConsoleColor.White;
            return null;
        }

        private class SceneLoaded
        {
            public string matchID;
            public string SceneName;
        }
        public static async Task<string> SceneWasLoaded(Client fromClient, string content)
        {
            Console.Write("SceneWasLoaded --> ");

            SceneLoaded sceneLoaded = JsonConvert.DeserializeObject<SceneLoaded>(content);

            fromClient.player.SceneWasLoaded(sceneLoaded.SceneName);

            MatchMaking.OnGoingMatches.TryGetValue(sceneLoaded.matchID, out Match match);

            return await match.SceneLoaded(fromClient);
        }

        private class PlayerSpawned
        {
            public string matchID;
            public string name;
            public Vector3 position;
            public Vector3 size;
        }
        public static async Task<string> PlayerWasSpawned(Client fromClient, string content)
        {
            PlayerSpawned playerSpawned = JsonConvert.DeserializeObject<PlayerSpawned>(content);

            fromClient.player.PlayerSpawned = true;
            fromClient.player.SyncPlayer = new SyncObject(-255, playerSpawned.name, playerSpawned.position, playerSpawned.size);

            MatchMaking.OnGoingMatches.TryGetValue(playerSpawned.matchID, out Match match);

            return await match.PlayerSpawned(fromClient);
        }

        private class ObjectSpawnedObject
        {
            public string matchID;
            public int objectID;
            public string name;
            public Vector3 position;
            public Vector3 size;
        }
        private class EnemySpawnedObject
        {
            public int objectID;
            public string name;
            public Vector3 position;
            public Vector3 size;
        }
        public static async Task<string> ObjectSpawned(Client fromClient, string content)
        {
            ObjectSpawnedObject spawnedObject = JsonConvert.DeserializeObject<ObjectSpawnedObject>(content);

            MatchMaking.OnGoingMatches.TryGetValue(spawnedObject.matchID, out Match match);
            fromClient.player.AddObject(spawnedObject.objectID, spawnedObject.name, spawnedObject.position, spawnedObject.size);

            EnemySpawnedObject enemy = new EnemySpawnedObject
            {
                objectID = spawnedObject.objectID,
                name = spawnedObject.name.Replace("Local", ""),
                position = spawnedObject.position,
                size = spawnedObject.size,
            };

            if (match.player1.Equals(fromClient))
            {
                await match.player2.SendMessageAsync(JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.EnemyObjectSpawned, enemy)));
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"{match.player2.player.username} will spawn : {enemy.name}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                await match.player1.SendMessageAsync(JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.EnemyObjectSpawned, enemy)));
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"{match.player1.player.username} will spawn : {enemy.name}");
                Console.ForegroundColor = ConsoleColor.White;
            }

            return null;
        }

        private class DestroyObj
        {
            public string matchID;
            public int objectID;
        }
        private class EnemyDestroyObject
        {
            public int objectID;
        }
        public static async Task<string> DestroyObject(Client fromClient, string content)
        {
            DestroyObj destroyObj = JsonConvert.DeserializeObject<DestroyObj>(content);
            Console.Write($"{fromClient.player.username} DestroyObject {destroyObj.objectID}-- > ");

            MatchMaking.OnGoingMatches.TryGetValue(destroyObj.matchID, out Match match);

            EnemyDestroyObject enemy = new EnemyDestroyObject
            {
                objectID = destroyObj.objectID,
            };
            if (destroyObj.objectID == -255) // if it is player that was destroyed
            {
                if (match.player1.Equals(fromClient))
                {
                    await match.player2.SendMessageAsync(JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.EnemyObjectDestroy, enemy)));
                }
                else
                {
                    await match.player1.SendMessageAsync(JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.EnemyObjectDestroy, enemy)));
                }
                fromClient.player.SyncPlayer = null;
            }
            else
            {
                try
                {
                    fromClient.player.SyncedObjects.TryRemove(destroyObj.objectID, out SyncObject obj);
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"THE ACTUAL FUCK HAPPEND HERE!!!! {e}");
                    Console.ForegroundColor = ConsoleColor.White;
                }

                if (match.player1.Equals(fromClient))
                {
                    await match.player2.SendMessageAsync(JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.EnemyObjectDestroy, enemy)));
                }
                else
                {
                    await match.player1.SendMessageAsync(JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.EnemyObjectDestroy, enemy)));
                }
            }

            return null;
        }

        private class EarlylossObj
        {
            public string matchID;
        }
        public static async Task<string> EarlyLoss(Client fromClient, string content)
        {
            //EarlylossObj earlyLoss = JsonConvert.DeserializeObject<EarlylossObj>(content);

            Console.WriteLine($"{fromClient.player.username} lost early");

            //MatchMaking.OnGoingMatches.TryGetValue(earlyLoss.matchID, out Match match);

            fromClient.player.EarlyLoss = true;

            return null;//await match.GameEndEarly(fromClient);
        }

        private class EndGameObj
        {
            public string matchID;
            public int score;
        }
        public static async Task<string> OneGameEnded(Client fromClient, string content)
        {
            EndGameObj endGame = JsonConvert.DeserializeObject<EndGameObj>(content);

            Console.Write($"OneGameEnded {fromClient.connID} ; Score : {endGame.score} --> ");

            fromClient.player.SetScore(endGame.score);

            MatchMaking.OnGoingMatches.TryGetValue(endGame.matchID, out Match match);

            return await match.OneGameEnded(fromClient);
        }

        private class ReadyObj
        {
            public string matchID;
        }
        public static async Task<string> Ready(Client fromClient, string content)
        {
            ReadyObj readyObj = JsonConvert.DeserializeObject<ReadyObj>(content);

            Console.WriteLine($"{fromClient.player.username} said he is ready");

            MatchMaking.OnGoingMatches.TryGetValue(readyObj.matchID, out Match match);

            fromClient.player.Ready = true;

            return await match.PlayerReady(fromClient);
        }

        private class PlayerLeftEndScreenObj
        {
            public string matchID;
        }
        public static async Task<string> PlayerLeftEndScreen(Client fromClient, string content)
        {
            //PlayerLeftEndScreenObj playerLeft = JsonConvert.DeserializeObject<PlayerLeftEndScreenObj>(content);

            Console.WriteLine($"{fromClient.player.username} left the endscreen");

            // MatchMaking.OnGoingMatches.TryGetValue(playerLeft.matchID, out Match match);

            await ClientsDataBase.UpdateINFO(fromClient);

            fromClient.player.LeftTheEndScreen = true;

            return null;
        }
    }
}
