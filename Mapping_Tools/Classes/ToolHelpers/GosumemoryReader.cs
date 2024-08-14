using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Mapping_Tools.Classes.SystemTools;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Mapping_Tools.Classes.ToolHelpers
{
    public class GosumemoryReader
    {
        private static Process? gosumemory;
        private static bool waitingForOsu = false;

        public static void StartGosumemoryNative()
        {
            if(gosumemory is not null) return;
            if(!SettingsManager.Settings.UseGosumemory || !SettingsManager.Settings.RunGosumemory) return;

            string gosuPath = SettingsManager.GetGosumemPath();
            gosumemory = new Process()
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = gosuPath,
					Arguments = $"-path {SettingsManager.GetSongsPath()}",
					UseShellExecute = false,
                    CreateNoWindow = true
				}
			};

            Console.WriteLine($"Attempting to start {gosuPath}");
            try
            {
			    gosumemory.Start();
            }
            catch(Exception e)
            {
                e.Show();
            }
        }

        public static void StartGosumemory()
        {
            if(gosumemory is not null) return;
            if(!SettingsManager.Settings.UseGosumemory || !SettingsManager.Settings.RunGosumemory) return;
            if(waitingForOsu) return;
            if(string.IsNullOrWhiteSpace(Bash.RunCommandDirect("pgrep", "osu!.exe")))
            {
                waitingForOsu = true;
                Console.WriteLine("Waiting for osu! to launch...");
                Task.Run(() =>
                {
                    do
                    {
                        Thread.Sleep(2000);
                    }while(string.IsNullOrWhiteSpace(Bash.RunCommandDirect("pgrep", "osu!.exe")));
                    waitingForOsu = false;
                    StartGosumemory();
                });
            }
            else
                StartGosumemoryWine();
        }

        private static void StartGosumemoryWine()
        {
            List<string> WineEnvironList = GetWineEnviron();
            string Wine = "wine";
            for(int i=0; i<WineEnvironList.Count; i++)
            {
                if(WineEnvironList[i].Contains("WINELOADER="))
                {
                    Wine = WineEnvironList[i].Replace("WINELOADER=", "");
                    WineEnvironList.Remove(WineEnvironList[i]);
                    break;
                }
            }
            string WineEnviron = string.Empty;

            string gosumemEXE = SettingsManager.GetGosumemPath();
            for(int i=0; i<WineEnvironList.Count; i++)
                WineEnviron += $" {WineEnvironList[i]}";

            gosumemory = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "bash",
                    Arguments = $"-c \"{WineEnviron.Trim()} {Wine} \'{gosumemEXE}\'\""
                }
            };

            Console.WriteLine($"Attempting to start {gosumemEXE}");
            try
            {
			    gosumemory.Start();
                Task.Run(() =>
                {
                    do
                    {
                        Thread.Sleep(2000);
                    }while(!string.IsNullOrWhiteSpace(Bash.RunCommandDirect("pgrep", "osu!.exe")));
                    Console.WriteLine("osu! is no longer detected.");
                    Stop();
                    StartGosumemory();
                });
            }
            catch(Exception e)
            {
                e.Show();
            }
        }

        public static void Stop()
        {
            if(gosumemory is not null)
            {
                try
                {
                    Console.WriteLine("Closing gosumemory");
                    gosumemory.Kill();
                }
                catch(Exception e)
                {
                    e.Show();
                }
                gosumemory = null;
                
            }
        }

        private static async Task<string> ReadSocket()
        {
            string resString = string.Empty;
            try
            {
                int byteCount = 2048;   // Make sure there's enough bytes to at least get the info we need

                Uri uri = new("ws://localhost:24050/ws");
                using ClientWebSocket ws = new();
                await ws.ConnectAsync(uri, default);
                byte[] bytes = new byte[byteCount];
                var resRaw = await ws.ReceiveAsync(bytes, default);
                resString = Encoding.UTF8.GetString(bytes, 0, resRaw.Count);
            }
            catch
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error!",
                "Either gosumemory or osu! is not running.",
                ButtonEnum.Ok);
				box.ShowAsync();
            }
            return resString;
        }

        public static async Task<string> GetCurrentBeatmap()
        {
            string gosumemoryString = await ReadSocket();
            if(string.IsNullOrEmpty(gosumemoryString)) return gosumemoryString;

            string fileName = gosumemoryString[gosumemoryString.IndexOf("\"file\"") .. (gosumemoryString.IndexOf(".osu\"") + 5)];
            string folderName = gosumemoryString[gosumemoryString.IndexOf("\"folder\"") .. gosumemoryString.IndexOf("\"file\"")];

            string fullPath = SettingsManager.GetSongsPath() + (SettingsManager.GetSongsPath()[^1] == '/' ? "" : '/') +
                folderName.Split(':')[1][1 .. ^2] + '/' +
                fileName.Split(':')[1][1 .. ^1];

            // Unescape for rare issues with certain unicode characters like '&'
            return Regex.Unescape(fullPath);
        }

        private static List<string> GetWineEnviron()
        {
            List<string> WineEnviron = [];
            string[] environ = Bash.RunCommand("cat /proc/`pgrep osu\\!.exe`/environ").Split('\0');
            string[] envars = ["WINELOADER", "WINEARCH", "WINEPREFIX", "WINEESYNC", "WINEFSYNC"];

            for(int i=0; i<environ.Length; i++)
            {
                for(int j=0; j<envars.Length; j++)
                {
                    if(environ[i].Contains($"{envars[j]}="))
                        WineEnviron.Add(environ[i].Trim());
                }
            }

            return WineEnviron;
        }
    }
}