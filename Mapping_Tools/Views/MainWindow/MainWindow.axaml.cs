using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia_Mapping_Tools.Models;
using Avalonia_Mapping_Tools.ViewModels;

namespace Avalonia_Mapping_Tools.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
		JsonWriter.CreateEmpty();
		SetTheme();
        InitializeComponent();
		DataContext = new MainWindowViewModel();
    }

	private static async void SetTheme()
	{
		bool? theme;
		try
		{
			theme = await JsonWriter.GetTheme();
			if(theme is null) return;
			Application.Current!.RequestedThemeVariant = (bool)theme ?
				ThemeVariant.Dark : ThemeVariant.Light;
		}
		catch(Exception e)
		{
			Console.WriteLine(e.Message);
		}

	}

	private async void OpenBeatmap(object obj, RoutedEventArgs args)
	{
		var storage = StorageProvider;
		string? strSongFolder = await JsonWriter.GetSong();
		IStorageFolder? songFolder = strSongFolder!=null ?
			await storage.TryGetFolderFromPathAsync(new Uri(strSongFolder)) : null;

		var file = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
		{
			Title = "Select osu! beatmap",
			AllowMultiple = false,
			FileTypeFilter = new[] { OsuFile },
			SuggestedStartLocation = songFolder
		});

		if(file.Count > 0)
		{
			string map = file[0].Name;
			MainWindowViewModel.SetCurrentmap(map);
			JsonWriter.SetCurrentMap(await file[0].SaveBookmarkAsync());
		}
	}

	private static FilePickerFileType OsuFile { get; } = new("osu! beatmap file")
	{
		Patterns = new[] { "*.osu" }
	};
}