using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace EasyVote
{
    public class PollReader
    {
        public PollList writeFile(string file)
        {
            StreamWriter tw = new StreamWriter(file, true);

            PollList pollList = new PollList();

            pollList.AddItem(new StandardPoll("Yes/No", "Vote Yes or No.", "none", 0));
            pollList.AddItem(new StandardPoll("Night/Guardian", "Vote for a dungeon guardian and night time.", "Night", 68));
            pollList.AddItem(new StandardPoll("Day", "Vote for day time :]", "Day", 0));

            tw.Write(JsonConvert.SerializeObject(pollList, Formatting.Indented));
            tw.Close();

            return pollList;
        }

        public PollList readFile(string file)
        {
            TextReader tr = new StreamReader(file);
            string raw = tr.ReadToEnd();
            tr.Close();
            PollList pollList = JsonConvert.DeserializeObject<PollList>(raw);
            return pollList;
        }
    }
}
