using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Mapping_Tools.Classes;
using Mapping_Tools.Classes.SystemTools;

namespace Avalonia_Mapping_Tools.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
	[ObservableProperty] private object _CurrentItem = "Preferences";
	[ObservableProperty] private ViewModelBase? _CurrentView;
	[ObservableProperty] private ObservableCollection<string> _ToolsList;
	[ObservableProperty] private bool _OpenPanel = true;
	[ObservableProperty] private string _DisplayCurrentMaps = "No Beatmap Selected!";
	[ObservableProperty] private string _DisplayCurrentMapsTooltip = "No Beatmap Selected!";
	[ObservableProperty] private string _TotalSelectedMaps = "(0) maps total";
	private static string[] CurrentMaps = [];
	private static MainWindowViewModel? Me;

	public MainWindowViewModel()
	{
		Me = this;
		DisplayCurrentMap();
		UpdateView((CurrentItem as string)!);
		string[] defaultTools = ["Preferences"];
		string[] mappingTools = ["Map Cleaner",
			"Hitsound Copier",
			"Hitsound Preview Helper"];
		string[] allTools = [.. defaultTools, .. mappingTools.OrderBy(d => d).ToArray()];
		
		ToolsList = new ObservableCollection<string>(allTools);
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
			e.Show();
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

	private void UpdateView(string value)
	{
		switch (value)
		{
			case "Map Cleaner":
				CurrentView = new MapCleanerViewModel();
				break;
			case "Preferences":
				CurrentView = new PreferencesViewModel();
				break;
			case "Hitsound Copier":
				CurrentView = new HitsoundCopierViewModel();
				break;
			case "Hitsound Preview Helper":
				CurrentView = new HitsoundPreviewHelperViewModel();
				break;
		}
	}

	partial void OnCurrentItemChanged(object value)
	{
		string toolName = (value as string)!;
		UpdateView(toolName);
	}

    partial void OnDisplayCurrentMapsChanged(string value)
    {
        DisplayCurrentMapsTooltip = DisplayCurrentMaps.Replace(" | ", Environment.NewLine);
    }
}
