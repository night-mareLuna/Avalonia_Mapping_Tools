using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia_Mapping_Tools.ViewModels;
using System.Collections.Generic;
using Avalonia_Mapping_Tools.Views;

namespace Mapping_Tools.Classes.SystemTools {
    public class IOHelper {
        //private static readonly StructuredOsuMemoryReader pioStructuredReader = StructuredOsuMemoryReader.Instance;
        //private static readonly OsuBaseAddresses osuBaseAddresses = new();
        //private static readonly object pioReaderLock = new();
		private static readonly IStorageProvider storage = MainWindow.Storage();

		public static async Task<string> FolderDialog(string initialDirectory = "")
		{
			string? folderPath = null;
			IStorageFolder? startLocation = string.IsNullOrWhiteSpace(initialDirectory) ?
					null : await storage.TryGetFolderFromPathAsync(new Uri(initialDirectory));

            var folder = await storage.OpenFolderPickerAsync(new FolderPickerOpenOptions
			{
				AllowMultiple = false,
				SuggestedStartLocation = startLocation
			});

			if(folder.Count > 0)
				folderPath = await folder[0].SaveBookmarkAsync();
			
			folderPath ??= string.Empty;
			return folderPath;
        }

        // public static string SaveProjectDialog(string initialDirectory = "") {
        //     bool restore = initialDirectory == "";

        //     SaveFileDialog saveFileDialog1 = new SaveFileDialog {
        //         Filter = "JSON File|*.json",
        //         Title = "Save a project",
        //         InitialDirectory = initialDirectory,
        //         RestoreDirectory = restore
        //     };
        //     saveFileDialog1.ShowDialog();
        //     return saveFileDialog1.FileName;
        // }

		public static async Task<string> SaveProjectDialog(string initialDirectory = "")
		{
			var filePicker = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
			{
				Title = "Save a project",
				FileTypeChoices = new [] { JsonFilePicker },
				SuggestedStartLocation = await storage.TryGetFolderFromPathAsync(new Uri(initialDirectory))
			});

			return await ReturnFiles(filePicker);
        }

        public static async Task<string> LoadProjectDialog(string initialDirectory = "")
		{
			var filePicker = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
			{
				Title = "Open a project",
				FileTypeFilter = new [] { JsonFilePicker },
				AllowMultiple = false,
				SuggestedStartLocation = await storage.TryGetFolderFromPathAsync(new Uri(initialDirectory))
			});

