using Newtonsoft.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using Mapping_Tools.Classes.Tools.MapCleanerStuff;

namespace Avalonia_Mapping_Tools.ViewModels;

public partial class MapCleanerViewModel : ViewModelBase
{
	[ObservableProperty] private MapCleanerArgs _MapCleanerArgs = MapCleanerArgs.BasicResnap;
	[ObservableProperty] [property: JsonIgnore] private int _Progress = 0;
	[JsonIgnore] public string[] Paths { get; set; }
	[JsonIgnore] public bool Quick { get; set; }
	private static MapCleanerViewModel? Me;
	public MapCleanerViewModel()
	{
		Me = this;
		Paths = [];
		Quick = false;
	}

	public static void SetProgress(int prog) => Me!.Progress = prog;

}