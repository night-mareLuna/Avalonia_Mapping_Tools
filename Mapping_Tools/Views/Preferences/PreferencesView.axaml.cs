using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Mapping_Tools.Classes.SystemTools;

namespace Avalonia_Mapping_Tools.Views;
public partial class PreferencesView : UserControl
{
	public PreferencesView()
	{
		DataContext = SettingsManager.Settings;
		InitializeComponent();
		MaxWidth = MainWindow.GetScreenSize()[0] - 10;
	}

	private void SetTheme(object obj, RoutedEventArgs args)
	{
		bool? darkTheme = (obj as ToggleSwitch)!.IsChecked;
		if(darkTheme is null)
			Application.Current!.RequestedThemeVariant = ThemeVariant.Default;
		else
			Application.Current!.RequestedThemeVariant = (bool)darkTheme ?
				ThemeVariant.Dark : ThemeVariant.Light;
	}

	public async void SelectFolder(object obj, RoutedEventArgs args)
	{
		string buttonName = (obj as Control)!.Name!;

		string? strLastFolder;
		string pickerTitle;
		string? folder;
		switch (buttonName)
		{
			case "osuFolder":
				strLastFolder = SettingsManager.GetOsuPath();
				pickerTitle = "Select osu! folder";
				folder = await SelectFolder(strLastFolder, pickerTitle);
				SettingsManager.Settings.OsuPath = folder ?? "";
				break;
			case "osuSongsFolder":
				strLastFolder = SettingsManager.GetSongsPath();
				pickerTitle = "Select osu! songs folder";
				folder = await SelectFolder(strLastFolder, pickerTitle);
				SettingsManager.Settings.SongsPath = folder ?? "";
				break;
			case "osuConfigFile":
				strLastFolder = SettingsManager.GetOsuPath();
				pickerTitle = "Select osu! user config file";
				folder = await SelectFile(strLastFolder, pickerTitle);
				SettingsManager.Settings.OsuConfigPath = folder ?? "";
				break;
			case "backupsFolder":
				strLastFolder = SettingsManager.GetBackupsPath();
				pickerTitle = "Select Mapping Tools backups folder";
				folder = await SelectFolder(strLastFolder, pickerTitle);
				SettingsManager.Settings.BackupsPath = folder ?? "";
				break;
			default:
				break;
		}
	}

	private async Task<string?> SelectFolder(string strLastFolder, string pickerTitle)
	{
		var storage = TopLevel.GetTopLevel(this)!.StorageProvider;
		IStorageFolder? lastFolder = strLastFolder != null ?
			await storage.TryGetFolderFromPathAsync(strLastFolder) : null;
		
		var path = await storage.OpenFolderPickerAsync(new FolderPickerOpenOptions
		{
			Title = pickerTitle,
			AllowMultiple = false,
			SuggestedStartLocation = lastFolder
		});

		if(path.Count > 0)
			return await path[0].SaveBookmarkAsync();
		return null;
	}

	private async Task<string?> SelectFile(string strLastFolder, string pickerTitle)
	{
		var storage = TopLevel.GetTopLevel(this)!.StorageProvider;
		IStorageFolder? lastFolder = strLastFolder != null ?
			await storage.TryGetFolderFromPathAsync(strLastFolder) : null;

		var file = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
		{
			Title = pickerTitle,
			AllowMultiple = false,
			SuggestedStartLocation = lastFolder
		});

		if(file.Count > 0)
			return await file[0].SaveBookmarkAsync();
		return null;
	}

	protected override async void OnUnloaded(RoutedEventArgs e)
    {
		await SettingsManager.WriteToJson();
        base.OnUnloaded(e);
    }
}