			return await ReturnFiles(filePicker);
        }

        public static async Task<string> ZipFileDialog()
		{
			var filePicker = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
			{
				FileTypeChoices = new [] { ZipFilePicker }
			});

			return await ReturnFiles(filePicker);
        }

        public static async Task<string> FileDialog()
		{
			var filePicker = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
			{
				AllowMultiple = false
			});

			return await ReturnFiles(filePicker);
        }

        public static async Task<string> ConfigFileDialog(string initialDirectory = "")
		{
			var filePicker = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
			{
				FileTypeFilter = new[] { ConfigFilePicker },
				AllowMultiple = false,
				SuggestedStartLocation = await storage.TryGetFolderFromPathAsync(new Uri(initialDirectory))
			});

			return await ReturnFiles(filePicker);
        }

        public static async Task<string> MidiFileDialog()
		{
			var filePicker = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
			{
				AllowMultiple = false,
				FileTypeFilter = new[] { MidiFilePicker }
			});

			return await ReturnFiles(filePicker);
        }

        public static async Task<string> SampleFileDialog()
		{
			var filePicker = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
			{
				AllowMultiple = false,
				FileTypeFilter = new[] { SampleFilePicker, SoundFontFilePicker }
			});

			return await ReturnFiles(filePicker);
        }

        public static async Task<string[]> AudioFileDialog(bool multiselect = false)
		{
			var filePicker = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
			{
				FileTypeFilter = new[] { SampleFilePicker },
				AllowMultiple = multiselect
			});

			return await ReturnFiles(filePicker, multiselect);
        }

        public static async Task<string[]> BeatmapFileDialog(bool multiselect = false, bool restore = false)
		{
			string[]? currentMaps = MainWindowViewModel.GetCurrentMaps();
			string path = string.Empty;
			if(currentMaps is not null)
				path = currentMaps[0];

			var filePicker = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
			{
				SuggestedStartLocation = await storage.TryGetFolderFromPathAsync(new Uri(path)),
				FileTypeFilter = new[] { OsuFilePicker },
				AllowMultiple = multiselect
			});

			return await ReturnFiles(filePicker, multiselect);
        }

        public static async Task<string[]> BeatmapFileDialog(string initialDirectory, bool multiselect = false)
		{
			var filePicker = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
			{
				SuggestedStartLocation = await storage.TryGetFolderFromPathAsync(new Uri(initialDirectory)),
				FileTypeFilter = new[] { OsuFilePicker },
				AllowMultiple = multiselect
			});

			return await ReturnFiles(filePicker, multiselect);
        }

		private static async Task<string[]> ReturnFiles(IReadOnlyList<IStorageFile> filePicker, bool multiselect = false)
		{
			string?[]? files = null;
			if(filePicker.Count > 0)
			{
				files = new string[filePicker.Count];
				if(!multiselect)
				{
					for(int i = 0; i < files.Length; i++)
					{
						files[i] = await filePicker[i].SaveBookmarkAsync();
						files[i] ??= string.Empty;
					}
				}
				else
				{
					files[0] = await filePicker[0].SaveBookmarkAsync();
					files[0] ??= string.Empty;
				}
			}
			files ??= [string.Empty];
            return files!;
		}

		private static async Task<string> ReturnFiles(IReadOnlyList<IStorageFile> filePicker)
		{
			string? file = null;
			if(filePicker.Count > 0)
				file = await filePicker[0].SaveBookmarkAsync();

			file ??= string.Empty;
			return file;
		}

		private static async Task<string> ReturnFiles(IStorageFile? filePicker)
		{
			string? file = null;
			if(filePicker is not null)
				file = await filePicker.SaveBookmarkAsync();
			file ??= string.Empty;
			return file;
		}

		private static FilePickerFileType JsonFilePicker { get; } = new(".json File")
		{
			Patterns = new [] {"*.json"}
		};
		private static FilePickerFileType ZipFilePicker { get; } = new("Compressed .zip File")
		{
			Patterns = new [] {"*.zip"}
		};
		private static FilePickerFileType ConfigFilePicker { get; } = new("Config .cfg File")
		{
			Patterns = new [] {"*.cfg"}
		};
		private static FilePickerFileType MidiFilePicker { get; } = new("Midi .mid File")
		{
			Patterns = new [] {"*.mid"}
		};
		private static FilePickerFileType SampleFilePicker { get; } = new("Hitsound Sample")
		{
			Patterns = new [] {"*.wav", "*.ogg" }
		};
		private static FilePickerFileType SoundFontFilePicker { get; } = new("SoundFont .sf2 File")
		{
			Patterns = new [] { "*.sf2" }
		};
		private static FilePickerFileType OsuFilePicker { get; } = new("osu! Beatmap File")
		{
			Patterns = new [] {"*.osu", "*.osb"}
		};

        // private static T ReadClassProperty<T>(object readObj, string propName, T defaultValue = default) where T : class {
        //     lock (pioReaderLock) {
        //         if (pioStructuredReader.TryReadProperty(readObj, propName, out var readResult))
        //             return (T) readResult;
        //     }

        //     return defaultValue;
        // }

        // private static string ReadString(object readObj, string propName)
        //     => ReadClassProperty<string>(readObj, propName);

        public static string GetCurrentBeatmap() {
            string? path = null;
			string[]? currentMaps = MainWindowViewModel.GetCurrentMaps();
			if(currentMaps is not null)
				path = currentMaps[0];
			path ??= string.Empty;
			
            // try {
            //     string songs = SettingsManager.GetSongsPath();

            //     if (string.IsNullOrEmpty(songs)) {
            //         throw new Exception(
            //             @"Can't fetch current in-game beatmap, because there is no Songs path specified in Preferences.");
            //     }

            //     string folder = ReadString(osuBaseAddresses.Beatmap, nameof(CurrentBeatmap.FolderName));
            //     string filename = ReadString(osuBaseAddresses.Beatmap, nameof(CurrentBeatmap.OsuFileName));

            //     if (string.IsNullOrEmpty(folder)) {
            //         throw new Exception(@"Can't fetch the folder name of the current in-game beatmap.");
            //     }

            //     if (string.IsNullOrEmpty(filename)) {
            //         throw new Exception(@"Can't fetch the file name of the current in-game beatmap.");
            //     }

            //     path = Path.Combine(songs, folder, filename);
            // }
            // catch (Exception ex) {
            //     Console.WriteLine(ex.Message);
            //     Console.WriteLine(ex.StackTrace);
            //     try {
            //         lock (EditorReaderStuff.EditorReaderLock) {
            //             var reader = EditorReaderStuff.GetEditorReader();
            //             reader.SetProcess(EditorReaderStuff.GetOsuProcess());
            //             reader.FetchHOM();
            //             reader.FetchBeatmap();
            //             path = EditorReaderStuff.GetCurrentBeatmap(reader);
            //         }
            //     }
            //     catch (Exception ex2) {
            //         Console.WriteLine(ex2.Message);
            //         Console.WriteLine(ex2.StackTrace);
            //         throw ex;
            //     }
            // }
			
            return path;
        }

        public static string GetCurrentBeatmapOrCurrentBeatmap(bool updateCurrentBeatmap = true) {
            // try {
            //     string path = GetCurrentBeatmap();

            //     if (File.Exists(path)) {
            //         if (updateCurrentBeatmap) {
            //             MainWindow.AppWindow.SetCurrentMapsString(path);
            //         }

            //         return path;
            //     }
            // } catch (Exception ex) {
            //     Console.WriteLine(ex.Message);
            //     Console.WriteLine(ex.StackTrace);
            // }
			string[]? currentMaps = MainWindowViewModel.GetCurrentMaps();

            return currentMaps!=null ? currentMaps[0] : string.Empty;
        }

        public static void ReplaceSettingTypePaths(string path, Tuple<string, string>[] replacements) {
            if (replacements.Length == 0) return;

            var contents = File.ReadAllText(path);

            foreach (var (oldPath, newPath) in replacements) {
                contents = contents.Replace(oldPath, newPath);
            }

            File.WriteAllText(path, contents);
        }
    }
}