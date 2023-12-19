using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia_Mapping_Tools.ViewModels;

namespace Avalonia_Mapping_Tools.Views;
public partial class MapCleanerView : UserControl
{

	public MapCleanerView()
	{
		InitializeComponent();
		DataContext = new MapCleanerViewModel();
	}

}