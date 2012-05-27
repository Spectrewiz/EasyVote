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

            pollList.AddItem(new StandardPoll("Yes/No", "Vote Yes or No.", "none", false, new List<Monster> { }, new List<string> { }));
            pollList.AddItem(new StandardPoll("Day", "Vote for day time :]", "Day", false, new List<Monster> { }, new List<string> { }));
            pollList.AddItem(new StandardPoll("Night/Guardian", "Vote for a dungeon guardian and night time.", "Night", false, new List<Monster> { new Monster(68, 1) }, new List<string> { }));
            pollList.AddItem(new StandardPoll("MultipleMonsters", "Vote for 3 chaos elementals, 2 blue slimes, and 1 werewolf.", "Night", false, new List<Monster> { new Monster(120, 3), new Monster(1, 2), new Monster(104, 1) }, new List<string> { }));
            pollList.AddItem(new StandardPoll("Commands", "Vote for a broadcast, a meteor strike, and a convert corruption. No matter what you vote - results will be visible.", "none", true, new List<Monster> { }, new List<string> { "/bc yes outvoted no", "dropmeteor", "convertcorruption" }));

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
