using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_bot.profilesystem
{
    public class DUser
    {
        public string Username { get; set; }
        public int Coins { get; set; }
        public string avatarurl { get; set; }
    }

    public class Loan
    {
        public DUser Loaner { get; set; }
        public DUser Banker { get; set; }
        public int Coins { get; set; }
        public int Time { get; set; }
        public int Payback { get; set; }
        public bool active { get; set; }

    }
    public class Company
    {
        public DUser owner { get; set; }
        public int coingeneration { get; set; }
        public int coingenerationupgrade { get; set; }
        public int workers { get; set; }
        public int operatingcost { get; set; }
        public int operationupgrades { get; set; }
        public string bustype { get; set; }

    }
}
