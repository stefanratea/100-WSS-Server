using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using WebSocketServer.Middleware;

namespace WebSocketServer.GameServerLogic
{
    public class Player
    {
        public string id;
        public string username;

        public string CurrentEnemyID;
        public int CurrentSelectedGame;

        public int[] Score;
        public int Crowns;
        public int CurrentGameIndex;
        public string CurrentSceneName;

        public bool SceneLoaded;
        public bool PlayerSpawned;
        public bool GameUpdated;
        public bool EarlyLoss;
        public bool LeftTheEndScreen;
        public bool Ready;

        public SyncObject SyncPlayer;
        public ConcurrentDictionary<int, SyncObject> SyncedObjects = new ConcurrentDictionary<int, SyncObject>();

        public string Emotion;

        public Player(string _id, string _username)
        {
            id = _id;
            username = _username;

            Score = new int[3];
            CurrentGameIndex = 0;
            Crowns = 0;

            ResetAllTriggers();
        }
        public void AddObject(int objectID, string name, Vector3 position, Vector3 size)
        {
            if(SyncedObjects.TryAdd(objectID, new SyncObject(objectID, name, position, size)))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{username} spawned : {name} with ID : {objectID}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{username} couldn't spawn : {name} with ID : {objectID}");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        public void SetEmotion(string _emotion)
        {
            Emotion = _emotion;
        }
        public void SetScore(int _score)
        {
            Score[CurrentGameIndex] = _score;
            GameUpdated = true;
            CurrentGameIndex++;
        }
        public void SceneWasLoaded(string _sceneName)
        {
            if (_sceneName.Equals(CurrentSceneName))
            {
                SceneLoaded = true;
            }
            else
            {
                Console.WriteLine($"{username} tried to load a different scene than expected");
            }
        }
        public void LeftTheGame()
        {
            LeftTheEndScreen = true;
        }
        public void ResetAllTriggers()
        {
            SceneLoaded = false;
            PlayerSpawned = false;
            GameUpdated = false;
            EarlyLoss = false;
            LeftTheEndScreen = false;
            Ready = false;
        }
        public void ResetGameStats()
        {
            for (int i = 0; i < Score.Length; i++)
            {
                Score[i] = 0;
            }
            Emotion = "";
            CurrentGameIndex = 0;
            Crowns = 0;
            CurrentSceneName = "";
        }

        public int TotalScore()
        {
            int Total = 0;
            for (int i = 0; i < Score.Length; i++)
            {
                Total += Score[i];
            }
            return Total;
        }
    }
}
