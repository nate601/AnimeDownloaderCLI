using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

public partial class HorribleSubsPacklist
{
    public class ShowEntry
    {

        public static int GetTotalNumberOfEpisodes(List<ShowEntry> shows)
        {
            int episodeNumbers = 0;
            foreach (var showEntry in shows)
            {
                if (int.Parse(showEntry.episodeNumber) > episodeNumbers)
                {
                    episodeNumbers = int.Parse(showEntry.episodeNumber);
                }
            }

            return episodeNumbers;
        }
        public static Dictionary<string, Quality[]> GetShowQualities(List<ShowEntry> shows)
        {
            var dic = new Dictionary<string, Quality[]>();
            var names = GetShowNames(shows);
            foreach (var name in names)
            {
                List<Quality> qualities = new List<Quality>();

                foreach (var show in shows.Where((x) => x.PrettyTitle() == name))
                {
                    if (!qualities.Contains(show.episodeQuality))
                        qualities.Add(show.episodeQuality);
                }
                dic.Add(name, qualities.ToArray());
            }
            return dic;
        }

        public static List<string> GetShowNames(List<ShowEntry> shows)
        {
            List<string> showNames = new List<string>();
            foreach (var show in shows)
            {
                if (!showNames.Contains(show.PrettyTitle()))
                {
                    showNames.Add(show.PrettyTitle());
                }
            }
            return showNames;

        }
        public string botName;
        public string packNumber;
        public string sizeInMb;
        public string Title;
        public string episodeNumber;
        public Quality episodeQuality;

        public ShowEntry(string lineEntry)
        {

            Regex nameMatcher = new Regex("(?:b:\"|f:\")([^\"]*)"); //Match 0 group 1 = bot name
            var nameMatches = nameMatcher.Match(lineEntry);
            botName = nameMatches.Groups[1].Value;
            Title = nameMatches.NextMatch().Groups[1].Value;
            Regex packNumberMatcher = new Regex(@"n:(\d*)");
            var packnumberMatches = packNumberMatcher.Match(lineEntry);
            packNumber = packnumberMatches.Groups[1].ToString();
            Regex sizeNumberMatcher = new Regex(@"s:(\d*)");
            var sizeNumberMatches = sizeNumberMatcher.Match(lineEntry);
            sizeInMb = sizeNumberMatches.Groups[1].Value;
            Regex episodeNumberMatcher = new Regex(@"(?:- )(\d*)");
            var regexNumberMatches = episodeNumberMatcher.Match(Title);
            episodeNumber = regexNumberMatches.Groups[1].Value;

            if (lineEntry.Contains("1080p"))
            {
                episodeQuality = Quality.TEN_EIGHTY_P;
            }
            else if (lineEntry.Contains("720p"))
            {
                episodeQuality = Quality.SEVEN_TWENTY_P;
            }
            else
            {
                episodeQuality = Quality.STANDARD_DEFINITION;
            }
        }

        internal static List<ShowEntry> ShakeByShowQuality(Quality chosenQuality, List<ShowEntry> shows)
        {
            List<ShowEntry> retVal = new List<ShowEntry>();
            retVal = shows.Where((x) => x.episodeQuality == chosenQuality).ToList();
            return retVal;
        }

        public bool Verify()
        {
            return botName != "" && packNumber != "" && sizeInMb != "" && Title != "" && episodeNumber != "";
        }
        public string PrettyTitle()
        {
            Regex titlePrettifier = new Regex("(.+)(?: -)");
            return titlePrettifier.Match(Title).Groups[1].Value.Split(']')[1].TrimStart(' ');
        }

        [Conditional("DEBUG")]
        public void PrettyPrint()
        {
            Console.WriteLine(PrettyTitle());
            Console.WriteLine("├─" + episodeNumber);
            Console.WriteLine("├─" + botName);
            Console.WriteLine("├─" + packNumber);
            Console.WriteLine("└─" + sizeInMb + " megabytes");
        }

        internal static List<ShowEntry> ShakeByShowName(string showNameChosen, List<ShowEntry> shows)
        {
            List<ShowEntry> retVal = new List<ShowEntry>();

            foreach (var item in shows)
            {
                if (item.PrettyTitle() == showNameChosen)
                    retVal.Add(item);
            }
            return retVal;
        }
    }

}