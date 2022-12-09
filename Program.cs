using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebSocketServer.GameServerLogic;
using WebSocketServer.Middleware;

namespace WebSocketServer
{
    public class Program
    {
        private static bool isRunning = false;
        public const int TICKS_PER_SEC = 60;
        public const float MS_PER_TICK = 1000f / TICKS_PER_SEC;

        public static void Main(string[] args)
        {

            ClientsDataBase.ConnectDB();
            //Console.Title = "Game Server";

            Thread mainThread = new Thread(MainThread);
            mainThread.Start();

            Thread sendThread = new Thread(RunSend);
            sendThread.Start();

            isRunning = true;

            CreateHostBuilder(args).Build().Run();
        }

        private static async void MainThread()
        {
            await Task.Delay(2000);
            Console.WriteLine($"Main thread started. Running at {TICKS_PER_SEC} ticks per second.");
            DateTime _nextLoop = DateTime.Now;

            while (isRunning)
            {
                while (_nextLoop < DateTime.Now)
                {
                    GameLogic.Update();

                    _nextLoop = _nextLoop.AddMilliseconds(MS_PER_TICK);

                    if (_nextLoop > DateTime.Now)
                    {
                        //Thread.Sleep(_nextLoop - DateTime.Now);
                        await Task.Delay(_nextLoop - DateTime.Now);
                    }
                }
            }
        }

        private static async void RunSend()
        {
            await Task.Delay(2000);
            Console.WriteLine("WebSocket Message Sender looping.");
            while (isRunning)
            {
                await ServerLogic.RunSend();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    //var port = Environment.GetEnvironmentVariable("PORT") ?? "4000";
                    //var url = $"http://0.0.0.0:{port}";
                    webBuilder.UseStartup<Startup>();//.UseUrls(url);
                });
    }
}
