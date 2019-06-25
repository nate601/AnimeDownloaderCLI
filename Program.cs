using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SimpleIRCLib;

namespace AnimeDown {
    class Program {
        private static readonly DownloadHandler handler = new DownloadHandler ();
        private static bool hasBegunDownload = false;
        static void Main () {

            Console.WriteLine ("Anime Downloader");
            Console.Write ("Anime Title:");
            var animeTitle = Console.ReadLine ();
            resolutionPrompt:
                Console.WriteLine ("Resolution:");
            Console.WriteLine ("0: 1080p");
            Console.WriteLine ("1: 720p");
            Console.WriteLine ("2: SD");
            var response = Console.ReadKey (true);
            HorribleSubsPacklist.Quality resolutionEnum;
            switch (response.Key) {
                case ConsoleKey.D0:
                case ConsoleKey.NumPad0:
                    resolutionEnum = HorribleSubsPacklist.Quality.TEN_EIGHTY_P;
                    break;
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    resolutionEnum = HorribleSubsPacklist.Quality.SEVEN_TWENTY_P;
                    break;
                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    resolutionEnum = HorribleSubsPacklist.Quality.STANDARD_DEFINITION;
                    break;
                default:
                    goto resolutionPrompt;
            }

            Console.WriteLine ($"Searching for {animeTitle} on horribleSubs");
            HorribleSubsPacklist horrible = new HorribleSubsPacklist ();
            var shows = horrible.GetShow (animeTitle, resolutionEnum);

            var showNamePossibilities = HorribleSubsPacklist.ShowEntry.GetShowNames (shows);

            Console.WriteLine ($"There are {showNamePossibilities.Count} results for {animeTitle} on horriblesubs.");
            Console.WriteLine ("Which one would you like to download?");
            for (int i = 0; i < showNamePossibilities.Count; i++) {
                string item = (string) showNamePossibilities[i];
                Console.WriteLine ($"{i} : {item}");
            }
            string showNameChosen = showNamePossibilities[ReadNumber ("", showNamePossibilities.Count)];

            List<HorribleSubsPacklist.ShowEntry> showNameShaken = HorribleSubsPacklist.ShowEntry.ShakeByShowName (showNameChosen, shows);

            prompt:
                Console.WriteLine ($"There are {HorribleSubsPacklist.ShowEntry.GetTotalNumberOfEpisodes(showNameShaken)} episodes of {HorribleSubsPacklist.ShowEntry.GetShowNames(showNameShaken).First()} out.");
            Console.WriteLine ("Would you like to download (a)ll of them, (s)ome of them, or (o)ne of them?");
            response = Console.ReadKey (true);
            switch (response.Key) {
                case ConsoleKey.A:
                    DownloadAllPrompt (showNameShaken);
                    break;
                case ConsoleKey.S:
                    DownloadSomePrompt (showNameShaken);

                    break;
                case ConsoleKey.O:
                    DownloadOnePrompt (showNameShaken);

                    break;
                default:
                    goto prompt;
            }

            Console.ReadKey ();

        }

