using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebSocketServer.Middleware;

namespace WebSocketServer.GameServerLogic
{
    public class MatchMaking
    {
        public static ConcurrentQueue<Match> TrippleGameQueue = new ConcurrentQueue<Match>();
        public static ConcurrentDictionary<string, Match> OnGoingMatches = new ConcurrentDictionary<string, Match>();

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
        private class MatchFound
        {
            public string opFound;
            public string matchID;
            public int[] games;
        }
        public static async Task<string> IncomingTriRun(Client fromClient, int selectedGame)
        {
            if (TrippleGameQueue.Count > 0) // if there is a free match for the client to join
            {
                TrippleGameQueue.TryDequeue(out Match match);
                if (match.player1.Equals(fromClient))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("HOW DAFQ THIS HAPPEND, THE ACTUAL FUCKING CLIENT MATCHED WITH HIS FUCKING SELF");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                match.ID = Guid.NewGuid().ToString();
                match.FoundMatch(fromClient, selectedGame);
                if (!OnGoingMatches.TryAdd(match.ID, match))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("HOW DAFQ THIS HAPPEND, THE FUCKING MATCH COULDN'T BE ADDED");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                MatchFound matchFound = new MatchFound
                {
                    opFound = "Opponenet found!",
                    matchID = match.ID,
                    games = match.Games,
                };
                await fromClient.SendMessageAsync(JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.MatchFound, matchFound)));
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{fromClient.player.username} was sent matchFound");
                Console.ForegroundColor = ConsoleColor.White;

                return await Task.Delay(2000).ContinueWith(o => { return match.LoadNextScene(fromClient); });
            }
            else // there is no free match, so we declare one for the client
            {
                var aux = new Match(3, fromClient, selectedGame);
                TrippleGameQueue.Enqueue(aux);
                Console.Write($"Client {fromClient.player.username} Enqued --> ");
                return await HasFoundPair(fromClient, aux);
            }
        }

        private static async Task<string> HasFoundPair(Client fromClient, Match match)
        {
            // ALTERNATIVE WAY : SpinWait.SpinUntil(() => match.player2 != null);
            while (match.player2 == null)
            {
                await Task.Delay(50);
                Console.WriteLine($"{fromClient.player.username} is waiting for pair");
            }
            MatchFound matchFound = new MatchFound
            {
                opFound = "Opponenet found!",
                matchID = match.ID,
                games = match.Games,
            };
            await fromClient.SendMessageAsync(JsonConvert.SerializeObject(new MessageWrap((int)ServerPackets.MatchFound, matchFound)));
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{fromClient.player.username} was sent matchFound");
            Console.ForegroundColor = ConsoleColor.White;

            return await Task.Delay(2000).ContinueWith(o => { return match.LoadNextScene(fromClient); });
        }

        public static async Task<string> IncomingQuadRun(Client fromClient, int selectedGame1, int selectedGame2)
        {
            return null;
        }
    }
}
