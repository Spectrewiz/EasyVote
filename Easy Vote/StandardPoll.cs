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
        public string Time;
        public List<Monster> Monsters;
        public List<string> Commands;

        public StandardPoll(string PollName, string Question, string Time, List<Monster> Monsters, List<string> Commands)
        {
            this.PollName = PollName;
            this.Question = Question;
            this.Time = Time;
            this.Monsters = Monsters;
            this.Commands = Commands;
        }

        public string getName()
        {
            return PollName.ToLower();
        }

        public void spawnMonsters(TSPlayer player)
        {
            foreach (Monster m in Monsters)
            {
                if (m.id != 0)
                {
                    TShockAPI.Commands.HandleCommand(player, string.Format("/spawnmob {0} {1}", m.id, m.amount));
                }
            }
        }

        public void handleCommands(TSPlayer player)
        {
            foreach (string cmd in Commands)
            {
                if (!cmd.StartsWith("/"))
                {
                    TShockAPI.Commands.HandleCommand(player, "/" + cmd);
                }
                else
                    TShockAPI.Commands.HandleCommand(player, cmd);
            }
        }
    }

    [Serializable]
    public class Monster
    {
        public int id;
        public int amount;

        public Monster(int id, int amount)
        {
            this.id = id;
            this.amount = amount;
        }
    }
}