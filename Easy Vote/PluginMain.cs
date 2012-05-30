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
using System.Net;

namespace EasyVote
{
    #region Plugin Stuff
    [APIVersion(1, 12)]
    public class EasyVote : TerrariaPlugin
    {
        public static string save = "";
        public static List<Player> Players = new List<Player>();
        public static PollList polls;
        public static int update = 0;
        public static string downloadFromUpdate;
        public static string versionFromUpdate;
        public static DateTime lastupdate = DateTime.Now;

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
            get { return "Create polls and watch as responses come in!"; }
        }

        public override Version Version
        {
            get { return new Version(0, 9, 61); }
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
            Commands.ChatCommands.Add(new Command(Poll, "poll", "polls"));
            PollReader reader = new PollReader();
            save = Path.Combine(TShock.SavePath, "Polls.json");

            if (File.Exists(save))
            {
                try
                {
                    polls = reader.readFile(save);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(polls.polls.Count + " polls have been loaded.");
                    Console.ResetColor();
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error in Polls.json file! Check log for more details.");
                    Console.WriteLine(e.Message);
                    Console.ResetColor();
                    Log.Error("--------- Config Exception in EasyVote Config file (Polls.json) ---------");
                    Log.Error(e.Message);
                    Log.Error("------------------------------- Error End -------------------------------");
                }
            }
            else
            {
                polls = reader.writeFile(save);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No polls found! Basic poll file being created. 5 example polls loaded.");
                Console.ResetColor();
            }
        }

        public void OnUpdate()
        {
            if (update == 0)
            {
                if (UpdateChecker())
                    update++;
                else
                    update--;
            }
            else if (update < 0)
            {
                if ((DateTime.Now - lastupdate).TotalHours >= 3)
                {
                    if (UpdateChecker())
                        update = 1;
                    else
                        lastupdate = DateTime.Now;
                }
            }
        }

