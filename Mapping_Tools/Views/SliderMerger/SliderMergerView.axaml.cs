using Avalonia_Mapping_Tools.ViewModels;
using Mapping_Tools.Classes.SystemTools;
using Mapping_Tools.Views;

namespace Avalonia_Mapping_Tools.Views;
public partial class SliderMergerView : SingleRunMappingTool, ISavable<SliderMergerViewModel>
{
	public SliderMergerView()
	{
		DataContext = new SliderMergerViewModel();
		InitializeComponent();
	}

    public string AutoSavePath => Program.configPath + "/slidermergerproject.json";

    public string DefaultSaveFolder => Program.configPath + "/Slider Merger Projects";

    public SliderMergerViewModel GetSaveData()
    {
        return (SliderMergerViewModel)DataContext!;
    }

    public void SetSaveData(SliderMergerViewModel saveData)
    {
        DataContext = saveData;
    }
}