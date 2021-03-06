﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SimpleIRCLib;
using System.IO;
using System.Text.RegularExpressions;

namespace AnimeDown
{
    public partial class DownloadHandler
    {
        private const string COMPLETED_STATUS = "COMPLETED";
        private const string RIZON_IRC_SERVER_HOST = "irc.rizon.net";
        private const int RIZON_IRC_SERVER_PORT = 6697;
        private const string RIZON_IRC_CHANNEL_NAME = "#horriblesubs";
        private const string IRC_USERNAME_PREFIX = "animeguy69";
        private SimpleIRC irc;
        private readonly Queue<DownloadPair> downloadQueue = new Queue<DownloadPair>();
        private bool firstRun = true;
        private string[] currentUsers;
        public DownloadHandler()
        {
            irc = new SimpleIRC();
            irc.SetupIrc(RIZON_IRC_SERVER_HOST, IRC_USERNAME_PREFIX + new Random().Next(100), RIZON_IRC_CHANNEL_NAME, RIZON_IRC_SERVER_PORT);
            irc.DccClient.OnDccEvent += DownloadStatusChanged;
            irc.IrcClient.OnUserListReceived += OnUserListReceived;
            irc.IrcClient.OnRawMessageReceived += RawMessageDebugLog;
            irc.SetCustomDownloadDir(System.IO.Directory.GetCurrentDirectory()); // ! This will be overwritten later.
            _ = irc.IrcClient.Connect();
            int timeout = 0;
            int cursorTop = Console.CursorTop;
            while (!irc.IrcClient.IsClientRunning())
            {
                Console.SetCursorPosition(0,cursorTop);
                Console.Write("Waiting to connect to server");
                for (int i = 0; i < timeout % 4; i++)
                {
                    Console.Write(".");
                }
                Thread.Sleep(50);
                timeout++;
                if (timeout > 3000)
                {
                    Console.WriteLine("Unable to connect to server!");
                    throw new Exception("Unable to connect to server");
                }
            }
            Console.WriteLine("Connected.");
        }


        private void RawMessageDebugLog(object sender, IrcRawReceivedEventArgs e)
        {
            Debug.WriteLine(e.Message);
        }

        public bool IsUserPresent(string userName)
        {
            if (currentUsers == null)
                WaitForUserList();
            return currentUsers.Contains(userName.ToLower());
        }

        private void WaitForUserList()
        {
            Console.WriteLine("Waiting for user list");
            using AutoResetEvent are = new AutoResetEvent(false);
            new Task(async () =>
            {
                while (currentUsers == null)
                {
                    System.Console.WriteLine("Still waiting...");
                    await Task.Delay(500);
                }
                are.Set();
            }).Start();

            are.WaitOne((int)TimeSpan.FromSeconds(30).TotalMilliseconds);

            if (currentUsers == null)
                throw new Exception("Never recieved a user list!");
            Console.WriteLine("User list found.");
        }

        private void OnUserListReceived(object sender, IrcUserListReceivedEventArgs e)
        {
            currentUsers = e.UsersPerChannel
                .Where(
                    (s) => s.Key.ToLower() == RIZON_IRC_CHANNEL_NAME.ToLower()
                )
                .First()
                .Value.Select(
                    (s) => s.ToLower()
                )
                .Where(
                    (s) => s.StartsWith('%')
                )
                .Select(
                    (s) => s.TrimStart('%')
                )
                .ToArray();
        }
        private void DownloadStatusChanged(object sender, DCCEventArgs args)
        {
            Console.Clear();
            Console.WriteLine($"Current File: {args.FileName} ");
            //Display progress for current file
            Console.WriteLine($"{args.Progress}%");
            Console.WriteLine();
            Console.Write("|");
            for (int i = 0; i <= args.Progress; i++)
            {
                Console.Write("%");
            }
            for (int i = args.Progress; i < 100; i++)
            {
                Console.Write(" ");
            }
            Console.Write("|");
            Console.WriteLine();
            Console.WriteLine($"{downloadQueue.Count + 1} files remaining");
            Console.Title = $"{downloadQueue.Count + 1} files remaining";
            Console.WriteLine();
            Console.WriteLine(string.Join('\n', downloadQueue.Select((downloadPair) => $"{downloadPair.DisplayTitle}\n :{downloadPair.BotName} : {downloadPair.PackNumber}")));
            if (args.Status == COMPLETED_STATUS && downloadQueue.Count != 0)
            {
                DownloadPair next = downloadQueue.Dequeue();
                PostDownload(args.FileName);
                SendToDownloadbot(next);
            }
            if (args.Status == COMPLETED_STATUS && downloadQueue.Count == 0)
            {
                Console.WriteLine("Download completed.");
                Console.Title = "Download completed";
                PostDownload(args.FileName);
            }
        }

        private void PostDownload(string fileName)
        {
            string modifiedFileName = Regex.Replace(fileName, @"\s?(\[.*?\])\s?", "");
            modifiedFileName = Regex.Replace(modifiedFileName, @"(-\s)", "- Episode ");
            File.Move(Path.Combine(downloadDirectory, fileName), Path.Combine(downloadDirectory, modifiedFileName));
        }
        private void SendToDownloadbot(DownloadPair pair)
        {
            irc.SendMessageToChannel($@"/msg {pair.BotName} xdcc send #{pair.PackNumber.ToString()}", RIZON_IRC_CHANNEL_NAME);
        }

        public void Download(DownloadPair pair)
        {
            downloadQueue.Enqueue(pair);
            if (firstRun)
            {
                DownloadPair next = downloadQueue.Dequeue();
                SendToDownloadbot(next);
                firstRun = false;
                Console.WriteLine("Waiting to be sent first DCC request");
            }
        }

        private string downloadDirectory;
        public void SetDownloadDirectory(string path)
        {
            downloadDirectory = path;
            irc.SetCustomDownloadDir(path);
        }
    }
}
