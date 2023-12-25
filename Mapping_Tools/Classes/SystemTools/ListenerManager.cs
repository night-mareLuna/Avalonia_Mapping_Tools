using System;
using System.Collections.Generic;
using System.IO;
using Mapping_Tools.Classes.BeatmapHelper;
using Mapping_Tools.Classes.ToolHelpers;
using Avalonia.Threading;
using Avalonia_Mapping_Tools;

namespace Mapping_Tools.Classes.SystemTools {
    public class ListenerManager {
        private string? previousPeriodicBackupHash;

        public readonly FileSystemWatcher FsWatcher = new FileSystemWatcher();
        //public readonly KeyboardHookManager KeyboardHookManager = new KeyboardHookManager();
        public Dictionary<string, ActionHotkey> ActiveHotkeys = new Dictionary<string, ActionHotkey>();
        public DispatcherTimer? PeriodicBackupTimer;
		private static ListenerManager? Me;

        public ListenerManager()
        {

            //LoadHotkeys();
            //ReloadHotkeys();
            //KeyboardHookManager.Start();
			Me = this;
            InitPeriodicBackupTimer();
        }

		public static void ContinuePeriodicBackups(bool cont)
		{
			if(Me is null) return;
			if(cont)
				Me.PeriodicBackupTimer!.Start();
			else
				Me.PeriodicBackupTimer!.Stop();
		}

		public static void PeriodicBackupTimerChange(TimeSpan time)
		{
			if(Me is null) return;
			Me.PeriodicBackupTimer!.Interval = time;
		}

		private void InitPeriodicBackupTimer() {
            previousPeriodicBackupHash = string.Empty;

            PeriodicBackupTimer = new DispatcherTimer(DispatcherPriority.Background)
                {Interval = SettingsManager.Settings.PeriodicBackupInterval};
            PeriodicBackupTimer.Tick += PeriodicBackupTimerOnTick;

            if (SettingsManager.Settings.MakePeriodicBackups) {
                PeriodicBackupTimer.Start();
            }
        }

        private async void PeriodicBackupTimerOnTick(object? sender, EventArgs e) {
            try {
                // Get the newest beatmap, save a temp version, get the hash and compare it to the previous hash, backup temp file
                var path = IOHelper.GetCurrentBeatmap();

                if (string.IsNullOrEmpty(path)) return;

                // Don't make period backup if the editor is not open
                //if (!EditorReaderStuff.IsEditorOpen()) return;

                //EditorReader reader = null;
                // try {
                //     reader = EditorReaderStuff.GetFullEditorReader();
                // } catch (Exception ex) {
                //     Console.WriteLine(ex.MessageStackTrace());
                // }

                var editor = EditorReaderStuff.GetNewestVersionOrNot(path);

                // Save temp version
                var tempPath = Program.configPath + "/temp.osu";

                Editor.SaveFile(tempPath, editor.Beatmap.GetLines());

                // Get MD5 from temp file
                var currentMapHash = EditorReaderStuff.GetMd5FromPath(tempPath);

                // Comparing with previously made periodic backup
                if (currentMapHash == previousPeriodicBackupHash) {
                    return;
                }

                // Saving backup of the map
                await BackupManager.SaveMapBackup(tempPath, true, Path.GetFileName(path), "PB");  // PB stands for Periodic Backup

                previousPeriodicBackupHash = currentMapHash;
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
