﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HTMLParser
{
    public class Session
    {
        public string ServerSeed { get; set; }
        public string PublicSeed { get; set; }
        public string Date { get; set; }
        
        public string Range { get; set; }

        public int MinRound { get; private set; }
        public int MaxRound { get; private set; }

        public Session()
        {
        }

        public void Compute()
        {
            string[] rounds = this.Range.Split('-');

            if (rounds.Length == 2)
            {
                this.MinRound = int.Parse(rounds[0]);
                this.MaxRound = int.Parse(rounds[1]);
            }
        }

        public string CreateHash(int round)
        {
            string seedToHash = string.Format($"{this.ServerSeed}-{this.PublicSeed}-{round}");
            string hash2 = GetHashSHA256(seedToHash);
            return hash2;
        }

        public string CreateHashByIndex(int index)
        {
            int round = MinRound + index;
            if (round > MaxRound)
            {
                return string.Empty;
            }

            string seedToHash = string.Format($"{this.ServerSeed}-{this.PublicSeed}-{round}");
            return GetHashSHA256(seedToHash);
        }

        public int ComputeResult(string hash)
        {
            string substring = hash.Substring(0, 8);
            long value = Convert.ToInt64(substring, 16);
            int result = (int)(value % 15);
            return result;
        }

        public long ComputeNumber(string hash)
        {
            string substring = hash.Substring(0, 8);
            long value = Convert.ToInt64(substring, 16);
            return value;
        }

        public string ComputeStringResult(long number)
        {
            string rstring = "black";
            if (number == 0)
                rstring = "bonus";
            else if (number < 8)
                rstring = "orange";

            return rstring;
        }

        public string GetHashSHA256(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            SHA256Managed hashString = new SHA256Managed();
            byte[] hash = hashString.ComputeHash(bytes);

            StringBuilder stringBuilder = new StringBuilder();

            foreach (byte b in hash)
                stringBuilder.AppendFormat("{0:X2}", b);

            return stringBuilder.ToString();
        }
    }
}
