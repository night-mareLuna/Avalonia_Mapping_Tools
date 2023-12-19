using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia_Mapping_Tools.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Avalonia_Mapping_Tools.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
	[ObservableProperty] private int _CurrentItem;
	[ObservableProperty] private ViewModelBase _CurrentView;
	[ObservableProperty] private ObservableCollection<string> _ToolsList;
	[ObservableProperty] private bool _OpenPanel = true;
	[ObservableProperty] private string? _CurrentMap;
	private static MainWindowViewModel? Me;

	private enum Tools
	{
		MapCleaner = 0,
		Preferences = 1
	}

	public MainWindowViewModel()
	{
		Me = this;
		GetCurrentMap();
		CurrentView = new MapCleanerViewModel();
		ToolsList = new ObservableCollection<string>(["Map Cleaner", "Preferences"]);
	}

	private async void GetCurrentMap()
	{
		string? fromJson = null;
		try
		{
			fromJson = await JsonWriter.GetCurrentMap();
		}
		catch(Exception e)
		{
			Console.WriteLine(e.Message);
		}
		CurrentMap = fromJson!=null ? fromJson.Split('/')[^1] : "No beatmap selected!";
	}

	public static void SetCurrentmap(string map) =>	Me!.CurrentMap = map;

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
		}
	}

	partial void OnCurrentItemChanged(int oldValue, int newValue)
	{
		UpdateView(newValue);
	}
}
