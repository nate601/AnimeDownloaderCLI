using System;
using System.Collections;
using System.Collections.Generic;
using SimpleIRCLib;

namespace AnimeDown {
    public class DownloadHandler {
        private const string COMPLETED_STATUS = "COMPLETED";
        private const string RIZON_IRC_SERVER_HOST = "irc.rizon.net";
        private const int RIZON_IRC_SERVER_PORT = 6697;
        private const string RIZON_IRC_CHANNEL_NAME = "#horriblesubs";
        private const string IRC_USERNAME_PREFIX = "animeguy69";
        private readonly SimpleIRC irc;
        readonly Queue<DownloadPair> downloadQueue = new Queue<DownloadPair> ();
        private bool firstRun = true;
        public DownloadHandler () {
            irc = new SimpleIRC ();
            irc.SetupIrc (RIZON_IRC_SERVER_HOST, IRC_USERNAME_PREFIX + new Random ().Next (100), RIZON_IRC_CHANNEL_NAME, RIZON_IRC_SERVER_PORT);
            irc.DccClient.OnDccEvent += DownloadStatusChanged;
            irc.SetCustomDownloadDir (System.IO.Directory.GetCurrentDirectory ()); // ! This will be overwritten later.
            irc.StartClient ();
        }
        private void DownloadStatusChanged (object sender, DCCEventArgs args) {
            Console.Clear ();
            Console.WriteLine ($"Current File: {args.FileName} ");
            Console.WriteLine ($"{args.Progress}%");
            foreach (var downloadPair in downloadQueue) {
                Console.WriteLine ($"{downloadPair.DisplayTitle} : {downloadPair.BotName} : {downloadPair.PackNumber}");
            }
            if (args.Status == COMPLETED_STATUS && downloadQueue.Count != 0) {
                var next = downloadQueue.Dequeue ();
                SendToDownloadbot (next);
            }
            if (args.Status == COMPLETED_STATUS && downloadQueue.Count == 0) {
                Console.WriteLine ("Download completed. Press any key to exit.");
            }
        }

        private void SendToDownloadbot (DownloadPair pair) {
            irc.SendMessageToChannel ($@"/msg {pair.BotName} xdcc send {pair.PackNumber.ToString()}", RIZON_IRC_CHANNEL_NAME);
        }

        public void Download (DownloadPair pair) {
            downloadQueue.Enqueue (pair);
            if (firstRun) {
                var next = downloadQueue.Dequeue ();
                SendToDownloadbot (next);
                firstRun = false;
            }
        }
        public struct DownloadPair {
            public DownloadPair (string botName, int packNumber, string displayTitle) {
                this.BotName = botName;
                this.PackNumber = packNumber;
                this.DisplayTitle = displayTitle;
            }
            public string BotName { get; set; }
            public int PackNumber { get; set; }
            public string DisplayTitle { get; set; }

        }
        public void SetDownloadDirectory (string path) {
            irc.SetCustomDownloadDir (path);
        }
    }
}