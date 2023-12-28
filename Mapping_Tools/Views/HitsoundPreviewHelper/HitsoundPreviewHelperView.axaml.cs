using Avalonia_Mapping_Tools.ViewModels;
using Mapping_Tools.Classes.SystemTools;
using Mapping_Tools.Views;

namespace Avalonia_Mapping_Tools.Views;
public partial class HitsoundPreviewHelperView : SingleRunMappingTool, ISavable<HitsoundPreviewHelperViewModel>
{
	public HitsoundPreviewHelperView()
	{
		DataContext = new HitsoundPreviewHelperViewModel();
		InitializeComponent();
	}

    public string AutoSavePath => Program.configPath + "/hspreviewproject.json";

    public string DefaultSaveFolder => Program.configPath + "/Hitsound Preview Projects";

    public HitsoundPreviewHelperViewModel GetSaveData()
    {
        return (HitsoundPreviewHelperViewModel) DataContext!;
    }

    public void SetSaveData(HitsoundPreviewHelperViewModel saveData) => DataContext = saveData;

}