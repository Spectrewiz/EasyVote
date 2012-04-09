using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyVote
{
    public class PollList
    {
        public List<StandardPoll> polls;

        public PollList()
        {
            polls = new List<StandardPoll>();
        }

        public void AddItem(StandardPoll p)
        {
            polls.Add(p);
        }

        public StandardPoll findPoll(string name)
        {
            foreach (StandardPoll p in polls)
            {
                if (p.getName().ToLower() == name.ToLower())
                {
                    return p;
                }
            }
            return null;
        }

        public List<string> getList()
        {
            List<string> PollNames = new List<string>();
            foreach (StandardPoll p in polls)
            {
                PollNames.Add(p.PollName);
            }
            return PollNames;
        }
    }
}