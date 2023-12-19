using CommunityToolkit.Mvvm.ComponentModel;

namespace Avalonia_Mapping_Tools.ViewModels;

public partial class MapCleanerViewModel : ViewModelBase
{
	[ObservableProperty] private string _MapCleaner;
	public MapCleanerViewModel()
	{
		MapCleaner = "Map Cleaner, waow!";
	}
}