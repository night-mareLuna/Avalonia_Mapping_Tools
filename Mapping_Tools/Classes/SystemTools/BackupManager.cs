﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Mapping_Tools.Classes.BeatmapHelper;
using Mapping_Tools.Classes.Exceptions;
using Mapping_Tools.Classes.ToolHelpers;
using Mapping_Tools.Classes.Tools;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Mapping_Tools.Classes.SystemTools {
    public static class BackupManager {
        public static async Task<bool> SaveMapBackup(string fileToCopy, bool forced = false, string? filename = null, string backupCode = "") {
            if (!File.Exists(fileToCopy)) {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Selected beatmap file does not exist! Check if you have the correct file selected in the current beatmap field, or try re-selecting the beatmap file.");
                await box.ShowAsync();
				return false;
            }

            string destinationDirectory = SettingsManager.GetBackupsPath();
            if (!Directory.Exists(destinationDirectory)) {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Backups folder does not exist! Check in the Preferences if the path to your backups folder is correct and the folder exists.");
				await box.ShowAsync();
                return false;
            }

            try {
                if (!SettingsManager.GetMakeBackups() && !forced)
                    return false;

                if (string.IsNullOrEmpty(filename))
                    filename = Path.GetFileName(fileToCopy);

                if (SettingsManager.Settings.UseEditorReader && Path.GetExtension(fileToCopy) == ".osu") {
                    fileToCopy = GetNewestVersionPath(fileToCopy);
                }

                DateTime now = DateTime.Now;

                var name = now.ToString("yyyy-MM-dd HH-mm-ss") + "_" + backupCode + "__" + filename;

                File.Copy(fileToCopy,
                    Path.Combine(destinationDirectory, name),
                    true);

                // Delete old files if the number of backup files are over the limit
                foreach (var fi in new DirectoryInfo(SettingsManager.GetBackupsPath()).GetFiles().OrderByDescending(x => x.CreationTime).Skip(SettingsManager.Settings.MaxBackupFiles))
                    fi.Delete();

                return true;
            } catch (Exception ex) {
                await ex.Show();
                return false;
            }
        }

        public static async Task<bool> SaveMapBackup(string[] filesToCopy, bool forced = false, string backupCode = "") {
            bool result = true;
            foreach (string fileToCopy in filesToCopy) {
                result = await SaveMapBackup(fileToCopy, forced, backupCode: backupCode);
                if (!result)
                    break;
            }
            return result;
        }

        /// <summary>
        /// Copies a backup to replace a beatmap at the destination path.
        /// </summary>
        /// <param name="backupPath">Path to the backup map.</param>
        /// <param name="destination">Path to the destination map.</param>
        /// <param name="allowDifferentFilename">If false, this method throws an exception when the backup and the destination have mismatching beatmap metadata.</param>
        public static void LoadMapBackup(string backupPath, string destination, bool allowDifferentFilename = false) {
            var backupEditor = new BeatmapEditor(backupPath);
            var destinationEditor = new BeatmapEditor(destination);

            var backupFilename = backupEditor.Beatmap.GetFileName();
            var destinationFilename = destinationEditor.Beatmap.GetFileName();

            if (!allowDifferentFilename && !string.Equals(backupFilename, destinationFilename)) {
                throw new BeatmapIncompatibleException($"The backup and the destination beatmap have mismatching metadata.\n{backupFilename}\n{destinationFilename}");
            }

            File.Copy(backupPath, destination, true);
        }

        public static async void QuickUndo() {
            try {
                var path = await IOHelper.GetCurrentBeatmap();
                var backupFile = new DirectoryInfo(SettingsManager.GetBackupsPath()).GetFiles().OrderByDescending(x => x.CreationTime).FirstOrDefault();
                if (backupFile != null) {
                    try {
                        LoadMapBackup(backupFile.FullName, path, false);
                    } catch (BeatmapIncompatibleException ex) {
                        await ex.Show();
						var backupBox = MessageBoxManager.GetMessageBoxStandard("Load backup", "Do you want to load the backup anyways?",
							ButtonEnum.YesNo);
						var result = await backupBox.ShowAsync();
                        if (result == ButtonResult.Yes) {
                            LoadMapBackup(backupFile.FullName, path, true);
                        } else {
                            return;
                        }
                    }
                    //Task.Factory.StartNew(() => MainWindow.MessageQueue.Enqueue("Backup successfully loaded!"));
					var box = MessageBoxManager.GetMessageBoxStandard("","Backup successfully loaded!");
					await box.ShowAsync();

                    // if (SettingsManager.Settings.AutoReload) {
                    //     ListenerManager.ForceReloadEditor();
                    // }
                }
            } catch (Exception ex) {
                await ex.Show();
            }
        }

        /// <summary>
        /// Generates a temp file with the newest version of specified map and returns the path to that temp file.
        /// </summary>
        /// <param name="mapPath"></param>
        /// <returns></returns>
        private static string GetNewestVersionPath(string mapPath) {
			throw new NotImplementedException("Editor Reador is only in the original Mapping Tools (exclusive to Windows)!");
            // var editor = EditorReaderStuff.GetNewestVersionOrNot(mapPath);

            // // Save temp version
            // var tempPath = Path.Combine(MainWindow.AppDataPath, "temp.osu");

            // Editor.SaveFile(tempPath, editor.Beatmap.GetLines());

            // return tempPath;
        }
    }
}