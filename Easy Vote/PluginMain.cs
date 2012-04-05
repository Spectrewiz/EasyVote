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
            get { return new Version(0, 9, 0); }
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
            Commands.ChatCommands.Add(new Command("Poll", GetResults, "getvotes", "getresults", "findvotes", "findresults"));
            Commands.ChatCommands.Add(new Command("Poll" ,StartPoll, "startpoll", "startvote"));
            Commands.ChatCommands.Add(new Command(Vote, "vote"));
            PollReader reader = new PollReader();
            save = Path.Combine(TShock.SavePath, "Polls.json");

            if (File.Exists(save))
            {
                polls = reader.readFile(save);
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
        public static string CurrentPoll = null;
        public static void StartPoll(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendMessage("Syntax: /startpoll <pollname>", Color.Cyan);
                args.Player.SendMessage("The poll names can be found inside the Polls.json file", Color.Cyan);
            }
            else
            {
                try
                {
                    string name = args.Parameters[0].ToLower();
                    if (FindPoll(name).PollName != null)
                    {
                        args.Player.SendMessage(FindPoll(name).PollName + " has been started!", Color.Yellow);
                        OpenPoll = true;
                        CurrentPoll = FindPoll(name).PollName;
                        foreach (Player player in Players)
                        {
                            player.TSPlayer.SendMessage("New poll (" + CurrentPoll + ") has been started! Type /vote for more info!", Color.Cyan);
                        }
                    }
                }
                catch (Exception e)
                {
                    args.Player.SendMessage("Cannot find the specified poll \"" + args.Parameters[0] + "\"", Color.Red);
                    args.Player.SendMessage("Check the Polls.json file to make sure you are using the right poll name.", Color.Red);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Message);
                    Console.ResetColor();
                }
            }
        }

        public static void GetResults(CommandArgs args)
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
            args.Player.SendMessage(string.Format("{0} player(s) voted yes and {1} player(s) voted no.", i, x), Color.Yellow);
            if (i > x)
            {
                switch (currentpoll.DayNight.ToLower())
                {
                    case "day":
                        Commands.HandleCommand(TSServerPlayer.Server, "/time day");
                        break;
                    case "night":
                        Commands.HandleCommand(TSServerPlayer.Server, "/time night");
                        break;
                }
                if (currentpoll.Monster != 0)
                    Commands.HandleCommand(TSServerPlayer.Server, string.Format("/spawnmob {0}", currentpoll.Monster));
            }
            foreach (Player player in Players)
            {
                player.SetVote(Player.VoteResults.novote);
                player.TSPlayer.SendMessage("Poll " + CurrentPoll + " has been closed.", Color.Cyan);
            }
            OpenPoll = false;
            CurrentPoll = null;
        }

        public static void Vote(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                if (OpenPoll)
                {
                    args.Player.SendMessage("Current Poll: " + CurrentPoll, Color.Cyan);
                    args.Player.SendMessage(FindPoll(CurrentPoll).Question, Color.Cyan);
                    args.Player.SendMessage("Vote yes or no (/vote <yes/no>)", Color.Yellow);
                }
                else
                {
                    args.Player.SendMessage("No polls available.", Color.Red);
                }
            }
            else if (OpenPoll == true)
            {
                var ListedPlayer = Player.GetPlayerByName(args.Player.Name);
                switch (args.Parameters[0].ToLower())
                {
                    case "yes":
                        args.Player.SendMessage("You voted Yes", Color.Cyan);
                        ListedPlayer.SetVote(Player.VoteResults.yes);
                        break;
                    case "no":
                        args.Player.SendMessage("You voted No", Color.Cyan);
                        ListedPlayer.SetVote(Player.VoteResults.no);
                        break;
                    default:
                        args.Player.SendMessage(string.Format("Invalid vote (your vote \"{0}\" did not match the possible votes: yes or no)", args.Parameters[0]), Color.Red);
                        break;
                }
            }
        }
    }
    #endregion
}