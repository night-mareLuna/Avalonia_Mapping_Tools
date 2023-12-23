using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia_Mapping_Tools.ViewModels;
using Mapping_Tools.Classes;
using Mapping_Tools.Classes.SystemTools;

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