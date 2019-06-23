using System;
using System.Collections;
using System.Collections.Generic;
using SimpleIRCLib;

namespace AnimeDown {
    public class DownloadHandler {
        private readonly SimpleIRC irc;
        readonly Queue<DownloadPair> downloadQueue = new Queue<DownloadPair> ();
        private bool firstRun = true;
        public DownloadHandler () {
            irc = new SimpleIRC ();
            irc.SetupIrc ("irc.rizon.net", "animeguy69" + new Random ().Next (70), "#horriblesubs", 6697);
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
            if (args.Status == "COMPLETED" && downloadQueue.Count != 0) {
                var next = downloadQueue.Dequeue ();
                SendToDownloadbot (next.BotName, next.PackNumber);
            }
            if (args.Status == "COMPLETED" && downloadQueue.Count == 0) {
                Console.WriteLine ("Download completed. Press any key to exit.");
            }
        }
        public void SendToDownloadbot (string botName, int packNumber) {
            irc.SendMessageToChannel ($@"/msg {botName} xdcc send {packNumber.ToString()}", "#horriblesubs");
        }

        public void Download (string botName, int packNumber) {
            downloadQueue.Enqueue (new DownloadPair (botName, packNumber));
            if (firstRun) {
                var next = downloadQueue.Dequeue ();
                SendToDownloadbot (next.BotName, next.PackNumber);
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