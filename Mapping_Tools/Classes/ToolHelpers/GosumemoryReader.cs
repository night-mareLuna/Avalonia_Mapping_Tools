using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Mapping_Tools.Classes.SystemTools;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Mapping_Tools.Classes.ToolHelpers
{
    public class GosumemoryReader
    {
        private static Process? gosumemory;

        public static void StartGosumemory()
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

        public static void Stop()
        {
            if(gosumemory is not null)
            {
                Console.WriteLine("Closing gosumemory");
                gosumemory.Kill();
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
    }
}