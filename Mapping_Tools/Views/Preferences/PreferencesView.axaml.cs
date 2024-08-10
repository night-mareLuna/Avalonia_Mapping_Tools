using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia_Mapping_Tools.ViewModels;
using Mapping_Tools.Classes.SystemTools;
using Mapping_Tools.Classes.ToolHelpers;

namespace Avalonia_Mapping_Tools.Views;
public partial class PreferencesView : UserControl
{
	public PreferencesView()
	{
		DataContext = SettingsManager.Settings;
		InitializeComponent();
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

	private async void InstallGosu(object obj, RoutedEventArgs args)
	{
		await SettingsManager.DownloadGosuMemory();
		if(SettingsManager.Settings.RunGosumemory)
			GosumemoryReader.StartGosumemory();
	}

	private void ChangeUsingGosu(object obj, RoutedEventArgs args)
	{
		bool checkBoxChecked = (bool) (obj as CheckBox)!.IsChecked!;
		MainWindowViewModel.ChangeUsingGosu(checkBoxChecked);
		if(checkBoxChecked)
		{
			if(SettingsManager.Settings.RunGosumemory)
				GosumemoryReader.StartGosumemory();
		}
		else
			GosumemoryReader.Stop();
	}

	private async void ChangeRunGosu(object obj, RoutedEventArgs args)
	{
		bool checkBoxChecked = (bool) (obj as CheckBox)!.IsChecked!;
		if(checkBoxChecked)
		{
			if(SettingsManager.Settings.GosumemoryPath == "none")
				await SettingsManager.DownloadGosuMemory();
			
			GosumemoryReader.StartGosumemory();
		}
		else
			GosumemoryReader.Stop();
	}

	private async void SelectFolder(object obj, RoutedEventArgs args)
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
				if(string.IsNullOrEmpty(folder)) break;
				SettingsManager.Settings.OsuPath = folder;
				break;
			case "osuSongsFolder":
				strLastFolder = SettingsManager.GetSongsPath();
				pickerTitle = "Select osu! songs folder";
				folder = await SelectFolder(strLastFolder, pickerTitle);
				if(string.IsNullOrEmpty(folder)) break;
				SettingsManager.Settings.SongsPath = folder;
				break;
			case "osuConfigFile":
				strLastFolder = SettingsManager.GetOsuPath();
				pickerTitle = "Select osu! user config file";
				folder = await SelectFile(strLastFolder, pickerTitle);
				if(string.IsNullOrEmpty(folder)) break;
				if(folder == "") break;
				SettingsManager.Settings.OsuConfigPath = folder;
				break;
			case "backupsFolder":
				strLastFolder = SettingsManager.GetBackupsPath();
				pickerTitle = "Select Mapping Tools backups folder";
				folder = await SelectFolder(strLastFolder, pickerTitle);
				if(string.IsNullOrEmpty(folder)) break;
				SettingsManager.Settings.BackupsPath = folder;
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