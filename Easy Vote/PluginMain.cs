using System;
using System.Collections.Generic;
using System.Reflection;
using System.Drawing;
using Terraria;
using Hooks;
using TShockAPI;
using TShockAPI.DB;
using System.ComponentModel;
using System.IO;

namespace EasyVote
{
    #region Plugin Stuff
    [APIVersion(1, 11)]
    public class EasyVote : TerrariaPlugin
    {
        public static string save = "";
        public static List<Player> Players = new List<Player>();
        public static PollList polls;

        public static StandardPoll FindPoll(string name)
        {
            return polls.findPoll(name);
        }

        public override string Name
        {
            get { return "Easy Vote"; }
        }

        public override string Author
        {
            get { return "Spectrewiz"; }
        }

        public override string Description
        {
            get { return "Allows users to create polls and other users to vote"; }
        }

        public override Version Version
        {
            get { return new Version(0, 0, 1); }
        }

        public override void Initialize()
        {
            GameHooks.Update += OnUpdate;
            GameHooks.Initialize += OnInitialize;
            NetHooks.GreetPlayer += OnGreetPlayer;
            ServerHooks.Leave += OnLeave;
            ServerHooks.Chat += OnChat;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GameHooks.Update -= OnUpdate;
                GameHooks.Initialize -= OnInitialize;
                NetHooks.GreetPlayer -= OnGreetPlayer;
                ServerHooks.Leave -= OnLeave;
                ServerHooks.Chat -= OnChat;
            }
            base.Dispose(disposing);
        }

        public EasyVote(Main game)
            : base(game)
        {
            Order = 10;
        }

        public void OnInitialize()
        {
            Commands.ChatCommands.Add(new Command(GetResults, "getvotes", "getresults", "findvotes", "findresults"));
            Commands.ChatCommands.Add(new Command(StartPoll, "startpoll"));
            Commands.ChatCommands.Add(new Command(Vote, "vote"));
            PollReader reader = new PollReader();
            save = @"tshock\Polls.cfg";

            if (File.Exists(save))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(polls.polls.Count + " polls have been loaded.");
                Console.ResetColor();
            }
            else
            {
                polls = reader.writeFile(save);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No polls found! Basic poll file being created. 3 example polls loaded.");
                Console.ResetColor();
            }
        }

        public void OnUpdate()
        {
        }

        public void OnGreetPlayer(int who, HandledEventArgs e)
        {
            lock (Players)
                Players.Add(new Player(who));
        }

        public void OnLeave(int ply)
        {
            lock (Players)
            {
                for (int i = 0; i < Players.Count; i++)
                {
                    if (Players[i].Index == ply)
                    {
                        Players.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        public void OnChat(messageBuffer msg, int ply, string text, HandledEventArgs e)
        {
        }
    #endregion
    #region Commands
        public static bool OpenPoll = false;
        public static string CurrentPoll;
        public static void StartPoll(CommandArgs args)
        {
            if (args.Player.Group.HasPermission("Poll"))
            {
                if (args.Parameters.Count < 1)
                {
                    args.Player.SendMessage("Syntax: /startpoll <pollname>", Color.DarkCyan);
                    args.Player.SendMessage("The poll name should be inside the Polls.cfg file.", Color.DarkCyan);
                }
                else
                {
                    string name = args.Parameters[0].ToLower();
                    if (FindPoll(name).PollName == name)
                    {
                        OpenPoll = true;
                        CurrentPoll = FindPoll(name).PollName;
                    }
                }
            }
        }

        public static void GetResults(CommandArgs args)
        {
            if (args.Player.Group.HasPermission("Poll"))
            {
                int i = 0;
                int x = 0;
                var currentpoll = FindPoll(CurrentPoll);
                var listedplayer = Player.GetPlayerByName(args.Player.Name);
                foreach (Player player in Players)
                {
                    if (player.GetVoteResult() != Player.VoteResults.novote)
                    {
                        if (player.GetVoteResult() == Player.VoteResults.yes)
                            i++;
                        else
                            x++;
                    }
                }
                args.Player.SendMessage(string.Format("{0} voted yes and {1} voted no.", i, x), Color.DarkCyan);
                if (i > x)
                {
                    switch (currentpoll.DayNight.ToLower())
                    {
                        case "day":
                            Commands.HandleCommand(listedplayer.TSPlayer, "/time day");
                            break;
                        case "night":
                            Commands.HandleCommand(listedplayer.TSPlayer, "/time night");
                            break;
                    }
                    if (currentpoll.Monster != 0)
                        Commands.HandleCommand(listedplayer.TSPlayer, string.Format("/spawnmob {0}", currentpoll.Monster));
                }
                foreach (Player player in Players)
                {
                    player.SetVote(Player.VoteResults.novote);
                }
            }
        }

        public static void Vote(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendMessage("Current Poll: " + CurrentPoll, Color.DarkCyan);
                args.Player.SendMessage(FindPoll(CurrentPoll).Question, Color.DarkCyan);
                args.Player.SendMessage("Vote Yes or No");
            }
            else if (OpenPoll == true)
            {
                var ListedPlayer = Player.GetPlayerByName(args.Player.Name);
                switch (args.Parameters[0].ToLower())
                {
                    case "yes":
                        args.Player.SendMessage("You voted Yes", Color.DarkCyan);
                        ListedPlayer.SetVote(Player.VoteResults.yes);
                        break;
                    case "no":
                        args.Player.SendMessage("You voted No", Color.DarkCyan);
                        ListedPlayer.SetVote(Player.VoteResults.no);
                        break;
                    default:
                        args.Player.SendMessage(string.Format("Invalid vote (your vote {0} did not match the possible votes: yes or no)", args.Parameters[0]), Color.Red);
                        break;
                }
            }
        }
    }
    #endregion
}