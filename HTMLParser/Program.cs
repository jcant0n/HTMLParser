using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Security.Cryptography;

namespace HTMLParser
{
    class Program
    {
        static void Main(string[] args)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument document = web.Load("https://csgoempire.com/provably-fair");

            var nodes = document.DocumentNode.SelectNodes("//div[contains(@class,'history-content')]");

            List<Day> seeds = new List<Day>();

            foreach (var node in nodes)
            {
                var child = node.ChildNodes[1];

                Day day = new Day()
                {
                    Date = child.ChildNodes[1].InnerText,
                    ServerSeed = child.ChildNodes[3].InnerText,
                    PublicSeed = child.ChildNodes[5].InnerText,
                    Range = child.ChildNodes[7].InnerText

                };

                seeds.Add(day);
            }

            // Create the hash
            var seed = seeds[1];
            string round = "1000201";
            string seedToHash = string.Format($"{seed.ServerSeed}-{seed.PublicSeed}-{round}");
            string hash = GetHashSHA256(seedToHash);
            Console.WriteLine(hash);

            // Calculate round result
            string substring = hash.Substring(0, 8);
            int value = Convert.ToInt32(substring, 16);
            int result = value % 15;
            Console.WriteLine(result);


            Console.ReadLine();
        }

        public static string GetHashSHA256(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            SHA256Managed hashString = new SHA256Managed();
            byte[] hash = hashString.ComputeHash(bytes);

            StringBuilder stringBuilder = new StringBuilder();

            foreach (byte b in hash)
                stringBuilder.AppendFormat("{0:X2}", b);

            return stringBuilder.ToString().ToLower();
        }
    }
}
