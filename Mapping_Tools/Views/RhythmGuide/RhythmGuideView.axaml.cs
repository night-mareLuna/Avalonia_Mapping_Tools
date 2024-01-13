using Avalonia.Controls;
using Avalonia_Mapping_Tools.ViewModels;

namespace Avalonia_Mapping_Tools.Views;
public partial class RhythmGuideView : UserControl
{
	public RhythmGuideView()
	{
		DataContext = new RhythmGuideViewModel();
		InitializeComponent();
	}
}