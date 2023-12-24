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
		var storage = TopLevel.GetTopLevel(this)!.StorageProvider;
		string buttonName = (obj as Control)!.Name!;
		int button = buttonName == "osuSongsFolder" ? 0 : 1;

		string? strLastFolder;
		string pickerTitle;
		switch(button)
		{
			case 0:
				strLastFolder = SettingsManager.GetSongsPath();
				pickerTitle = "Select osu! song folder";
				break;
			case 1:
				strLastFolder = SettingsManager.GetBackupsPath();
				pickerTitle = "Select Mapping Tools backups folder";
				break;
			default:
				strLastFolder = null;
				pickerTitle = "";
				break;
		}
		IStorageFolder? lastFolder = strLastFolder != null ?
			await storage.TryGetFolderFromPathAsync(strLastFolder) : null;
		
		var path = await storage.OpenFolderPickerAsync(new FolderPickerOpenOptions
		{
			Title = pickerTitle,
			AllowMultiple = false,
			SuggestedStartLocation = lastFolder
		});

		if(path.Count > 0)
		{
			switch(button)
			{
				case 0:
					SettingsManager.Settings.SongsPath = (await path[0].SaveBookmarkAsync())!;
					break;
				case 1:
					SettingsManager.Settings.BackupsPath = (await path[0].SaveBookmarkAsync())!;
					break;
				default:
					break;
			}
		}
	}

	protected override async void OnUnloaded(RoutedEventArgs e)
    {
		await SettingsManager.WriteToJson();
        base.OnUnloaded(e);
    }
}