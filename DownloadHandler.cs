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
            irc.SetCustomDownloadDir (System.IO.Directory.GetCurrentDirectory ());
            irc.StartClient ();
        }
        private void DownloadStatusChanged (object sender, DCCEventArgs args) {
            Console.Clear ();
            Console.WriteLine ($"Current File: {args.FileName} ");
            Console.WriteLine ($"{args.Progress}%");
            foreach (var downloadPair in downloadQueue) {
                Console.WriteLine ($"{downloadPair.BotName} : {downloadPair.PackNumber}");
            }
            if (args.Status == COMPLETED_STATUS && downloadQueue.Count != 0) {
                var next = downloadQueue.Dequeue ();
                SendToDownloadbot (next);
            }
            if (args.Status == COMPLETED_STATUS && downloadQueue.Count == 0) {
                Console.WriteLine ("Download completed. Press any key to exit.");
            }
        }

        [Obsolete ("Use DownloadPair")]
        public void SendToDownloadbot (string botName, int packNumber) {
            irc.SendMessageToChannel ($@"/msg {botName} xdcc send {packNumber.ToString()}", RIZON_IRC_CHANNEL_NAME);
        }
        public void SendToDownloadbot (DownloadPair pair) {
            irc.SendMessageToChannel ($@"/msg {pair.BotName} xdcc send {pair.PackNumber.ToString()}", RIZON_IRC_CHANNEL_NAME);
        }

        public void Download (string botName, int packNumber) {
            downloadQueue.Enqueue (new DownloadPair (botName, packNumber));
            if (firstRun) {
                var next = downloadQueue.Dequeue ();
                SendToDownloadbot (next);
                firstRun = false;
            }
        }

        public struct DownloadPair {
            public string BotName { get; set; }
            public int PackNumber { get; set; }

            public DownloadPair (string botName, int packNumber) {
                this.BotName = botName;
                this.PackNumber = packNumber;
            }
        }
        public void SetDownloadDirectory (string path) {
            irc.SetCustomDownloadDir (path);
        }
    }
}