using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

public class HorribleSubsPacklist {
    public List<ShowEntry> GetShow (string title, Quality quality) {
        string url = "https://xdcc.horriblesubs.info/search.php" + "?t=" + title + "";
        HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
        req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

        req.Referer = @"https://xdcc.horriblesubs.info/";
        req.Method = "GET";

        HttpWebResponse resp = (HttpWebResponse) req.GetResponse ();
        int responseCode = (int) resp.StatusCode;
        Console.WriteLine ("Sending 'GET' request to URL : " + url);
        Console.WriteLine ("Response Code : " + responseCode);

        string response;
        using (StreamReader input = new StreamReader (resp.GetResponseStream ())) {
            response = input.ReadToEnd ();
        }

        List<ShowEntry> shows = new List<ShowEntry> ();
        foreach (var s in response.Split (Environment.NewLine.ToCharArray ())) {
            switch (quality) {
                case Quality.TEN_EIGHTY_P:
                    if (s.Contains ("1080p")) {
                        shows.Add (new ShowEntry (s));
                    }
                    break;
                case Quality.SEVEN_TWENTY_P:
                    if (s.Contains ("720p")) {
                        shows.Add (new ShowEntry (s));
                    }
                    break;
                case Quality.STANDARD_DEFINITION:
                    if (!s.Contains ("1080p") && !s.Contains ("720p"))
                        shows.Add (new ShowEntry (s));
                    break;
            }
        }

        var showVerified = new List<ShowEntry> ();
        foreach (var showEntry in shows) {
            if (showEntry.Verify ())
                showVerified.Add (showEntry);
        }

        return showVerified;

    }

    public class ShowEntry {

        public static int GetTotalNumberOfEpisodes (List<ShowEntry> shows) {
            int episodeNumbers = 0;
            foreach (var showEntry in shows) {
                if (int.Parse (showEntry.episodeNumber) > episodeNumbers) {
                    episodeNumbers = int.Parse (showEntry.episodeNumber);
                }
            }

            return episodeNumbers;
        }

        public static string GetShowName (List<ShowEntry> shows) {
            return shows.First ().PrettyTitle ();
        }
        public string botName;
        public string packNumber;
        public string sizeInMb;
        public string Title;
        public string episodeNumber;

        public ShowEntry (string lineEntry) {

            Regex nameMatcher = new Regex ("(?:b:\"|f:\")([^\"]*)"); //Match 0 group 1 = bot name
            //match 1 group 1 = show name

            var nameMatches = nameMatcher.Match (lineEntry);
            botName = nameMatches.Groups[1].Value;
            Title = nameMatches.NextMatch ().Groups[1].Value;
            Regex packNumberMatcher = new Regex (@"n:(\d*)");
            var packnumberMatches = packNumberMatcher.Match (lineEntry);
            packNumber = packnumberMatches.Groups[1].ToString ();
            Regex sizeNumberMatcher = new Regex (@"s:(\d*)");
            var sizeNumberMatches = sizeNumberMatcher.Match (lineEntry);
            sizeInMb = sizeNumberMatches.Groups[1].Value;
            Regex episodeNumberMatcher = new Regex (@"(?:- )(\d*)");
            var regexNumberMatches = episodeNumberMatcher.Match (Title);
            episodeNumber = regexNumberMatches.Groups[1].Value;

        }

        public bool Verify () {
            return botName != "" && packNumber != "" && sizeInMb != "" && Title != "" && episodeNumber != "";
        }
        public string PrettyTitle () {
            Regex titlePrettifier = new Regex ("(.+)(?: -)");
            return titlePrettifier.Match (Title).Groups[1].Value.Split (']') [1].TrimStart (' ');
        }

        public void PrettyPrint () {
            Console.WriteLine (PrettyTitle ());
            Console.WriteLine ("├─" + episodeNumber);
            Console.WriteLine ("├─" + botName);
            Console.WriteLine ("├─" + packNumber);
            Console.WriteLine ("└─" + sizeInMb + " megabytes");
        }
    }
    public enum Quality {
        TEN_EIGHTY_P,
        SEVEN_TWENTY_P,
        STANDARD_DEFINITION,

    }

}