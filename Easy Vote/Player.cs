using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using TShockAPI;

namespace EasyVote
{
    public class Player
    {
        public int Index { get; set; }
        public TSPlayer TSPlayer { get { return TShock.Players[Index]; } }
        //Add other variables here - MAKE SURE YOU DON'T MAKE THEM STATIC

        public Player(int index)
        {
            Index = index;
        }

        public static Player GetPlayerByName(string name)
        {
            var player = TShock.Utils.FindPlayer(name)[0];
            if (player != null)
            {
                foreach (Player ply in EasyVote.Players)
                {
                    if (ply.TSPlayer == player)
                    {
                        return ply;
                    }
                }
            }
            return null;
        }

        protected VoteResults Vote = VoteResults.novote;
        public void SetVote(VoteResults votestate)
        {
            Vote = votestate;
        }
        public VoteResults GetVoteResult()
        {
            return Vote;
        }
        public enum VoteResults
        {
            yes,
            no,
            novote
        }
    }
}
