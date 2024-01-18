using Avalonia_Mapping_Tools.ViewModels;
using Mapping_Tools.Classes.SystemTools;
using Mapping_Tools.Views;

namespace Avalonia_Mapping_Tools.Views;
public partial class MetadataManagerView : SingleRunMappingTool, ISavable<MetadataManagerViewModel>
{
	public MetadataManagerView()
	{
		DataContext = new MetadataManagerViewModel();
		InitializeComponent();
	}

    public string AutoSavePath => Program.configPath + "/metadataproject.json";

    public string DefaultSaveFolder => Program.configPath + "/Metadata Manager Projects";

    public void SetSaveData(MetadataManagerViewModel saveData)
    {
		DataContext = saveData;
    }

    public MetadataManagerViewModel GetSaveData()
    {
        return (MetadataManagerViewModel)DataContext!;
    }
}