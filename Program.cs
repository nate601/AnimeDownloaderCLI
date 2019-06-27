using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AnimeDown
{
    partial class Program
    {
        // ? Consider changing back from Lazy initialization
        // ? Using lazy initialization prevents the program from
        // ? using resources connecting early, but now that user checking
        // ? is implemented this is slightly pointless
        private static readonly Lazy<DownloadHandler> handler = new Lazy<DownloadHandler>();
        private static bool hasBegunDownload = false;
        static async Task Main()
        {
            Console.WriteLine("Anime Downloader");
            Console.Write("Anime Title:");
            var animeTitle = Console.ReadLine();
            var resolutionMap = new Dictionary<ConsoleKey, HorribleSubsPacklist.Quality>() {
                { ConsoleKey.D0, HorribleSubsPacklist.Quality.TEN_EIGHTY_P },
                { ConsoleKey.D1, HorribleSubsPacklist.Quality.SEVEN_TWENTY_P },
                { ConsoleKey.D2, HorribleSubsPacklist.Quality.STANDARD_DEFINITION },
                { ConsoleKey.NumPad0, HorribleSubsPacklist.Quality.TEN_EIGHTY_P },
                { ConsoleKey.NumPad1, HorribleSubsPacklist.Quality.SEVEN_TWENTY_P },
                { ConsoleKey.NumPad2, HorribleSubsPacklist.Quality.STANDARD_DEFINITION },
            };
            var resolutionEnum = ReadKeyMap("Resolution:\n0:1080p\n1:720p\n2:SD\n", resolutionMap);


            HorribleSubsPacklist horrible = new HorribleSubsPacklist();
            var shows = horrible.GetShow(animeTitle, resolutionEnum);
            var showNamePossibilities = HorribleSubsPacklist.ShowEntry.GetShowNames(shows);
            Console.WriteLine($"There are {showNamePossibilities.Count} results for {animeTitle} on horriblesubs.");
            for (int i = 0; i < showNamePossibilities.Count; i++)
            {
                string item = (string)showNamePossibilities[i];
                Console.WriteLine($"{i} : {item}");
            }
            string showNameChosen = showNamePossibilities[ReadNumber("Which one would you like to download?", showNamePossibilities.Count - 1)];
            List<HorribleSubsPacklist.ShowEntry> showNameShaken = HorribleSubsPacklist.ShowEntry.ShakeByShowName(showNameChosen, shows);

            var DownloadMethodMap = new Dictionary<ConsoleKey, Action<List<HorribleSubsPacklist.ShowEntry>>>(){
                {ConsoleKey.A, DownloadAllPrompt},
                {ConsoleKey.S, DownloadSomePrompt},
                {ConsoleKey.O, DownloadOnePrompt},
            };
            ReadKeyMap(
                $"There are {HorribleSubsPacklist.ShowEntry.GetTotalNumberOfEpisodes(showNameShaken)} episodes of {HorribleSubsPacklist.ShowEntry.GetShowNames(showNameShaken).First()} out." + "\n" +
                "Would you like to download (a)ll of them, (s)ome of them, or (o)ne of them?\n",
                DownloadMethodMap)(showNameShaken);
            Console.ReadLine();
            await Task.Delay(-1);
        }
        private static string ChooseBotPrompt(List<HorribleSubsPacklist.ShowEntry> shows)
        {
            List<string> botNames = new List<string>();
            foreach (var showEntry in shows)
            {
                if (!botNames.Contains(showEntry.botName)) { botNames.Add(showEntry.botName); }
            }
            for (int i = 0; i < botNames.Count; i++)
            {
                bool v = handler.Value.IsUserPresent(botNames[i]);
                Console.Write($"{i} : ");
                WriteInColor($"{botNames[i]} [{(v ? "ON" : "OFF")}LINE]", v ? ConsoleColor.Green : ConsoleColor.Red);
                Console.Write("\n");
            }
            var botNumber = ReadNumber("Which bot would you like to download from?", botNames.Count - 1);

            return botNames[botNumber];
        }

        private static void DownloadAllPrompt(List<HorribleSubsPacklist.ShowEntry> downloadCandidates)
        {
            List<HorribleSubsPacklist.ShowEntry> showsToDownload = new List<HorribleSubsPacklist.ShowEntry>();
            Console.WriteLine($"Downloading all {HorribleSubsPacklist.ShowEntry.GetTotalNumberOfEpisodes(downloadCandidates)} episodes!");
            string botName = ChooseBotPrompt(downloadCandidates);
            for (int i = 0; i <= HorribleSubsPacklist.ShowEntry.GetTotalNumberOfEpisodes(downloadCandidates); i++)
            {
                foreach (var show in downloadCandidates)
                {
                    if (int.Parse(show.episodeNumber) == i)
                    {
                        if (show.botName != botName)
                            continue;
                        if (!showsToDownload.Where((entry => int.Parse(entry.episodeNumber) == i)).Any())
                        {
                            showsToDownload.Add(show);
                        }
                    }
                }
            }
            Download(showsToDownload);
        }
        private static void DownloadSomePrompt(List<HorribleSubsPacklist.ShowEntry> downloadCandidates)
        {
        rangePrompt: var episodeRangeBegin =
           ReadNumber(
               $"Which episode would you like the range to begin with?", 1, HorribleSubsPacklist.ShowEntry.GetTotalNumberOfEpisodes(downloadCandidates));

            var episodeRangeEnd =
                ReadNumber(
                    $"Which episode would you like the range to end with?", episodeRangeBegin, HorribleSubsPacklist.ShowEntry.GetTotalNumberOfEpisodes(downloadCandidates));
            if (episodeRangeEnd <= episodeRangeBegin)
            {
                Console.WriteLine("Episode range end must be greater than the beginning of the episode range!");
                goto rangePrompt;
            }
            List<HorribleSubsPacklist.ShowEntry> showsToDownload = new List<HorribleSubsPacklist.ShowEntry>();
            var botName = ChooseBotPrompt(downloadCandidates);
            for (int i = episodeRangeBegin; i <= episodeRangeEnd; i++)
            {
                foreach (var show in downloadCandidates)
                {
                    if (show.botName != botName)
                        continue;
                    if (int.Parse(show.episodeNumber) == i)
                    {
                        if (!showsToDownload.Where((entry => int.Parse(entry.episodeNumber) == i)).Any())
                        {
                            showsToDownload.Add(show);
                        }
                    }
                }
            }
            Download(showsToDownload);
        }
        private static void DownloadOnePrompt(List<HorribleSubsPacklist.ShowEntry> downloadCandidates)
        {
            var episodeNumber = ReadNumber("Which episode would you like to download?", 1, HorribleSubsPacklist.ShowEntry.GetTotalNumberOfEpisodes(downloadCandidates));

            List<HorribleSubsPacklist.ShowEntry> showsToDownload = new List<HorribleSubsPacklist.ShowEntry>();
            foreach (var show in downloadCandidates)
            {
                if (int.Parse(show.episodeNumber) == episodeNumber)
                {
                    showsToDownload.Add(show);
                }

            }
            var botName = ChooseBotPrompt(showsToDownload);
            var downloadShow = showsToDownload.First((b) => b.botName == botName);
            Download(downloadShow);

        }

        private static void Download(HorribleSubsPacklist.ShowEntry entry)
        {
            if (!hasBegunDownload)
            {
                handler.Value.SetDownloadDirectory(Path.Combine(Directory.GetCurrentDirectory(), entry.PrettyTitle()));

                hasBegunDownload = true;
            }

            DownloadHandler.DownloadPair pair = new DownloadHandler.DownloadPair(
                entry.botName,
                int.Parse(entry.packNumber),
                $"{ entry.PrettyTitle() } Episode { entry.episodeNumber }"
            );
            handler.Value.Download(pair);
        }

        private static void Download(List<HorribleSubsPacklist.ShowEntry> entries)
        {
            foreach (var showEntry in entries)
            {
                Download(showEntry);
            }
        }


    }
}
