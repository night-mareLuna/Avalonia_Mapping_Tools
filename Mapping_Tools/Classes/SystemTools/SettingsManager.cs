using Avalonia_Mapping_Tools;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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
						"Could not automatically find osu! install directory. Please set the correct paths in the Preferences.");
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

		private static async Task<string> FindOsuPath()
		{
			string? path = null;
            path = TryRunningProcess();
			path ??= TryOsuWinello();
			//path ??= TryLutris();
			path ??= await SelectOsuFolder();

			return path ?? "";
		}

        private static string? TryRunningProcess()
        {
            string? path = null;
            
            bool isOsuRunning = BashCommand("pgrep osu\\!.exe") != string.Empty;
            if(isOsuRunning)
            {
                string result = BashCommand("cat /proc/`pgrep osu\\!.exe`/cmdline");
                if(result[0] == 'Z' || result[0] == 'z')
                    path = $"/{string.Join('/', result.Split("\\")[1 .. ^1])}/";
            }
            return path;
        }

		private static string? TryOsuWinello()
		{
			string? path = null;
			
			string result = BashCommand($"/home/{Environment.UserName}/.local/bin/osu-wine --info");
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
				folder = await IOHelper.FolderDialog("");
			}
			while(string.IsNullOrEmpty(folder));

			return folder;
		}

		private static string BashCommand(string command)
		{
			if(string.IsNullOrWhiteSpace(command))
				return string.Empty;

			string commandOutput = string.Empty;
			try
			{
				var process = new Process()
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = "bash",
						Arguments = $"-c \"{command}\"",
						RedirectStandardOutput = true,
						UseShellExecute = false,
						CreateNoWindow = true,
						WorkingDirectory = Path.GetDirectoryName("/usr/bin")
					}
				};
				process.Start();
				commandOutput = process.StandardOutput.ReadToEnd();
				process.WaitForExit();
			}
			catch(Exception e)
			{
                Console.WriteLine(e.Message);
            }

			return commandOutput;
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
