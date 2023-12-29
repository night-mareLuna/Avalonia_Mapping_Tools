using System;
using Avalonia.Controls;
using Avalonia_Mapping_Tools.ViewModels;
using Mapping_Tools.Classes.SystemTools;
using Mapping_Tools.Views;

namespace Avalonia_Mapping_Tools.Views;
public partial class HitsoundStudioView : SingleRunMappingTool, ISavable<HitsoundStudioViewModel>, IHaveExtraProjectMenuItems
{
	public HitsoundStudioView()
	{
		DataContext = new HitsoundPreviewHelperViewModel();
		InitializeComponent();
		Verbose = true;
	}

    public string AutoSavePath => Program.configPath + "/hsstudioproject.json";

    public string DefaultSaveFolder => Program.configPath + "/Hitsound Studio Projects";

    public MenuItem[] GetMenuItems()
    {
        throw new NotImplementedException();
    }

    public HitsoundStudioViewModel GetSaveData()
    {
        return (HitsoundStudioViewModel) DataContext!;
    }

    public void SetSaveData(HitsoundStudioViewModel saveData)
    {
        throw new NotImplementedException();
    }
}