using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;

public partial class HorribleSubsPacklist {
    public List<ShowEntry> GetShow (string title, Quality quality) {
        string url = "https://xdcc.horriblesubs.info/search.php" + "?t=" + title + "";
        HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
        req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

        req.Referer = @"https://xdcc.horriblesubs.info/";
        req.Method = "GET";

        HttpWebResponse resp = (HttpWebResponse) req.GetResponse ();
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
    public enum Quality {
        TEN_EIGHTY_P,
        SEVEN_TWENTY_P,
        STANDARD_DEFINITION,

    }

}