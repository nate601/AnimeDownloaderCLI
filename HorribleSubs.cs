using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

public partial class HorribleSubsPacklist
{
    public List<ShowEntry> GetShow(string title)
    {
        string url = "https://xdcc.horriblesubs.info/search.php" + "?t=" + title + "";
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
        req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

        req.Referer = @"https://xdcc.horriblesubs.info/";
        req.Method = "GET";

        HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
        string response;
        using (StreamReader input = new StreamReader(resp.GetResponseStream()))
        {
            response = input.ReadToEnd();
        }

        List<ShowEntry> shows = new List<ShowEntry>();
        foreach (var s in response.Split(Environment.NewLine.ToCharArray()))
        {
            shows.Add(new ShowEntry(s));
        }

        var showVerified = new List<ShowEntry>();
        foreach (var showEntry in shows)
        {
            if (showEntry.Verify())
                showVerified.Add(showEntry);
        }

        return showVerified;

    }

}