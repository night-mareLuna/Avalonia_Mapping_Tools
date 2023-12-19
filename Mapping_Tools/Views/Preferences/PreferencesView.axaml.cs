using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia_Mapping_Tools.Models;
using Avalonia_Mapping_Tools.ViewModels;

namespace Avalonia_Mapping_Tools.Views;
public partial class PreferencesView : UserControl
{
	public PreferencesView()
	{
		DataContext = new PreferencesViewModel();
		InitializeComponent();
	}

	public async void SelectFolder(object obj, RoutedEventArgs args)
	{
		var storage = TopLevel.GetTopLevel(this)!.StorageProvider;
		string buttonName = (obj as Control)!.Name!;
		int button = buttonName == "osuSongsFolder" ? 0 : 1;

		string? strLastFolder;
		string pickerTitle;
		switch(button)
		{
			case 0:
				strLastFolder = await JsonWriter.GetSong();
				pickerTitle = "Select osu! song folder";
				break;
			case 1:
				strLastFolder = await JsonWriter.GetBackup();
				pickerTitle = "Select Mapping Tools backups folder";
				break;
			default:
				strLastFolder = null;
				pickerTitle = "";
				break;
		}
		IStorageFolder? lastFolder = strLastFolder != null ?
			await storage.TryGetFolderFromPathAsync(new Uri(strLastFolder)) : null;
		
		var path = await storage.OpenFolderPickerAsync(new FolderPickerOpenOptions
		{
		
			Title = pickerTitle,
			AllowMultiple = false,
			SuggestedStartLocation = lastFolder
		});

		if(path.Count > 0)
#pragma warning disable CS8604 // Possible null reference argument.
			PreferencesViewModel.SetFolder(button, await path[0].SaveBookmarkAsync());
#pragma warning restore CS8604 // Possible null reference argument.
	}
}