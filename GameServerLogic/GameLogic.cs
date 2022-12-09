using System;
using WebSocketServer.Middleware;

namespace WebSocketServer.GameServerLogic
{
    public class GameLogic
    {
        public static void Update()
        {
            foreach(var match in MatchMaking.OnGoingMatches)
            {
                match.Value.Update();
            }

            ThreadManager.UpdateMain();
        }
    }
}
