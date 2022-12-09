using System;
using System.Numerics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebSocketServer.Middleware;

namespace WebSocketServer.GameServerLogic
{
    public interface IGameManager
    {
        public void RunGame(Client player1, Client player2);
    }
    public abstract class GameManager :IGameManager
    {
        public DateTime dateTime;
        public abstract void RunGame(Client player1, Client player2);
    }
    public class Ext
    {
        public static float[] ShuffleArray(float[] array)
        {
            Random r = new Random();
            float[] arr = new float[array.Length];
            array.CopyTo(arr, 0);
            // Start from the last element and 
            // swap one by one. We don't need to 
            // run for the first element  
            // that's why i > 0
            int n = arr.Length;
            for (int i = n - 1; i > 0; i--)
            {

                // Pick a random index 
                // from 0 to i 
                int j = r.Next(0, i + 1);

                // Swap arr[i] with the 
                // element at random index 
                float temp = arr[i];
                arr[i] = arr[j];
                arr[j] = temp;
            }
            // Prints the random array 
            return arr;
        }
        public static Vector3 RandVect3(float minX, float maxX, float minY, float maxY)
        {
            Random r = new Random();
            var x = r.NextDouble() * (maxX - minX) + minX;
            var y = r.NextDouble() * (maxY - minY) + minY;
            return new Vector3((float)x, (float)y, 0);
        }
        public static float FloatRandRange(float min, float max)
        {
            Random r = new Random();
            var x = r.NextDouble() * (max - min) + min;
            return (float)x;
        }
        public static int IntRandRange(int min, int max)
        {
            Random r = new Random();
            return r.Next(min, max);
        }
    }
    public class MessageWrap
    {
        public int requestID;
        public string jsonContent;

        public MessageWrap(int ID, object content)
        {
            requestID = ID;
            jsonContent = JsonConvert.SerializeObject(content);
        }
    }
    public class GameXX
    {
        public float[] Data;
    }

    #region Game1X

