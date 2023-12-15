using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeDiscordBot
{
    public class test
    {
        public UserStatus status { get; }
    }

    public class User
    {
        
        public ulong id { get; set; }
        public string username { get; set; }
        public string profileurl { get; set; }
        public int money { get; set; }
        public string location { get; set; }
        public string? workplace { get; set; }
        public int cut { get; set; }
        public DateTime time { get; set; }
    }

    public class Company 
    {
        public string name { get; set; }
        public ulong owner { get; set; }
        public string type { get; set; }
        public int worth { get; set; }
        public int npcworkers { get; set; }
        public int playerworkers { get; set; }
        public int stock { get; set; }
        public int stockprice { get; set; }
        public string stockname { get; set; }
        public int factories { get; set; }
        public int operatingcost { get; set; }
        public int earnings { get; set; }
    }
}
