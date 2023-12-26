using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Mapping_Tools.Classes.SystemTools;

namespace Avalonia_Mapping_Tools.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
	[ObservableProperty] private int _CurrentItem = 0;
	[ObservableProperty] private ViewModelBase _CurrentView;
	[ObservableProperty] private ObservableCollection<string> _ToolsList;
	[ObservableProperty] private bool _OpenPanel = true;
	[ObservableProperty] private string _DisplayCurrentMaps = "No Beatmap Selected!";
	[ObservableProperty] private string _DisplayCurrentMapsTooltip = "No Beatmap Selected!";
	[ObservableProperty] private string _TotalSelectedMaps = "(0) maps total";
	private static string[] CurrentMaps = [];
	private static MainWindowViewModel? Me;

	private enum Tools
	{
		Preferences = 0,
		MapCleaner = 1,
		HitsoundCopier = 2
	}

	public MainWindowViewModel()
	{
		Me = this;
		DisplayCurrentMap();
		CurrentView = new PreferencesViewModel();
		ToolsList = new ObservableCollection<string>(
			["Preferences",
			"Map Cleaner",
			"Hitsound Copier"]);
	}

	private static void DisplayCurrentMap()
	{
		try
		{
			string[] fromJson = SettingsManager.GetLatestCurrentMaps();
			SetCurrentMaps(fromJson);
		}
		catch(Exception e)
		{
			Console.WriteLine(e.Message);
		}
	}

	public static void SetCurrentMaps(string[] maps)
	{
		if(!string.IsNullOrWhiteSpace(maps[0]))
		{
			Me!.DisplayCurrentMaps = "";
			for(int i = 0; i < maps.Length; i++)
			{
				if(i != 0)	Me!.DisplayCurrentMaps += " | ";
				Me!.DisplayCurrentMaps += maps[i].Split('/')[^1];
			}
			Me!.TotalSelectedMaps = maps.Length == 1 ?
				$"({maps.Length}) map total" : $"({maps.Length}) maps total";
		}
		CurrentMaps = maps;
	}
	public static string[] GetCurrentMaps()
	{
		return CurrentMaps;
	}

	public void PanelView() => OpenPanel = !OpenPanel;

	private void UpdateView(int value)
	{
		switch (value)
		{
			case (int)Tools.MapCleaner:
				CurrentView = new MapCleanerViewModel();
				break;
			case (int)Tools.Preferences:
				CurrentView = new PreferencesViewModel();
				break;
			case (int)Tools.HitsoundCopier:
				CurrentView = new HitsoundCopierViewModel();
				break;
		}
	}

	partial void OnCurrentItemChanged(int oldValue, int newValue)
	{
		UpdateView(newValue);
	}

    partial void OnDisplayCurrentMapsChanged(string value)
    {
        DisplayCurrentMapsTooltip = DisplayCurrentMaps.Replace(" | ", Environment.NewLine);
    }
}