        public void OnGreetPlayer(int who, HandledEventArgs e)
        {
            lock (Players)
                Players.Add(new Player(who));
            if (TShock.Players[who].Group.Name.ToLower() == "superadmin")
                if (update > 0)
                {
                    TShock.Players[who].SendMessage("Update for Easy Vote available! Check log for download link.", Color.Yellow);
                    Log.Info(string.Format("NEW VERSION: {0}  |  Download here: {1}", versionFromUpdate, downloadFromUpdate));
                }
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
        public static void Poll(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                if (OpenPoll)
                {
                    args.Player.SendMessage("Current Poll: " + CurrentPoll, 30, 144, 255);
                    args.Player.SendMessage(FindPoll(CurrentPoll).Question, 30, 144, 255);
                    args.Player.SendMessage("Vote yes or no (/poll vote <yes/no>)", 135, 206, 255);
                    args.Player.SendMessage("------------------------------------------", Color.DarkGray);
                }
                args.Player.SendMessage("Syntax: /poll(s) <help/vote/start/getresults/reload>", 30, 144, 255);
                args.Player.SendMessage("- /poll(s) <help>: This displays this page.", 135, 206, 255);
                args.Player.SendMessage("- /poll(s) <vote> <yes/no>: This allows you to vote on the current poll (displayed by doing /poll vote).", 135, 206, 255);
                if (args.Player.Group.HasPermission("Poll"))
                {
                    args.Player.SendMessage("- /poll(s) <start>* <pollname>: This starts a poll based on its name (a poll list is displayed by doing /poll start).", 135, 206, 255);
                    args.Player.SendMessage("- /poll(s) <getresults>*: This closes the poll and gathers the results.", 135, 206, 255);
                    args.Player.SendMessage("- /poll(s) <reload>*: This reloads the Polls.json file.", 135, 206, 255);
                    args.Player.SendMessage("* These commands need the permission \"Poll\" to be used.", Color.Red);
                }
                args.Player.SendMessage("Note: /poll can be written as /polls, it does not make a difference in the command at all.", 30, 144, 255);
            }
            else switch (args.Parameters[0].ToLower())
                {
                    case "help":
                        if (OpenPoll)
                        {
                            args.Player.SendMessage("Current Poll: " + CurrentPoll, 30, 144, 255);
                            args.Player.SendMessage(FindPoll(CurrentPoll).Question, 30, 144, 255);
                            args.Player.SendMessage("Vote yes or no (/poll vote <yes/no>)", 135, 206, 255);
                            args.Player.SendMessage("------------------------------------------", Color.DarkGray);
                        }
                        args.Player.SendMessage("Syntax: /poll(s) <help/vote/start/getresults/reload>", 30, 144, 255);
                        args.Player.SendMessage("- /poll(s) <help>: This displays this page.", 135, 206, 255);
                        args.Player.SendMessage("- /poll(s) <vote> <yes/no>: This allows you to vote on the current poll (displayed by doing /poll vote).", 135, 206, 255);
                        if (args.Player.Group.HasPermission("Poll"))
                        {
                            args.Player.SendMessage("- /poll(s) <start>* <pollname>: This starts a poll based on its name (a poll list is displayed by doing /poll start).", 135, 206, 255);
                            args.Player.SendMessage("- /poll(s) <getresults>*: This closes the poll and gathers the results.", 135, 206, 255);
                            args.Player.SendMessage("- /poll(s) <reload>*: This reloads the Polls.json file.", 135, 206, 255);
                            args.Player.SendMessage("* These commands need the permission \"Poll\" to be used.", Color.Red);
                        }
                        args.Player.SendMessage("Note: /poll can be written as /polls, it does not make a difference in the command at all.", 30, 144, 255);
                        break;
                    case "vote":
                        if (args.Parameters.Count == 1)
                        {
                            if (OpenPoll)
                            {
                                args.Player.SendMessage("Current Poll: " + CurrentPoll, 30, 144, 255);
                                args.Player.SendMessage(FindPoll(CurrentPoll).Question, 30, 144, 255);
                                args.Player.SendMessage("Vote yes or no (/poll(s) vote <yes/no>)", 135, 206, 255);
                            }
                            else
                            {
                                args.Player.SendMessage("No polls available.", Color.Red);
                            }
                        }
                        else if (OpenPoll)
                        {
                            var ListedPlayer = Player.GetPlayerByName(args.Player.Name);
                            switch (args.Parameters[1].ToLower())
                            {
                                case "yes":
                                    args.Player.SendMessage("You voted yes.", 30, 144, 255);
                                    ListedPlayer.SetVote(Player.VoteResults.yes);
                                    break;
                                case "no":
                                    args.Player.SendMessage("You voted no.", 30, 144, 255);
                                    ListedPlayer.SetVote(Player.VoteResults.no);
                                    break;
                                default:
                                    args.Player.SendMessage(string.Format("Invalid vote (your vote \"{0}\" did not match the possible votes: yes or no)", args.Parameters[1]), Color.Red);
                                    break;
                            }
                        }
                        else
                        {
                            args.Player.SendMessage("No polls available.", Color.Red);
                        }
                        break;
                    case "start":
                        if (args.Player.Group.HasPermission("Poll"))
                        {
                            if (!OpenPoll)
                            {
                                if (args.Parameters.Count == 1)
                                {
                                    args.Player.SendMessage("Syntax: /poll(s) startpoll <pollname>", 30, 144, 255);
                                    args.Player.SendMessage("----- List of Polls -----", 135, 206, 255);
                                    List<string> pollnames = polls.getList();
                                    if (pollnames.Count > 0)
                                    {
                                        foreach (string poll in pollnames)
                                        {
                                            args.Player.SendMessage(poll, 30, 144, 255);
                                        }
                                    }
                                    else
                                    {
                                        args.Player.SendMessage("There are no polls", Color.Red);
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        string name = args.Parameters[1].ToLower();
                                        if (FindPoll(name).PollName != null)
                                        {
                                            args.Player.SendMessage(FindPoll(name).PollName + " has been started!", 135, 206, 255);
                                            OpenPoll = true;
                                            CurrentPoll = FindPoll(name).PollName;
                                            foreach (Player player in Players)
                                            {
                                                player.TSPlayer.SendMessage("New poll (" + CurrentPoll + ") has been started! Type /poll(s) vote for more info!", 30, 144, 255);
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        args.Player.SendMessage("Cannot find the specified poll \"" + args.Parameters[1] + "\"", Color.Red);
                                        args.Player.SendMessage("Type /poll(s) startpoll to find the right name", Color.Red);
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine(e.Message);
                                        Console.ResetColor();
                                    }
                                }
                            }
                            else
                            {
                                args.Player.SendMessage("There is already a poll open!", Color.Red);
                            }
                        }
                        else
                        {
                            args.Player.SendMessage("You do not have access to that command.", Color.Red);
                        }
                        break;
                    case "getresults":
                        if (args.Player.Group.HasPermission("Poll"))
                        {
                            int i = 0;
                            int x = 0;
                            var currentpoll = FindPoll(CurrentPoll);
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
                            args.Player.SendMessage(string.Format("{0} player(s) voted yes and {1} player(s) voted no.", i, x), 135, 206, 255);
                            if (i > x)
                            {
                                switch (currentpoll.Time.ToLower())
                                {
                                    case "day":
                                        Commands.HandleCommand(TSServerPlayer.Server, "/time day");
                                        break;
                                    case "night":
                                        Commands.HandleCommand(TSServerPlayer.Server, "/time night");
                                        break;
                                }
                                currentpoll.spawnMonsters(TSServerPlayer.Server);
                                currentpoll.handleCommands(TSServerPlayer.Server);
                            }
                            foreach (Player player in Players)
                            {
                                player.SetVote(Player.VoteResults.novote);
                                player.TSPlayer.SendMessage("Poll \"" + CurrentPoll + "\" has been closed.", 30, 144, 255);
                            }
                            if (currentpoll.PublicResults)
                            {
                                foreach (Player player in Players)
                                {
                                    player.TSPlayer.SendMessage(string.Format("Results for the poll: {0} player(s) voted yes and {1} player(s) voted no.", i, x), 30, 144, 255);
                                }
                            }
                            OpenPoll = false;
                            CurrentPoll = null;
                        }
                        else
                        {
                            args.Player.SendMessage("You do not have access to that command.", Color.Red);
                        }
                        break;
                    case "reload":
                        if (args.Player.Group.HasPermission("Poll"))
                        {
                            try
                            {
                                PollReader reader = new PollReader();
                                if (File.Exists(save))
                                {
                                    polls = reader.readFile(save);
                                    args.Player.SendMessage(polls.polls.Count + " polls have been reloaded.", 135, 206, 255);
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine(polls.polls.Count + " polls have been reloaded.");
                                    Console.ResetColor();
                                }
                            }
                            catch (Exception e)
                            {
                                args.Player.SendMessage("Error in Polls.json file! Check log for more details.", Color.Red);
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(e.Message);
                                Console.ResetColor();
                                Log.Error("--------- Config Exception in EasyVote Config file (Polls.json) ---------");
                                Log.Error(e.Message);
                            }
                        }
                        else
                        {
                            args.Player.SendMessage("You do not have access to that command.", Color.Red);
                        }
                        break;
                }
        }
    #endregion
        public bool UpdateChecker()
        {
            string raw;
            try
            {
                raw = new WebClient().DownloadString("https://github.com/Spectrewiz/EasyVote/raw/master/README.txt");
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return false;
            }
            string[] readme = raw.Split('\n');
            string[] download = readme[readme.Length - 1].Split('-');
            Version version;
            if (!Version.TryParse(readme[0], out version)) return false;
            if (Version.CompareTo(version) >= 0) return false;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("New Easy Vote version: " + readme[0].Trim());
            Console.WriteLine("Download here: " + download[1].Trim());
            Console.ResetColor();
            Log.Info(string.Format("NEW VERSION: {0}  |  Download here: {1}", readme[0].Trim(), download[1].Trim()));
            downloadFromUpdate = download[1].Trim();
            versionFromUpdate = readme[0].Trim();
            return true;
        }
    }
}