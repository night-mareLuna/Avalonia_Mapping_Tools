using Avalonia_Mapping_Tools;
using Avalonia_Mapping_Tools.ViewModels;
using Avalonia_Mapping_Tools.Views;
using Mapping_Tools.Classes.ToolHelpers;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mapping_Tools.Classes.SystemTools {
    public static class SettingsManager {
        private static string JsonPath { get; set; }
        private static readonly JsonSerializer serializer = new() {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };

        public static readonly Settings Settings = new();
		private static readonly string ConfigFolder = Program.configPath;
        public static bool InstanceComplete;

        public static async Task LoadConfig() {
            JsonPath = ConfigFolder + "/config.json";
            InstanceComplete = File.Exists(JsonPath) ? await LoadFromJson() : await CreateJson();

            try {
                DefaultPaths();
            } catch (Exception e) {
                await e.Show();
            }
        }

        private static async Task<bool> LoadFromJson() {
            try {
                using( StreamReader sr = new StreamReader(JsonPath)) {
                    using (JsonReader reader = new JsonTextReader(sr)) {
                        Settings newSettings = serializer.Deserialize<Settings>(reader)!;
                        newSettings.CopyTo(Settings);
                    }
                }
            }
            catch( Exception ex ) {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.Message);

				var box = MessageBoxManager.GetMessageBoxStandard("","User-specific configuration could not be loaded!");
                await box.ShowAsync();
                await ex.Show();
                return false;
            }
            return true;
        }

        private static async Task<bool> CreateJson() {
            try {
                using( StreamWriter sw = new StreamWriter(JsonPath)) {
                    using (JsonWriter writer = new JsonTextWriter(sw)) {
                        serializer.Serialize(writer, Settings);
                    }
                }
            }
            catch( Exception ex ) {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.Message);

				var box = MessageBoxManager.GetMessageBoxStandard("","User-specific configuration could not be loaded!");
				await box.ShowAsync();
                await ex.Show();
                return false;
            }
            return true;
        }

        public static async Task<bool> WriteToJson(bool doLoading=false) {
            try {
                using( StreamWriter sw = new StreamWriter(JsonPath)) {
                    using (JsonWriter writer = new JsonTextWriter(sw)) {
                        serializer.Serialize(writer, Settings);
                    }
                }
            }
            catch( Exception ex ) {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.Message);

				var box = MessageBoxManager.GetMessageBoxStandard("","User-specific configuration could not be saved!");
				await box.ShowAsync();
				await ex.Show();
                return false;
            }

            if( doLoading ) {
                await LoadFromJson();
            }

            return true;
        }

        public static void AddRecentMap(string[] paths, DateTime date) {
            foreach (var path in paths)
            {
                Settings.RecentMaps.RemoveAll(o => o[0] == path);
                if (Settings.RecentMaps.Count > 19) {
                    try {
                        Settings.RecentMaps.Remove(Settings.RecentMaps.Last());
                    } catch (ArgumentOutOfRangeException) {
                    }
                }
                Settings.RecentMaps.Insert(0, new[] { path, date.ToString(CultureInfo.CurrentCulture) });
            }
        }

        public static void AddRecentMap(string path, DateTime date) => AddRecentMap([path], date);

        public static async void DefaultPaths() {
            if (string.IsNullOrWhiteSpace(Settings.OsuPath)) {
             //try {
             //    var regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
             //    Settings.OsuPath = FindByDisplayName(regKey, "osu!");
             //} catch (KeyNotFoundException) {
             //    try {
             //        var regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
             //        Settings.OsuPath = FindByDisplayName(regKey, "osu!");
             //    } catch (KeyNotFoundException) {
             //        Settings.OsuPath = Path.Combine(AppComm, "osu!");
             //        MessageBox.Show("Could not automatically find osu! install directory. Please set the correct paths in the Preferences.");
             //    }
             //}
				Settings.OsuPath = await FindOsuPath();
				if(Settings.OsuPath == string.Empty)
				{
					var box = MessageBoxManager.GetMessageBoxStandard("Error!",
						"Please set the correct paths in the Preferences.");
					box.ShowAsync();
				}
            }

            if (string.IsNullOrWhiteSpace(Settings.OsuConfigPath)) {
                Settings.OsuConfigPath = Path.Combine(Settings.OsuPath, $"osu!.{Environment.UserName}.cfg");
            }

            if (string.IsNullOrWhiteSpace(Settings.SongsPath)) {
                var beatmapDirectory = GetBeatmapDirectory(Settings.OsuConfigPath);
                Settings.SongsPath = Path.Combine(Settings.OsuPath, beatmapDirectory);
            }

            if (string.IsNullOrWhiteSpace(Settings.BackupsPath)) {
                Settings.BackupsPath = ConfigFolder + "/Backups";
            }

            if (string.IsNullOrWhiteSpace(Settings.GosumemoryPath)) {
                await SetupGosumemory();
            }
            //Directory.CreateDirectory(Settings.BackupsPath);
        }

        private static string GetBeatmapDirectory(string configPath) {
            try {
                foreach (var line in File.ReadLines(configPath)) {
                    var split = line.Split('=');
                    if (split[0].Trim() == "BeatmapDirectory") {
                        return split[1].Trim();
                    }
                }
            }
            catch (Exception exception) {
                Console.WriteLine(exception);
            }

            return "Songs";
        }

        // private static string FindByDisplayName(RegistryKey parentKey, string name) {
        //     var nameList = parentKey.GetSubKeyNames();
        //     foreach (var t in nameList)
        //     {
        //         RegistryKey regKey = parentKey.OpenSubKey(t);
        //         try {
        //             if (regKey != null && regKey.GetValue("DisplayName")?.ToString() == name) {
        //                 return Path.GetDirectoryName(regKey.GetValue("UninstallString")?.ToString());
        //             }
        //         } catch (NullReferenceException) { }
        //     }

        //     throw new KeyNotFoundException($"Could not find registry key with display name \"{name}\".");
        // }

        public static List<string[]> GetRecentMaps() {
            return Settings.RecentMaps;
        }

        public static string[] GetLatestCurrentMaps() {
            if (GetRecentMaps().Count > 0) {
                return GetRecentMaps()[0][0].Split('|');
            } else {
                return new[] { "" };
            }
        }

        public static string GetOsuPath() {
            return Settings.OsuPath;
        }

        public static string GetSongsPath() {
            return Settings.SongsPath;
        }

        public static string GetBackupsPath() {
            return Settings.BackupsPath;
        }

        public static bool GetMakeBackups() {
            return Settings.MakeBackups;
        }

		public static bool GetTheme() {
			return Settings.DarkTheme;
		}

        public static string GetGosumemPath() {
            return Settings.GosumemoryPath;
        }

		private static async Task<string> FindOsuPath()
		{
			string? path;
            path = TryRunningProcess();
			path ??= TryOsuWinello();
			//path ??= TryLutris();
			path ??= await SelectOsuFolder();

			return path ?? "";
		}

        private static string? TryRunningProcess()
        {
            string? path = null;
            
            bool isOsuRunning = Bash.RunCommand("pgrep osu\\!.exe") != string.Empty;
            if(isOsuRunning)
            {
                string result = Bash.RunCommand("cat /proc/`pgrep osu\\!.exe`/cmdline");
                if(result[0] == 'Z' || result[0] == 'z')
                    path = $"/{string.Join('/', result.Split("\\")[1 .. ^1])}/";
            }
            return path;
        }

		private static string? TryOsuWinello()
		{
			string? path = null;
			
			string result = Bash.RunCommand($"/home/{Environment.UserName}/.local/bin/osu-wine --info");
			foreach(string line in result.Split(Environment.NewLine))
			{
				if(line.Contains("osu! folder:"))
					return line.Split(":")[^1].Trim()[1..^1] + '/';
			}

			return path;
		}

		private static string? TryLutris()
		{
			throw new NotImplementedException();
		}

		private static async Task<string?> SelectOsuFolder()
		{
			string? folder;

			do
			{
				var box = MessageBoxManager.GetMessageBoxStandard("",
					"Please select your osu! folder",
					ButtonEnum.Ok);
				await box.ShowAsync();
				string path = await IOHelper.FolderDialog("");
                path = path[^1] == '/' ? path : path + '/';
                if(File.Exists(path + "osu!.exe"))
                    folder = path;
                else
                {
                    var warningBox = MessageBoxManager.GetMessageBoxStandard("Error!",
                        "osu!.exe does not exist in this folder!",
                        ButtonEnum.Ok);
                    await warningBox.ShowAsync();
                    folder = null;
                }
			}
			while(string.IsNullOrEmpty(folder));

			return folder;
		}

        private static async Task SetupGosumemory()
        {
            var useGosuBox = MessageBoxManager.GetMessageBoxStandard("Gosumemory Setup",
                "Do you want to use Gosumemory to read currently selected beatmaps?",
                ButtonEnum.YesNo);
            bool useGosumemory = await useGosuBox.ShowAsync() == ButtonResult.Yes;
            Settings.UseGosumemory = useGosumemory;
            Settings.GosumemoryPath = "none";
            MainWindowViewModel.ChangeUsingGosu(useGosumemory);

            if(!useGosumemory) return;

            var autoGosuBox = MessageBoxManager.GetMessageBoxStandard("Gosumemory Setup",
                "Do you want Gosumemory to autorun when opening Avalonia Mapping Tools?",
                ButtonEnum.YesNo);
            bool autoGosumemory = await autoGosuBox.ShowAsync() == ButtonResult.Yes;

            if(autoGosumemory)
            {
                Settings.RunGosumemory = await DownloadGosuMemory();
                GosumemoryReader.StartGosumemory();
            }
            else
            {
                var manualGosuBox =  MessageBoxManager.GetMessageBoxStandard("Gosumemory Setup",
                    "Gosumemory will not run with Avalonia Mapping Tools.\nYou need to manually run it if you want to use it.",
                    ButtonEnum.Ok);
                manualGosuBox.ShowAsync();
                Settings.GosumemoryPath = "none";
            }
        }

        public static async Task<bool> DownloadGosuMemory()
        {
            //string gosuLink32 = "https://github.com/l3lackShark/gosumemory/releases/download/1.3.9/gosumemory_linux_386.zip";
            string gosuLink64 = "https://github.com/l3lackShark/gosumemory/releases/download/1.3.9/gosumemory_linux_amd64.zip";
            string downloadPath = Program.configPath + "/gosumemory_linux_amd64.zip";

            var downloadGosuBox = MessageBoxManager.GetMessageBoxStandard("Gosumemory Setup",
                $"Gosumemory will be downloaded and saved to\n{downloadPath}",
                ButtonEnum.OkAbort);
            
            if(await downloadGosuBox.ShowAsync() == ButtonResult.Ok)
            {
                CancellationTokenSource cts = new();
                var downloadingBox = new DownloadProgress($"Downloading... Please wait.", cts);
                downloadingBox.Show();

                bool downloaded = await DownloadManager.Download(gosuLink64, downloadPath, cts, downloadingBox.ViewModel());
                if(downloaded)
                {
                    string extractPath = Program.configPath + "/gosumemory";
                    downloadingBox.ViewModel().UpdateInfo($"Download complete. Extracting to {extractPath}");
                    downloadingBox.ViewModel().Progress = 0;

                    bool extracted = DownloadManager.Unzip(downloadPath, extractPath);
                    downloadingBox.ViewModel().Progress = 50;
                    if(extracted)
                    {
                        downloadingBox.ViewModel().UpdateInfo("Finalising.");
                        Settings.GosumemoryPath = extractPath + "/gosumemory";
                        File.Delete(downloadPath);
                        downloadingBox.ViewModel().Progress = 95;
                        try
                        {
                            Bash.RunCommand($"chmod +x {Settings.GosumemoryPath}");
                        }
                        catch(Exception e)
                        {
                            e.Show();
                        }
                        downloadingBox.ViewModel().Progress = 100;
                        downloadingBox.ViewModel().UpdateInfo("Gosumemory setup complete");
                        downloadingBox.ViewModel().DownloadComplete = true;
                        return true;
                    }

                }
            }
            else
            {
                var abortBox = MessageBoxManager.GetMessageBoxStandard("Gosumemory Setup",
                    "Aborting download",
                    ButtonEnum.Ok);
                abortBox.ShowAsync();
            }
            return false;
        }


        // internal static void UpdateSettings() {
        //     Settings.MainWindowMaximized = MainWindow.AppWindow.WindowState == WindowState.Maximized;
        //     if (MainWindow.AppWindow.WindowState == WindowState.Maximized) {
        //         Settings.MainWindowRestoreBounds = MainWindow.AppWindow.RestoreBounds;
        //     } else{
        //         Settings.MainWindowRestoreBounds = new Rect(new Point(
        //             MainWindow.AppWindow.Left,
        //             MainWindow.AppWindow.Top
        //             ), new Vector(
        //             MainWindow.AppWindow.Width,
        //             MainWindow.AppWindow.Height));
        //     }
        // }
    }
}
