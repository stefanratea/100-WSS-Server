using System;
using System.Collections.Generic;

namespace WebSocketServer
{
    public class ClientInfo
    {
        public string Name;
        public Dictionary<int, Game> Games = new Dictionary<int, Game>();
        public List<string> Friends = new List<string>();

        public ClientInfo(string _name)
        {
            Name = _name;
            Games.Add(11, new Game(11));
            Games.Add(12, new Game(12));
            Games.Add(13, new Game(13));
            Games.Add(14, new Game(14));
            Games.Add(21, new Game(21));
            Games.Add(22, new Game(22));
            Games.Add(23, new Game(23));
            Games.Add(24, new Game(24));
            Games.Add(31, new Game(31));
            Games.Add(32, new Game(32));
            Games.Add(33, new Game(33));
            Games.Add(34, new Game(34));
            Games.Add(41, new Game(41));
            Games.Add(42, new Game(42));
            Games.Add(43, new Game(43));
            Games.Add(44, new Game(44));
        }
        public void UpdateGame(int gameID, int crownsWon, int newScore)
        {
            Games.TryGetValue(gameID, out var refGame);
            refGame.UpdateStats(crownsWon, newScore);
        }
        public void AddFriend(string friendName)
        {
            Friends.Add(friendName);
        }
    }

    public class Game
    {
        public int ID;
        public int Crowns;
        public int max_Score;
        public int max_Score_Time;
        public bool Owned;

        public Game()
        {
        }
        public Game(int id)
        {
            ID = id;
            Crowns = 0;
            max_Score = 0;
            max_Score_Time = 0;
            Owned = true;
        }
        public void UpdateStats(int crownsWon, int newScore, int seconds = 0)
        {
            Crowns += crownsWon;
            if (max_Score < newScore)
            {
                max_Score = newScore;
                max_Score_Time = seconds;
            }
            if (Crowns < 0) Crowns = 0;
        }
    }
}