    public class GameManager11 : GameManager
    {
        public override void RunGame(Client player1, Client player2)
        {
            //Console.WriteLine("RunGame from 11");
        }
    }
    public class GameManager12 : GameManager
    {
        public override void RunGame(Client player1, Client player2)
        {
            //Console.WriteLine("RunGame from 12");
        }
    }
    public class GameManager13 : GameManager
    {
        public override void RunGame(Client player1, Client player2)
        {
            //Console.WriteLine("RunGame from 13");
        }
    }
    public class GameManager14 : GameManager
    {
        public override void RunGame(Client player1, Client player2)
        {
            //Console.WriteLine("RunGame from 14");
            if (dateTime < DateTime.Now)
            {
                dateTime = DateTime.Now;
                dateTime = dateTime.AddSeconds(1);

                var spawnPoint = Ext.RandVect3(-2f, 2f, -3f, 4f);
                GameXX game = new GameXX
                {
                    Data = new float[] { spawnPoint.X, spawnPoint.Y },
                };
                ServerMiddleware.AddMessageToQueue(player1.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameXX, game)));
                ServerMiddleware.AddMessageToQueue(player2.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameXX, game)));
            }
        }
    }
    #endregion

    #region Game2X

    public class GameManager21 : GameManager
    {
        public override void RunGame(Client player1, Client player2)
        {
            //Console.WriteLine("RunGame from 21");

            if (dateTime < DateTime.Now)
            {
                dateTime = DateTime.Now;
                dateTime = dateTime.AddSeconds(0.5f);
                
                var drip = Ext.IntRandRange(0, 5);

                GameXX game = new GameXX
                {
                    Data = new float[] { drip },
                };
                ServerMiddleware.AddMessageToQueue(player1.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameXX, game)));
                ServerMiddleware.AddMessageToQueue(player2.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameXX, game)));
            }
        }
    }
    public class GameManager22 : GameManager
    {
        public override void RunGame(Client player1, Client player2)
        {
            //Console.WriteLine("RunGame from 22");
            if (dateTime < DateTime.Now)
            {
                dateTime = DateTime.Now;
                dateTime = dateTime.AddSeconds(1.5f);

                float firePositionplayer1 = Ext.FloatRandRange(0f, 2.5f);
                float firePositionplayer2 = Ext.FloatRandRange(-2.5f, 0f);

                if (firePositionplayer1 % 2 == 0) //paianjeni Left right
                {
                    GameXX game1 = new GameXX
                    {
                        Data = new float[] { firePositionplayer1, firePositionplayer2 },
                    };
                    ServerMiddleware.AddMessageToQueue(player1.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameXX, game1)));
                    GameXX game2 = new GameXX
                    {
                        Data = new float[] { firePositionplayer2, firePositionplayer1 },
                    };
                    ServerMiddleware.AddMessageToQueue(player2.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameXX, game2)));
                }
                else
                {
                    GameXX game1 = new GameXX
                    {
                        Data = new float[] { firePositionplayer2, firePositionplayer1 },
                    };
                    ServerMiddleware.AddMessageToQueue(player1.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameXX, game1)));
                    GameXX game2 = new GameXX
                    {
                        Data = new float[] { firePositionplayer1, firePositionplayer2 },
                    };
                    ServerMiddleware.AddMessageToQueue(player2.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameXX, game2)));
                }
            }
        }
    }
    public class GameManager23 : GameManager
    {
        public override void RunGame(Client player1, Client player2)
        {
            //Console.WriteLine("RunGame from 23");
            if (dateTime < DateTime.Now)
            {
                dateTime = DateTime.Now;
                dateTime = dateTime.AddSeconds(2f);

                float firePositionplayer1 = Ext.FloatRandRange(0f, 3.991f);
                float firePositionplayer2 = Ext.FloatRandRange(-4.759f, 0f);
                float firePositionAux = Ext.FloatRandRange(-2.5f, 2.5f);

                if (firePositionplayer1 % 2 == 0) // same left right as game 22
                {
                    GameXX game1 = new GameXX
                    {
                        Data = new float[] { firePositionplayer1, firePositionplayer2, firePositionAux },
                    };
                    ServerMiddleware.AddMessageToQueue(player1.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameXX, game1)));
                    GameXX game2 = new GameXX
                    {
                        Data = new float[] { firePositionplayer2, firePositionplayer1, firePositionAux },
                    };
                    ServerMiddleware.AddMessageToQueue(player2.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameXX, game2)));
                }
                else
                {
                    GameXX game1 = new GameXX
                    {
                        Data = new float[] { firePositionplayer2, firePositionplayer1, firePositionAux },
                    };
                    ServerMiddleware.AddMessageToQueue(player1.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameXX, game1)));
                    GameXX game2 = new GameXX
                    {
                        Data = new float[] { firePositionplayer1, firePositionplayer2, firePositionAux },
                    };
                    ServerMiddleware.AddMessageToQueue(player2.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameXX, game2)));
                }
            }
        }
    }
    public class GameManager24 : GameManager
    {
        public override void RunGame(Client player1, Client player2)
        {
            //Console.WriteLine("RunGame from 24");
            if (dateTime < DateTime.Now)
            {
                dateTime = DateTime.Now;
                dateTime = dateTime.AddSeconds(1f);

                int TargetPlayer = Ext.IntRandRange(0, 2);
                Console.WriteLine($"TargetPlayer : {TargetPlayer}");

                if (player1.player.SyncPlayer != null && player2.player.SyncPlayer != null)
                {
                    switch (TargetPlayer)
                    {
                        case 0:
                            GameXX game0 = new GameXX
                            {
                                Data = new float[] { player1.player.SyncPlayer.position.X, player1.player.SyncPlayer.position.Y },
                            };
                            ServerMiddleware.AddMessageToQueue(player1.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameXX, game0)));
                            ServerMiddleware.AddMessageToQueue(player2.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameXX, game0)));
                            break;
                        case 1:
                            GameXX game1 = new GameXX
                            {
                                Data = new float[] { player2.player.SyncPlayer.position.X, player2.player.SyncPlayer.position.Y },
                            };
                            ServerMiddleware.AddMessageToQueue(player1.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameXX, game1)));
                            ServerMiddleware.AddMessageToQueue(player2.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameXX, game1)));
                            break;
                    }
                }
                else
                {
                    if (player1.player.SyncPlayer != null)
                    {
                        GameXX game0 = new GameXX
                        {
                            Data = new float[] { player1.player.SyncPlayer.position.X, player1.player.SyncPlayer.position.Y },
                        };
                        ServerMiddleware.AddMessageToQueue(player1.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameXX, game0)));
                        ServerMiddleware.AddMessageToQueue(player2.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameXX, game0)));
                    }
                    else if (player2.player.SyncPlayer != null)
                    {
                        GameXX game1 = new GameXX
                        {
                            Data = new float[] { player2.player.SyncPlayer.position.X, player2.player.SyncPlayer.position.Y },
                        };
                        ServerMiddleware.AddMessageToQueue(player1.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameXX, game1)));
                        ServerMiddleware.AddMessageToQueue(player2.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameXX, game1)));
                    }
                }
            }
        }
    }
    #endregion

    #region Game3X

    public class GameManager31 : GameManager
    {
        public override void RunGame(Client player1, Client player2)
        {
            //Console.WriteLine("RunGame from 31");
            if (dateTime < DateTime.Now)
            {
                dateTime = DateTime.Now;
                dateTime = dateTime.AddSeconds(4f);

                var TargetPoint = Ext.RandVect3(-1.9f, 1.9f, -1f, 4.35f);
                var LR = Ext.IntRandRange(0, 2);

                GameXX game = new GameXX
                {
                    Data = new float[] { TargetPoint.X, TargetPoint.Y, LR },
                };
                ServerMiddleware.AddMessageToQueue(player1.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameXX, game)));
                ServerMiddleware.AddMessageToQueue(player2.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameXX, game)));
            }
        }
    }
    public class GameManager32 : GameManager
    {
        public override void RunGame(Client player1, Client player2)
        {
            //Console.WriteLine("RunGame from 32");
            if (dateTime < DateTime.Now)
            {
                dateTime = DateTime.Now;
                dateTime = dateTime.AddSeconds(7f);

                float[] Sizes = { 1, 2, 3, 4 };
                float[] _sizes = Ext.ShuffleArray(Sizes);
                float[] _num = Ext.ShuffleArray(_sizes);
                float[] _pos = new float[2 + _sizes.Length + _num.Length];
                _pos[0] = Ext.IntRandRange(0, 2);
                _pos[1] = _sizes.Length;
                _sizes.CopyTo(_pos, 2);
                _num.CopyTo(_pos, 2 + _sizes.Length);

                GameXX game = new GameXX
                {
                    Data = _pos,
                };
                ServerMiddleware.AddMessageToQueue(player1.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameXX, game)));
                ServerMiddleware.AddMessageToQueue(player2.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameXX, game)));
            }
        }
    }
    public class GameManager33 : GameManager
    {
        public override void RunGame(Client player1, Client player2)
        {
            //Console.WriteLine("RunGame from 33");
            if (dateTime < DateTime.Now)
            {
                dateTime = DateTime.Now;
                dateTime = dateTime.AddSeconds(7f);

                float[] _pos = { Ext.IntRandRange(0, 2), Ext.IntRandRange(1, 4) };

                GameXX game = new GameXX
                {
                    Data = _pos,
                };
                ServerMiddleware.AddMessageToQueue(player1.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameXX, game)));
                ServerMiddleware.AddMessageToQueue(player2.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameXX, game)));
            }
        }
    }
    public class GameManager34 : GameManager
    {
        public override void RunGame(Client player1, Client player2)
        {
            //Console.WriteLine("RunGame from 34");
        }
    }
    #endregion

    #region Game4X

    public class GameManager41 : GameManager
    {
        public override void RunGame(Client player1, Client player2)
        {
            //Console.WriteLine("RunGame from 41");
            if (dateTime < DateTime.Now)
            {
                dateTime = DateTime.Now;
                dateTime = dateTime.AddSeconds(6f);

                int CurrentMode = Ext.IntRandRange(0, 2);
                float[] numbers = { 1, 2, 3, 4 };
                float[] sizes = { 0, 1, 2, 3 };
                float[] num = Ext.ShuffleArray(numbers);
                float[] size = Ext.ShuffleArray(sizes);

                var spawnPoint1 = Ext.RandVect3(-2.5f, 0f, -2.5f, 0f);
                var spawnPoint2 = Ext.RandVect3(0f, 2.5f, 0f, 5f);
                var spawnPoint3 = Ext.RandVect3(-2.5f, 0f, 0f, 5f);
                var spawnPoint4 = Ext.RandVect3(0f, 2.5f, -2.5f, 0f);
                float[] _pos = new float[] { CurrentMode,
                                                    spawnPoint1.X, spawnPoint1.Y, num[0], size[0],
                                                    spawnPoint2.X, spawnPoint2.Y, num[1], size[1],
                                                    spawnPoint3.X, spawnPoint3.Y, num[2], size[2],
                                                    spawnPoint4.X, spawnPoint4.Y, num[3], size[3],
                };

                GameXX game = new GameXX
                {
                    Data = _pos,
                };
                ServerMiddleware.AddMessageToQueue(player1.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameXX, game)));
                ServerMiddleware.AddMessageToQueue(player2.webSocket, JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.GameXX, game)));
            }
        }
    }
    public class GameManager42 : GameManager
    {
        public override void RunGame(Client player1, Client player2)
        {
            //Console.WriteLine("RunGame from 42");
        }
    }
    public class GameManager43 : GameManager
    {
        public override void RunGame(Client player1, Client player2)
        {
            //Console.WriteLine("RunGame from 43");
        }
    }
    public class GameManager44 : GameManager
    {
        public override void RunGame(Client player1, Client player2)
        {
            //Console.WriteLine("RunGame from 44");
        }
    }
    #endregion
}
