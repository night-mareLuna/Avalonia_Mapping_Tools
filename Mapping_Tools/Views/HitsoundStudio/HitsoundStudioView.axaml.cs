using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia_Mapping_Tools.ViewModels;
using Mapping_Tools.Classes.SystemTools;
using Mapping_Tools.Views;

namespace Avalonia_Mapping_Tools.Views;
public partial class HitsoundStudioView : SingleRunMappingTool, ISavable<HitsoundStudioViewModel>, IHaveExtraProjectMenuItems
{
	public HitsoundStudioView()
	{
		DataContext = new HitsoundStudioViewModel();
		InitializeComponent();
		Verbose = true;
	}

	private void Start_Click(object obj, RoutedEventArgs args)
	{
		
	}

	private void Add_Click(object obj, RoutedEventArgs args)
	{

	}

	private void Delete_Click(object obj, RoutedEventArgs args)
	{

	}

	private void Raise_Click(object obj, RoutedEventArgs args)
	{

	}

	private void Lower_Click(object obj, RoutedEventArgs args)
	{
		
	}
	private void ValidateSamples_Click(object obj, RoutedEventArgs args)
	{
		
	}

	private void SelectedNameBox_TextChanged(object obj, TextChangedEventArgs args)
	{
		
	}

	private void SelectedSampleSetBox_SelectionChanged(object obj, SelectionChangedEventArgs args)
	{

	}

	private void SelectedHitsoundBox_SelectionChanged(object obj, SelectionChangedEventArgs args)
	{
		
	}

	private void SelectedSamplePathBox_TextChanged(object obj, TextChangedEventArgs args)
	{

	}

	private void SelectedSamplePathBrowse_Click(object obj, RoutedEventArgs args)
	{

	}

	private void SelectedSampleVolumeBox_TextChanged(object obj, TextChangedEventArgs args)
	{

	}

	private void SelectedSamplePanningBox_TextChanged(object obj, TextChangedEventArgs args)
	{

	}

	private void SelectedSamplePitchShiftBox_TextChanged(object obj, TextChangedEventArgs args)
	{

	}

	private void SelectedSampleBankBox_TextChanged(object obj, TextChangedEventArgs args)
	{

	}

	private void SelectedSamplePatchBox_TextChanged(object obj, TextChangedEventArgs args)
	{

	}

	private void SelectedSampleInstrumentBox_TextChanged(object obj, TextChangedEventArgs args)
	{

	}

	private void SelectedSampleKeyBox_TextChanged(object obj, TextChangedEventArgs args)
	{

	}

	private void SelectedSampleLengthBox_TextChanged(object obj, TextChangedEventArgs args)
	{

	}

	private void SelectedSampleVelocityBox_TextChanged(object obj, TextChangedEventArgs args)
	{

	}

	private void SelectedImportTypeBox_SelectionChanged(object obj, SelectionChangedEventArgs args)
	{

	}

	private void SelectedImportPathBox_TextChanged(object obj, TextChangedEventArgs args)
	{

	}

	private void SelectedImportPathLoad_Click(object obj, RoutedEventArgs args)
	{

	}

	private void SelectedImportPathBrowse_Click(object obj, RoutedEventArgs args)
	{

	}

	private void SelectedImportXCoordBox_TextChanged(object obj, TextChangedEventArgs args)
	{

	}

	private void SelectedImportYCoordBox_TextChanged(object obj, TextChangedEventArgs args)
	{

	}

	private void SelectedImportSamplePathBox_TextChanged(object obj, TextChangedEventArgs args)
	{

	}

	private void SelectedImportSamplePathBrowse_Click(object obj, RoutedEventArgs args)
	{

	}

	private void SelectedImportDiscriminateVolumesBox_OnCheck(object obj, RoutedEventArgs args)
	{
		bool? isChecked = (obj as CheckBox)!.IsChecked;
	}

	private void SelectedHitsoundImportDetectDuplicateSamplesBox_OnCheck(object obj, RoutedEventArgs args)
	{
		bool? isChecked = (obj as CheckBox)!.IsChecked;
	}

	private void SelectedImportRemoveDuplicatesBox_OnCheck(object obj, RoutedEventArgs args)
	{
		bool? isChecked = (obj as CheckBox)!.IsChecked;
	}

	private void SelectedImportRemoveDuplicatesBox_OnChecked(object obj, RoutedEventArgs args)
	{
		bool? isChecked = (obj as CheckBox)!.IsChecked;
	}

	private void SelectedImportBankBox_TextChanged(object obj, TextChangedEventArgs args)
	{

	}

	private void SelectedImportPatchBox_TextChanged(object obj, TextChangedEventArgs args)
	{

	}

	private void SelectedImportKeyBox_TextChanged(object obj, TextChangedEventArgs args)
	{

	}

	private void SelectedImportLengthBox_TextChanged(object obj, TextChangedEventArgs args)
	{

	}

	private void SelectedImportLengthRoughnessBox_TextChanged(object obj, TextChangedEventArgs args)
	{

	}

	private void SelectedImportVelocityBox_TextChanged(object obj, TextChangedEventArgs args)
	{

	}

	private void SelectedImportVelocityRoughnessBox_TextChanged(object obj, TextChangedEventArgs args)
	{

	}

	private void SelectedImportOffsetBox_TextChanged(object obj, TextChangedEventArgs args)
	{

	}

	private void ReloadFromSource_Click(object obj, RoutedEventArgs args)
	{

	}

	private void LayersList_SelectionChanged(object obj, SelectionChangedEventArgs args)
	{

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