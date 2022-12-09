using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using WebSocketServer.Middleware;

namespace WebSocketServer
{
    public class ClientsDataBase
    {
        static MySqlConnection conDB;
        public static void ConnectDB()
        {
            string cs = @"server=34.91.48.5;userid=root;password=dataoil97;database=herbydatabase";

            conDB = new MySqlConnection(cs);
            conDB.Open();
            Console.WriteLine($"MySQL version : {conDB.ServerVersion}");
        }
        //static string DataBasePath = "/Users/rateastefan/Desktop/ServerDataBase.txt";
        //static string DataBasePath = "ServerDataBase.txt";
        //public static List<ClientInfo> DataList = new List<ClientInfo>();

        /*
        public static void LoadData()
        {
            string serializedJson = File.ReadAllText(DataBasePath);
            DataList = JsonConvert.DeserializeObject<List<ClientInfo>>(serializedJson);

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"Loaded Clients Data, count : " + DataList.Count);
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static ClientInfo CheckName(string _name)
        {
            if (DataList != null) foreach (ClientInfo client in DataList)
                {
                    if (client.Name.Equals(_name)) return client;
                }
            else DataList = new List<ClientInfo>();
            return null;
        }
        public static ClientInfo AddNewClient(string _name)
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine($"New Client !yaaay! NAME:{_name}");
            Console.ForegroundColor = ConsoleColor.White;

            var aux = new ClientInfo(_name);
            DataList.Add(aux);
            SaveData();
            return aux;
        }
        */
        public static async Task UpdateINFO(Client client)
        {
            var cmd = new MySqlCommand($"update clients set data='{JsonConvert.SerializeObject(client.info)}' where username='{client.player.username}';", conDB);
            await cmd.ExecuteNonQueryAsync();
        }
        public static async Task<ClientInfo> CheckName(string _name)
        {
            var cmd = new MySqlCommand($"select data from clients where username='{_name}';", conDB);

            MySqlDataReader rdr = cmd.ExecuteReader();
            if (await rdr.ReadAsync())
            {
                var result = JsonConvert.DeserializeObject<ClientInfo>(rdr.GetString(0));
                await rdr.CloseAsync();
                return result;
            }
            await rdr.CloseAsync();
            return null;
        }
        public static async Task<ClientInfo> AddNewClient(string _name)
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine($"New Client !yaaay! NAME:{_name}");
            Console.ForegroundColor = ConsoleColor.White;

            var aux = new ClientInfo(_name);
            var sql = "INSERT INTO clients(username, data) VALUES(@username, @data)";
            var cmd = new MySqlCommand(sql, conDB);

            cmd.Parameters.AddWithValue("@username", _name);
            cmd.Parameters.AddWithValue("@data", JsonConvert.SerializeObject(aux));
            cmd.Prepare();

            await cmd.ExecuteNonQueryAsync();

            Console.WriteLine("new client inserted");
            return aux;
        }
    }
}
