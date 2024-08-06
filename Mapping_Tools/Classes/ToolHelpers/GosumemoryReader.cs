using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Mapping_Tools.Classes.SystemTools;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Mapping_Tools.Classes.ToolHelpers
{
    public static class GosumemoryReader
    {
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
                "Gosumemory most likely is not running.",
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

            return fullPath;
        }
    }
}