        public static void DownloadAllPrompt (List<HorribleSubsPacklist.ShowEntry> shows) {
            Console.WriteLine ($"Downloading all {HorribleSubsPacklist.ShowEntry.GetTotalNumberOfEpisodes(shows)} episodes!");
            List<HorribleSubsPacklist.ShowEntry> showOptions = new List<HorribleSubsPacklist.ShowEntry> ();

            List<string> botNames = new List<string> ();
            foreach (var showEntry in shows) {
                if (!botNames.Contains (showEntry.botName))
                    botNames.Add (showEntry.botName);
            }

            for (int botIndex = 0; botIndex < botNames.Count; botIndex++) {
                Console.WriteLine (botIndex + " : " + botNames[botIndex]);
            }

            var botNumber = ReadNumber ("Which bot would you like to download from?", botNames.Count);

            for (int i = 0; i <= HorribleSubsPacklist.ShowEntry.GetTotalNumberOfEpisodes (shows); i++) {
                foreach (var show in shows) {
                    if (int.Parse (show.episodeNumber) == i) {
                        if (show.botName != botNames[botNumber])
                            continue;
                        if (!showOptions.Where ((entry => int.Parse (entry.episodeNumber) == i)).Any ()) {
                            showOptions.Add (show);
                        }
                    }

                }
            }
            Download (showOptions);
        }
        public static void DownloadSomePrompt (List<HorribleSubsPacklist.ShowEntry> shows) {
            rangePrompt : var episodeRangeBegin =
                ReadNumber (
                    $"Which episode would you like the range to begin with? (1-{HorribleSubsPacklist.ShowEntry.GetTotalNumberOfEpisodes(shows)})", HorribleSubsPacklist.ShowEntry.GetTotalNumberOfEpisodes (shows));

            var episodeRangeEnd =
                ReadNumber (
                    $"Which episode would you like the range to end with? ({episodeRangeBegin}-{HorribleSubsPacklist.ShowEntry.GetTotalNumberOfEpisodes(shows)})", episodeRangeBegin, HorribleSubsPacklist.ShowEntry.GetTotalNumberOfEpisodes (shows));

            if (episodeRangeEnd <= episodeRangeBegin) {
                Console.WriteLine ("Episode range end must be greater than the beginning of the episode range!");
                goto rangePrompt;
            }
            List<HorribleSubsPacklist.ShowEntry> showOptions = new List<HorribleSubsPacklist.ShowEntry> ();

            List<string> botNames = new List<string> ();
            foreach (var showEntry in shows) {
                if (!botNames.Contains (showEntry.botName))
                    botNames.Add (showEntry.botName);
            }

            for (int botIndex = 0; botIndex < botNames.Count; botIndex++) {
                Console.WriteLine (botIndex + " : " + botNames[botIndex]);
            }

            var botNumber = ReadNumber ("Which bot would you like to download from?", botNames.Count);

            for (int i = episodeRangeBegin; i <= episodeRangeEnd; i++) {
                foreach (var show in shows) {
                    if (show.botName != botNames[botNumber])
                        continue;
                    if (int.Parse (show.episodeNumber) == i) {
                        if (!showOptions.Where ((entry => int.Parse (entry.episodeNumber) == i)).Any ()) {
                            showOptions.Add (show);
                        }
                    }
                }
            }
            Download (showOptions);
        }
        public static void DownloadOnePrompt (List<HorribleSubsPacklist.ShowEntry> shows) {
            var episodeNumber = ReadNumber ("Which episode would you like to download? (1-" +
                HorribleSubsPacklist.ShowEntry.GetTotalNumberOfEpisodes (shows) + ")", HorribleSubsPacklist.ShowEntry.GetTotalNumberOfEpisodes (shows));

            List<HorribleSubsPacklist.ShowEntry> showOptions = new List<HorribleSubsPacklist.ShowEntry> ();
            foreach (var show in shows) {
                if (int.Parse (show.episodeNumber) == episodeNumber) {
                    showOptions.Add (show);
                }

            }
            for (var index = 0; index < showOptions.Count; index++) {
                var showOption = showOptions[index];
                Console.WriteLine ($"{index} : {showOption.botName}");
            }
            var botNumber = ReadNumber ("Which bot would you like to download from?", showOptions.Count);
            Download (showOptions[botNumber]);

        }

        public static void Download (HorribleSubsPacklist.ShowEntry entry) {
            if (!hasBegunDownload) {
                handler.SetDownloadDirectory (Path.Combine (Directory.GetCurrentDirectory (), entry.PrettyTitle ()));

                hasBegunDownload = true;
            }

            handler.Download (entry.botName, int.Parse (entry.packNumber));
        }

        public static void Download (List<HorribleSubsPacklist.ShowEntry> entries) {

            foreach (var showEntry in entries) {
                Download (showEntry);
            }
        }
        /// <summary>
        /// Prompts the operator for a number, and repeats the request if the number is invalid
        /// </summary>
        /// <param name="prompt">The message with which to prompt the operator</param>
        /// <param name="min">The minimum number that the operator must enter (inclusive)</param>
        /// <param name="max">The maximum number that the operator must enter (exclusive)</param>
        /// <returns></returns>
        public static int ReadNumber (string prompt, int min, int max) {
            int result;
            while (true) {
                Console.WriteLine (prompt);
                if (int.TryParse (Console.ReadLine (), out result) && result >= min && result < max)
                    break;
            }

            return result;
        }
        /// <summary>
        /// Prompts the operator for a number, and repeats the request if the number is invalid
        /// </summary>
        /// <param name="prompt">The message with which to prompt the operator</param>
        /// <param name="max">The maximum number that the operator must enter (exclusive)</param>
        /// <returns></returns>
        public static int ReadNumber (string prompt, int max) => ReadNumber (prompt, 0, max);
    }
}