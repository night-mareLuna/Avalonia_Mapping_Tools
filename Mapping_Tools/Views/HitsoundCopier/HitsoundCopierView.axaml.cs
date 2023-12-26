using Avalonia.Controls;
using Avalonia_Mapping_Tools.ViewModels;
using Mapping_Tools.Classes.SystemTools;

namespace Avalonia_Mapping_Tools.Views;
public partial class HitsoundCopierView : UserControl, ISavable<HitsoundCopierViewModel>
{
	public HitsoundCopierView()
	{
		DataContext = new HitsoundCopierViewModel();
		InitializeComponent();
	}

	public HitsoundCopierViewModel ViewModel => (HitsoundCopierViewModel) DataContext;

    public HitsoundCopierViewModel GetSaveData()
    {
        return ViewModel;
    }

    public void SetSaveData(HitsoundCopierViewModel saveData) => DataContext = saveData;

    public string AutoSavePath => Program.configPath + "/hitsoundcopierproject.json";

    public string DefaultSaveFolder => Program.configPath + "/Hitsound Copier Projects";
}