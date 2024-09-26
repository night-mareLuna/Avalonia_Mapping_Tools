using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia_Mapping_Tools.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using Mapping_Tools.Classes;
using Mapping_Tools.Classes.SystemTools;

namespace Avalonia_Mapping_Tools.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
	[ObservableProperty] private object _CurrentItem = "Preferences";
	[ObservableProperty] private ViewModelBase? _CurrentView;
	[ObservableProperty] private ObservableCollection<object> _ToolsList;
	[ObservableProperty] private bool _OpenPanel = true;
	[ObservableProperty] private string _DisplayCurrentMaps = "No Beatmap Selected!";
	[ObservableProperty] private string _DisplayCurrentMapsTooltip = "No Beatmap Selected!";
	[ObservableProperty] private string _TotalSelectedMaps = "(0) maps total";
	[ObservableProperty] private bool _UseGosumemory;
	private static string[] CurrentMaps = [];
	private static MainWindowViewModel? Me;

	public MainWindowViewModel()
	{
		Me = this;
		DisplayCurrentMap();
		UpdateView((CurrentItem as string)!);
		string[] defaultTools = ["Preferences"];
		 
        ListBoxItem separator = new()
        {
            Focusable = false,
			IsHitTestVisible = false,
		 	Content = new Separator()
        };

        string[] mappingTools = ["Map Cleaner",
			"Hitsound Copier",
			"Hitsound Preview Helper",
			"Hitsound Studio",
			"Rhythm Guide",
			"Metadata Manager",
			"Slider Merger",
			"Property Transformer"];

		object[] allTools = [.. defaultTools, separator, .. mappingTools.OrderBy(d => d).ToArray()];
		ToolsList = new ObservableCollection<object>(allTools);
		UseGosumemory = SettingsManager.Settings.UseGosumemory;
	}

	public static void ChangeUsingGosu(bool useGosu) => Me!.UseGosumemory = useGosu;

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

	public async void PasteItem()
	{
		string? clipboard = await MainWindow.GetClipboard();
		if(string.IsNullOrWhiteSpace(clipboard)) return;
		string[] mapsFromClipboard = clipboard.Split('\n');

		foreach(string map in mapsFromClipboard)
		{
			if(!Path.Exists(map)) return;
			if(map.Split('.')[^1] != "osu") return;
		}

		SetCurrentMaps(mapsFromClipboard);
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

	public static void SetCurrentMaps(string map) => SetCurrentMaps([map]);

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
			case "Hitsound Studio":
				CurrentView = new HitsoundStudioViewModel();
				break;
			case "Rhythm Guide":
				CurrentView = new RhythmGuideViewModel();
				break;
			case "Metadata Manager":
				CurrentView = new MetadataManagerViewModel();
				break;
			case "Slider Merger":
				CurrentView = new SliderMergerViewModel();
				break;
			case "Property Transformer":
				CurrentView = new PropertyTransformerViewModel();
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
