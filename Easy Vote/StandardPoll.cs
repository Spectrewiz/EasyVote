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

        public StandardPoll(string PollName, string Question, string DayNight, int Monster)
        {
            this.PollName = PollName;
            this.Question = Question;
            this.DayNight = DayNight;
            this.Monster = Monster;
        }

        public string getName()
        {
            return PollName.ToLower();
        }
    }
}