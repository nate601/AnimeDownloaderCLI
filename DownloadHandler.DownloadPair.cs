namespace AnimeDown {
    public partial class DownloadHandler {
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
    }
}