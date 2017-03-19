using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Security.Cryptography;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;

namespace HTMLParser
{
    class Program
    {
        static void Main(string[] args)
        {
            SQLiteConnection dbConnection;
            dbConnection = new SQLiteConnection("Data Source=CSEmpireDB.sqlite; Version=3;");

            if (!File.Exists("CSEmpireDB.sqlite"))
            {
                // Create database
                Console.WriteLine("Creating database");
                Stopwatch timer = new Stopwatch();
                timer.Start();

                SQLiteConnection.CreateFile("CSEmpireDB.sqlite");
                dbConnection.Open();

                string sql = "create table sessions (Date varchar(15), ServerSeed varchar(65), PublicSeed varchar(10), Range varchar(25))";
                SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
                command.ExecuteNonQuery();

                // Load all data
                HtmlWeb web = new HtmlWeb();
                HtmlDocument document = web.Load("https://csgoempire.com/provably-fair");

                var nodes = document.DocumentNode.SelectNodes("//div[contains(@class,'history-content')]");

                foreach (var node in nodes)
                {
                    var child = node.ChildNodes[1];

                    string date = child.ChildNodes[1].InnerText;
                    string serverSeed = child.ChildNodes[3].InnerText;
                    string publicSeed = child.ChildNodes[5].InnerText;
                    string range = child.ChildNodes[7].InnerText;

                    sql = $"insert into sessions (Date, ServerSeed, PublicSeed, Range) values('{date}','{serverSeed}','{publicSeed}','{range}')";
                    command = new SQLiteCommand(sql, dbConnection);
                    command.ExecuteNonQuery();
                }
                dbConnection.Close();

                timer.Stop();
                Console.WriteLine($"Database created, time: {timer.ElapsedMilliseconds}");
            }

            // GetData
            dbConnection.Open();
            string sqlSelect = "select * from sessions";
            SQLiteCommand cmdSelect = new SQLiteCommand(sqlSelect, dbConnection);
            SQLiteDataReader reader = cmdSelect.ExecuteReader();

            List<Session> sessions = new List<Session>();
            while (reader.Read())
            {
                Session newSessions = new Session()
                {
                    Date = reader["Date"].ToString(),
                    ServerSeed = reader["ServerSeed"].ToString(),
                    PublicSeed = reader["PublicSeed"].ToString(),
                    Range = reader["Range"].ToString()
                };

                sessions.Add(newSessions);
            }

            dbConnection.Close();

            // Generate files to study on Excel

            // Create Folder
            string directoryName = "Sessions";
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            // Create the hash
            for (int j = 1; j < sessions.Count; j++)
            {
                var s = sessions[j];
                s.Compute();

                // Create a file
                StreamWriter sw;
                var dt = DateTime.Parse(s.Date);
                string fileName = $"Sessions\\{dt.ToString("yyyy_MM_dd")}.txt";
                if (!File.Exists(fileName))
                {
                    sw = File.CreateText(fileName);
                }
                else
                {
                    continue;
                }

                for (int i = s.MinRound, index = 1; i <= s.MaxRound; i++, index++)
                {
                    string hash = s.CreateHash(i);
                    long number = s.ComputeNumber(hash);
                    int module = (int)(number % 15);
                    string rstring = s.ComputeStringResult(module);

                    sw.WriteLine($"{index}\t{i}\t{number}\t{module}\t{rstring}");
                }
                sw.Close();
            }
            Console.WriteLine("END");

            Console.ReadLine();
        }
    }
}
