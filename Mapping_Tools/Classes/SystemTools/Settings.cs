using System;
using System.Collections.Generic;

namespace Mapping_Tools.Classes.SystemTools {
    public class Settings : BindableBase {
        public List<string[]> RecentMaps { get; set; }
        public List<string> FavoriteTools { get; set; }
        public double[] MainWindowRestoreBounds { get; set; }
        public bool MainWindowMaximized { get; set; }

        private string osuPath;
        public string OsuPath {
            get => osuPath;
            set => Set(ref osuPath, value);
        }

        private string songsPath;
        public string SongsPath {
            get => songsPath;
            set => Set(ref songsPath, value);
        }

        private string backupsPath;
        public string BackupsPath {
            get => backupsPath;
            set => Set(ref backupsPath, value);
        }

        private string osuConfigPath;
        public string OsuConfigPath {
            get => osuConfigPath;
            set => Set(ref osuConfigPath, value);
        }

        private bool makeBackups;
        public bool MakeBackups {
            get => makeBackups;
            set => Set(ref makeBackups, value);
        }

        private bool useEditorReader;
        public bool UseEditorReader {
            get => useEditorReader;
            set => Set(ref useEditorReader, value);
        }

        private bool overrideOsuSave;
        public bool OverrideOsuSave {
            get => overrideOsuSave;
            set => Set(ref overrideOsuSave, value);
        }

        private bool autoReload;
        public bool AutoReload {
            get => autoReload;
            set => Set(ref autoReload, value);
        }

        private Hotkey quickRunHotkey;
        public Hotkey QuickRunHotkey {
            get => quickRunHotkey;
            set => Set(ref quickRunHotkey, value);
        }

        private bool smartQuickRunEnabled;
        public bool SmartQuickRunEnabled {
            get => smartQuickRunEnabled;
            set => Set(ref smartQuickRunEnabled, value);
        }

        private string noneQuickRunTool;
        public string NoneQuickRunTool {
            get => noneQuickRunTool;
            set => Set(ref noneQuickRunTool, value);
        }

        private string singleQuickRunTool;
        public string SingleQuickRunTool {
            get => singleQuickRunTool;
            set => Set(ref singleQuickRunTool, value);
        }

        private string multipleQuickRunTool;
        public string MultipleQuickRunTool {
            get => multipleQuickRunTool;
            set => Set(ref multipleQuickRunTool, value);
        }

        private Hotkey betterSaveHotkey;
        public Hotkey BetterSaveHotkey {
            get => betterSaveHotkey;
            set => Set(ref betterSaveHotkey, value);
        }

        private int maxBackupFiles;
        public int MaxBackupFiles {
            get => maxBackupFiles;
            set => Set(ref maxBackupFiles, value);
        }

        private bool makePeriodicBackups;
        public bool MakePeriodicBackups {
            get => makePeriodicBackups;
            set
			{
				Set(ref makePeriodicBackups, value);
                OnMakePeriodicBackupsChanged(value);
			}
        }

        private TimeSpan periodicBackupInterval;
        public TimeSpan PeriodicBackupInterval {
            get => periodicBackupInterval;
            set
			{
				Set(ref periodicBackupInterval, value);
				OnPeriodicBackupIntervalChanged(value);
			}
        }

        private bool currentBeatmapDefaultFolder;
        public bool CurrentBeatmapDefaultFolder {
            get => currentBeatmapDefaultFolder;
            set => Set(ref currentBeatmapDefaultFolder, value);
        }

        private Hotkey quickUndoHotkey;
        public Hotkey QuickUndoHotkey {
            get => quickUndoHotkey;
            set => Set(ref quickUndoHotkey, value);
        }

        private Version? skipVersion;
        public Version? SkipVersion {
            get => skipVersion;
            set => Set(ref skipVersion, value);
        }

		private bool darkTheme;
		public bool DarkTheme
		{
			get => darkTheme;
			set => Set(ref darkTheme, value);
		}

		private bool showSaveDialog;
		public bool ShowSaveDialog
		{
			get => showSaveDialog;
			set => Set(ref showSaveDialog, value);
		}
        private bool useGosumemory;
        public bool UseGosumemory
        {
            get => useGosumemory;
            set => Set(ref useGosumemory, value);
        }
        private bool runGosumemory;
		public bool RunGosumemory
		{
			get => runGosumemory;
			set => Set(ref runGosumemory, value);
		}
        private string gosumemoryPath;
        public string GosumemoryPath
        {
            get => gosumemoryPath;
            set => Set(ref gosumemoryPath, value);
        }

        /// <summary>
        /// Makes a new Settings objects and initializes default settings.
        /// </summary>
        public Settings() {
            RecentMaps = new List<string[]>();
            FavoriteTools = new List<string>();
            MainWindowRestoreBounds = [1280, 720];
            MainWindowMaximized = false;
            OsuPath = "";
            SongsPath = "";
            BackupsPath = "";
            MakeBackups = true;
            UseEditorReader = false;
            OverrideOsuSave = false;
            AutoReload = false;
            SmartQuickRunEnabled = false;
            NoneQuickRunTool = "<Current Tool>";
            SingleQuickRunTool = "<Current Tool>";
            MultipleQuickRunTool = "<Current Tool>";
            MaxBackupFiles = 1000;
            MakePeriodicBackups = true;
            PeriodicBackupInterval = TimeSpan.FromMinutes(10);
            CurrentBeatmapDefaultFolder = true;
            SkipVersion = null;
			DarkTheme = true;
			ShowSaveDialog = true;
            UseGosumemory = false;
            RunGosumemory = false;
            GosumemoryPath = "";
        }

		private static void OnMakePeriodicBackupsChanged(bool newValue) =>
			ListenerManager.ContinuePeriodicBackups(newValue);

		private void OnPeriodicBackupIntervalChanged(TimeSpan newValue)
		{
			if(newValue.TotalSeconds == 0)
			{
				PeriodicBackupInterval = new TimeSpan(0,5,0);
				return;
			}
			ListenerManager.PeriodicBackupTimerChange(newValue);
		}

        public void CopyTo(Settings other) {
            foreach (var prop in typeof(Settings).GetProperties()) {
                if (!prop.CanRead || !prop.CanWrite) { continue; }
                prop.SetValue(other, prop.GetValue(this));
            }
        }
    }
}
