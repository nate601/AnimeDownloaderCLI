using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SimpleIRCLib;

namespace AnimeDown
{
    class Program
    {
        private static bool verboseLogging = false;
        private static DownloadHandler handler = new DownloadHandler();

        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "-l")
                verboseLogging = true;
            

            Console.WriteLine("Anime Downloader");
            Console.Write("Anime Title:");
            var animeTitle = Console.ReadLine();
            resolutionPrompt:
            Console.WriteLine("Resolution:");
            Console.WriteLine("0: 1080p");
            Console.WriteLine("1: 720p");
            Console.WriteLine("2: SD");
            var resolution = ReadNumber("");
            var resolutionEnum = HorribleSubsPacklist.Quality.TEN_EIGHTY_P;
            switch (resolution)
            {
                case 0:
                    resolutionEnum = HorribleSubsPacklist.Quality.TEN_EIGHTY_P;
                    break;
                case 1:
                    resolutionEnum = HorribleSubsPacklist.Quality.SEVEN_TWENTY_P;
                    break;
                case 2:
                    resolutionEnum = HorribleSubsPacklist.Quality.STANDARD_DEFINITION;
                    break;
                default:
                    goto resolutionPrompt;
            }



            Console.WriteLine($"Searching for {animeTitle} on horribleSubs");
            HorribleSubsPacklist horrible = new HorribleSubsPacklist();
            var shows = horrible.GetShow(animeTitle, resolutionEnum);

            //foreach (var showEntry in shows)
            //{
            //    showEntry.PrettyPrint();
            //}

            prompt:
            Console.WriteLine($"There are {HorribleSubsPacklist.ShowEntry.GetTotalNumberOfEpisodes(shows)} episodes of {HorribleSubsPacklist.ShowEntry.GetShowName(shows)} out.");
            Console.WriteLine("Would you like to download (a)ll of them, (s)ome of them, or (o)ne of them?");
            var response = Console.ReadKey(true);
            switch (response.Key)
            {
                case ConsoleKey.A:
                    DownloadAllPrompt(shows);
                    break;
                case ConsoleKey.S:
                    DownloadSomePrompt(shows);

                    break;
                case ConsoleKey.O:
                    DownloadOnePrompt(shows);

                    break;
                default:
                    goto prompt;
            }

            Console.ReadKey();
            

        }

        public static void DownloadAllPrompt(List<HorribleSubsPacklist.ShowEntry> shows)
        {
            Console.WriteLine($"Downloading all {HorribleSubsPacklist.ShowEntry.GetTotalNumberOfEpisodes(shows)} episodes!");
            List<HorribleSubsPacklist.ShowEntry> showOptions = new List<HorribleSubsPacklist.ShowEntry>();

            List<string> botNames = new List<string>();
            foreach (var showEntry in shows)
            {
                if(!botNames.Contains(showEntry.botName))
                    botNames.Add(showEntry.botName);
            }

            for (int botIndex = 0; botIndex < botNames.Count; botIndex++)
            {
                Console.WriteLine(botIndex + " : " + botNames[botIndex]);
            }

            var botNumber =  ReadNumber("Which bot would you like to download from?");

           
            


            for (int i = 0; i <=HorribleSubsPacklist.ShowEntry.GetTotalNumberOfEpisodes(shows); i++)
            {
                foreach (var show in shows)
                {
                    if (int.Parse(show.episodeNumber) == i)
                    {
                        if (show.botName != botNames[botNumber])
                            continue;
                        if (!showOptions.Where((entry => int.Parse(entry.episodeNumber) == i)).Any())
                        {
                            showOptions.Add(show);
                        }
                    }

                }
            }
            Download(showOptions);
        }
        public static void DownloadSomePrompt(List<HorribleSubsPacklist.ShowEntry> shows)
        {
            rangePrompt:
            var episodeRangeBegin =
                ReadNumber(
                    $"Which episode would you like the range to begin with? (1-{HorribleSubsPacklist.ShowEntry.GetTotalNumberOfEpisodes(shows)})");

            var episodeRangeEnd =
                ReadNumber(
                    $"Which episode would you like the range to end with? ({episodeRangeBegin}-{HorribleSubsPacklist.ShowEntry.GetTotalNumberOfEpisodes(shows)})");

            if (episodeRangeEnd <= episodeRangeBegin)
            {
                Console.WriteLine("Episode range end must be greater than the beginning of the episode range!");
                goto rangePrompt;
            }
            List<HorribleSubsPacklist.ShowEntry> showOptions = new List<HorribleSubsPacklist.ShowEntry>();



            List<string> botNames = new List<string>();
            foreach (var showEntry in shows)
            {
                if (!botNames.Contains(showEntry.botName))
                    botNames.Add(showEntry.botName);
            }

            for (int botIndex = 0; botIndex < botNames.Count; botIndex++)
            {
                Console.WriteLine(botIndex + " : " + botNames[botIndex]);
            }

            var botNumber = ReadNumber("Which bot would you like to download from?");


            for (int i = episodeRangeBegin; i <= episodeRangeEnd; i++)
            {
                foreach (var show in shows)
                {
                    if (show.botName != botNames[botNumber])
                        continue;
                    if (int.Parse(show.episodeNumber) == i)
                    {
                        if (!showOptions.Where((entry => int.Parse(entry.episodeNumber) == i)).Any())
                        {
                            showOptions.Add(show);
                        }
                    }
                }
            }
            Download(showOptions);
            


        }
        public static void DownloadOnePrompt(List<HorribleSubsPacklist.ShowEntry> shows)
        {
            var episodeNumber = ReadNumber("Which episode would you like to download? (1-" +
                                           HorribleSubsPacklist.ShowEntry.GetTotalNumberOfEpisodes(shows) + ")");

            List<HorribleSubsPacklist.ShowEntry> showOptions = new List<HorribleSubsPacklist.ShowEntry>();
            foreach (var show in shows)
            {
                if (int.Parse(show.episodeNumber) == episodeNumber)
                {
                    showOptions.Add(show);
                }

            }
            for (var index = 0; index < showOptions.Count; index++)
            {
                var showOption = showOptions[index];
                Console.WriteLine($"{index} : {showOption.botName}");
            }
            var botNumber = ReadNumber("Which bot would you like to download from?");
            Download(showOptions[botNumber]);



        }

        public static void Download(HorribleSubsPacklist.ShowEntry entry)
        {
            handler.Download(entry.botName, int.Parse(entry.packNumber));
        }

        public static void Download(List<HorribleSubsPacklist.ShowEntry> entries)
        {
            
            foreach (var showEntry in entries)
            {
                Download(showEntry);
            }
        }



        public static int ReadNumber(string prompt)
        {
            int result;
            while (true)
            {
                Console.WriteLine(prompt);
                if (int.TryParse(Console.ReadLine(), out result))
                    break;
            }

            return result;
        }

        public static void Log(string log, bool verbose = false)
        {
           if(!verbose)
           {
               Console.WriteLine(log);
           }
           else
           {
               if(verboseLogging ||System.Diagnostics.Debugger.IsAttached)
                   Console.WriteLine("[VERB] : " +log);
           }
        }

    }
    
    
}
