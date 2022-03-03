using Newtonsoft.Json.Linq;
using System;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace UpdateToken
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().Wait();
        }
        static async Task MainAsync()
        {
            try
            {
                SQLiteConnection sqlite_conn = CreateConnection();
                string token = ReadData(sqlite_conn);
                UpdateTokenAsync(token, sqlite_conn).Wait();
            }
         
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static string ReadData(SQLiteConnection conn)
        {
            string token = "";
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT token from token ORDER BY id DESC LIMIT 1";

            sqlite_datareader = sqlite_cmd.ExecuteReader();
            while (sqlite_datareader.Read())
            {
                string myreader = sqlite_datareader.GetString(0);
                Console.WriteLine(myreader);
                token = myreader;
            }
            
            return token;
        }

        public static SQLiteConnection CreateConnection()
        {
            SQLiteConnection sqlite_conn;
            // Create a new database connection:
            sqlite_conn = new SQLiteConnection("Data Source=c:/sqlite/releasethehounds");
            // Open the connection:
            try
            {
                sqlite_conn.Open();
            }
            catch (Exception ex)
            {

            }
            return sqlite_conn;
        }

        public static async Task UpdateTokenAsync(string token, SQLiteConnection conn)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                var uri = new Uri(string.Format("https://graph.instagram.com/refresh_access_token?grant_type=ig_refresh_token&access_token={0}", token));

                var response = await client.GetAsync(uri);

                string textResult = await response.Content.ReadAsStringAsync();
                string newtoken = "";
               JObject data = JObject.Parse(textResult);
                if (data != null)
                {
                    
                        Console.WriteLine(data["access_token"].ToString());
                        newtoken = data["access_token"].ToString();
                    
                }


                SQLiteCommand sqlite_cmd;
                sqlite_cmd = conn.CreateCommand();
                sqlite_cmd.CommandText = string.Format("UPDATE token SET token ='{0}'", newtoken);
                sqlite_cmd.ExecuteNonQuery();

                conn.Close();
            }
        }
    }
}
