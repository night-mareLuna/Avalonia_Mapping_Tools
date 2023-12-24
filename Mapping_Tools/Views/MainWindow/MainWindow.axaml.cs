using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia_Mapping_Tools.ViewModels;
using Mapping_Tools.Classes;
using Mapping_Tools.Classes.Exceptions;
using Mapping_Tools.Classes.SystemTools;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Avalonia_Mapping_Tools.Views;

public partial class MainWindow : Window
{
	private static MainWindow? Me;
    public MainWindow()
    {
		Me = this;
		Setup();
        InitializeComponent();
		DataContext = new MainWindowViewModel();
    }

	private static void Setup()
	{
		try
		{
			Directory.CreateDirectory(Program.configPath + "/Backups");
			Directory.CreateDirectory(Program.configPath + "/Exports");
			SettingsManager.LoadConfig();
		}
		catch (Exception e)
		{
			e.Show();
		}
		SetTheme();
	}

	private static void SetTheme()
	{
		bool? theme;
		try
		{
			theme = SettingsManager.GetTheme();
			if(theme is null) return;
			Application.Current!.RequestedThemeVariant = (bool)theme ?
				ThemeVariant.Dark : ThemeVariant.Light;
		}
		catch(Exception e)
		{
			e.Show();
		}
	}

	private async void OpenBeatmap(object obj, RoutedEventArgs args)
	{
		var storage = StorageProvider;
		IStorageFolder? songFolder = await storage.TryGetFolderFromPathAsync(SettingsManager.GetSongsPath());

		var file = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
		{
			Title = "Select osu! beatmap",
			AllowMultiple = true,
			FileTypeFilter = new[] { OsuFile },
			SuggestedStartLocation = songFolder
		});

		if(file.Count > 0)
		{
			string[] maps = new string[file.Count];
			for(int i = 0; i < file.Count; i++)
				maps[i] = (await file[i].SaveBookmarkAsync())!;
			MainWindowViewModel.SetCurrentMaps(maps!);
			SettingsManager.AddRecentMap(maps, DateTime.Now);
		}
	}

	private async void SaveBackup(object obj, RoutedEventArgs args)
	{
		try
		{
			var paths = MainWindowViewModel.GetCurrentMaps();
			var result = await Task.Run(() => BackupManager.SaveMapBackup(paths, true, "UB"));
			if(result)
			{
				var box = MessageBoxManager.GetMessageBoxStandard("Backup Success!",
					$"Beatmap{( paths.Length == 1 ? "" : "s" )} successfully copied!",
					ButtonEnum.Ok);
				box.ShowAsync();
			}
		}
		catch(Exception ex)
		{
			ex.Show();
		}
	}

	private async void LoadBackup(object sender, RoutedEventArgs e)
	{
        try {
            var paths = MainWindowViewModel.GetCurrentMaps();
            if (paths.Length > 1) {
                throw new Exception($"Can't load backup into multiple beatmaps. You currently have {paths.Length} beatmaps selected.");
            }
            var backupPaths = await IOHelper.BeatmapFileDialog(SettingsManager.GetBackupsPath(), false);
			if(backupPaths[0] == string.Empty) return;
			
            if (backupPaths.Length == 1) {
                try {
                    await Task.Run(() => BackupManager.LoadMapBackup(backupPaths[0], paths[0], false));
                } catch (BeatmapIncompatibleException ex) {
                    var exResult = await ex.Show();
                    if (exResult == ButtonResult.Cancel) {
                        return;
                    }

					var resultBox = MessageBoxManager.GetMessageBoxStandard("Load Backup",
						"Do you want to load the backup anyways?",
						ButtonEnum.YesNo);
					var result = await resultBox.ShowAsync();
					

                    if (result == ButtonResult.Yes) {
                        await Task.Run(() => BackupManager.LoadMapBackup(backupPaths[0], paths[0], true));
                    } else {
                        return;
                    }
                }
                var box = MessageBoxManager.GetMessageBoxStandard("Restore success",
					"Backup successfully loaded!",
					ButtonEnum.Ok);
				box.ShowAsync();
            }
        } catch (Exception ex) {
            ex.Show();
        }
    }

    protected override async void OnClosing(WindowClosingEventArgs e)
    {
		await SettingsManager.WriteToJson();
        base.OnClosing(e);
    }

	public static IStorageProvider Storage()
	{
		return Me!.StorageProvider;
	}

    private static FilePickerFileType OsuFile { get; } = new("osu! beatmap file")
	{
		Patterns = new[] { "*.osu" }
	};
}