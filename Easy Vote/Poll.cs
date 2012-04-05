using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using Terraria;
using System.IO;
using Newtonsoft.Json;

namespace EasyVote
{
    [Serializable]
    public class StandardPoll
    {
        public string PollName;
        public string Question;
        public string DayNight;
        public int Monster;

        public StandardPoll(string p, string q, string dn, int m)
        {
            PollName = p.ToLower();
            Question = q;
            DayNight = dn;
            Monster = m;
        }

        public string getName()
        {
            return PollName;
        }
    }
}