using Avalonia.Controls;
using Avalonia_Mapping_Tools.ViewModels;

namespace Avalonia_Mapping_Tools.Views;
public partial class About : Window
{
	public About()
	{
		InitializeComponent();
		DataContext = new AboutViewModel();
		Width = 400;
		Height = 300;
		CanResize = false;
	}
}