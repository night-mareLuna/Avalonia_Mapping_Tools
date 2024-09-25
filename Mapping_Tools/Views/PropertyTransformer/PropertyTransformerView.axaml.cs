using Avalonia_Mapping_Tools.ViewModels;
using Mapping_Tools.Views;

namespace Avalonia_Mapping_Tools.Views;
public partial class PropertyTransformerView : SingleRunMappingTool
{
	public PropertyTransformerView()
	{
		DataContext = new PropertyTransformerViewModel();
		
		InitializeComponent();
	}
}