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
        static void Main()
        {
            Console.WriteLine("Anime Downloader");
            Console.Write("Anime Title:");
            var animeTitle = Console.ReadLine();


            HorribleSubsPacklist horrible = new HorribleSubsPacklist();

            var shows = horrible.GetShow(animeTitle);
            var showNamePossibilities = HorribleSubsPacklist.ShowEntry.GetShowNames(shows);
            var getShowQualities = HorribleSubsPacklist.ShowEntry.GetShowQualities(shows);
            var sortedShows = HorribleSubsPacklist.ShowEntry.GetShowsSeperated(shows);

            PrintShowNamesTable(sortedShows, showNamePossibilities, animeTitle);

            string showNameChosen = showNamePossibilities[ReadNumber("Which one would you like to download?", showNamePossibilities.Count - 1)];
            List<HorribleSubsPacklist.ShowEntry> showsShakenByName = HorribleSubsPacklist.ShowEntry.ShakeByShowName(showNameChosen, shows);
            List<HorribleSubsPacklist.Quality> possibleQualities = getShowQualities[showNameChosen].ToList();
            possibleQualities.Sort();


            HorribleSubsPacklist.Quality chosenQuality = ChooseQualityPrompt(possibleQualities.ToArray());
            List<HorribleSubsPacklist.ShowEntry> showsShakenByQuality = HorribleSubsPacklist.ShowEntry.ShakeByShowQuality(chosenQuality, showsShakenByName);


            var DownloadMethodMap = new Dictionary<ConsoleKey, Action<List<HorribleSubsPacklist.ShowEntry>>>(){
                {ConsoleKey.A, DownloadAllPrompt},
                {ConsoleKey.S, DownloadSomePrompt},
                {ConsoleKey.O, DownloadOnePrompt},
            };
            ReadKeyMap(
                $"There are {HorribleSubsPacklist.ShowEntry.GetTotalNumberOfEpisodes(showsShakenByQuality)} episodes of {HorribleSubsPacklist.ShowEntry.GetShowNames(showsShakenByQuality).First()} out." + "\n" +
                "Would you like to download (a)ll of them, (s)ome of them, or (o)ne of them?\n",
                DownloadMethodMap)(showsShakenByQuality);
            Console.ReadLine();
        }

        private static void PrintShowNamesTable(List<List<HorribleSubsPacklist.ShowEntry>> sortedShows, List<string> showNamePossibilities, string animeTitle)
        {
            Console.Clear();
            Console.WriteLine($"There are {showNamePossibilities.Count} results for \"{animeTitle}\" on horriblesubs.\n");

            System.Console.WriteLine($"{"Index",-5} : {"Show Title",-45} {"Number of Episodes",-18} {"Quality",-8}");
            for (int i = 0; i < sortedShows.Count; i++)
            {
                List<HorribleSubsPacklist.ShowEntry> item = sortedShows[i];
                string showName = HorribleSubsPacklist.ShowEntry.GetShowNames(item).First();
                var qualities = HorribleSubsPacklist.ShowEntry.GetShowQualities(item)[showName];
                System.Console.Write($"{i,5} : {showName,-45} {HorribleSubsPacklist.ShowEntry.GetTotalNumberOfEpisodes(item),-18} ");
                WriteInColor($"[1080p]",
                    qualities.Contains(HorribleSubsPacklist.Quality.TEN_EIGHTY_P) ? ConsoleColor.Green : ConsoleColor.Red);
                WriteInColor($"[720p]",
                    qualities.Contains(HorribleSubsPacklist.Quality.SEVEN_TWENTY_P) ? ConsoleColor.Green : ConsoleColor.Red);
                WriteInColor($"[SD]",
                    qualities.Contains(HorribleSubsPacklist.Quality.STANDARD_DEFINITION) ? ConsoleColor.Green : ConsoleColor.Red);
                Console.Write("\n");
            }
            Console.WriteLine();
        }

        private static HorribleSubsPacklist.Quality ChooseQualityPrompt(HorribleSubsPacklist.Quality[] possibleQualities)
        {
            var qualityPrettyMap = new Dictionary<HorribleSubsPacklist.Quality, string>(){
              {HorribleSubsPacklist.Quality.TEN_EIGHTY_P, "1080p"},
              {HorribleSubsPacklist.Quality.SEVEN_TWENTY_P, "720p"},
              {HorribleSubsPacklist.Quality.STANDARD_DEFINITION, "SD"},
            };
            for (int i = 0; i < possibleQualities.Length; i++)
            {
                System.Console.WriteLine($"{i} : {qualityPrettyMap[possibleQualities[i]]}");
            }
            var qualNum = ReadNumber("What quality would you like to download?", possibleQualities.Length - 1);
            return possibleQualities[qualNum];
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
            int episodeRangeBegin;
            int episodeRangeEnd;
            while (true)
            {
                episodeRangeBegin =
                    ReadNumber(
                        $"Which episode would you like the range to begin with?",
                        1,
                        HorribleSubsPacklist.ShowEntry.GetTotalNumberOfEpisodes(downloadCandidates)
                    );
                episodeRangeEnd =
                    ReadNumber(
                        $"Which episode would you like the range to end with?",
                        episodeRangeBegin,
                        HorribleSubsPacklist.ShowEntry.GetTotalNumberOfEpisodes(downloadCandidates)
                    );
                if (episodeRangeEnd <= episodeRangeBegin)
                {
                    Console.WriteLine("Episode range end must be greater than the beginning of the episode range!");
                }
                else { break; }
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
