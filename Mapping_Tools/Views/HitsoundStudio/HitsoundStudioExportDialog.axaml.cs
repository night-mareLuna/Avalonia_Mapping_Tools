using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia_Mapping_Tools.ViewModels;
using Mapping_Tools.Classes.SystemTools;

namespace Avalonia_Mapping_Tools.Views.HitsoundStudio;
public partial class HitsoundStudioExportDialog : Window
{
	public HitsoundStudioViewModel Settings => (HitsoundStudioViewModel) DataContext!;
	public bool result = false;
	public HitsoundStudioExportDialog(HitsoundStudioViewModel settings)
	{
		DataContext = settings;
		InitializeComponent();
	}

	private async void ExportFolderBrowseButton_OnClick(object sender, RoutedEventArgs e) {
        string path = await IOHelper.FolderDialog();
        if (!string.IsNullOrWhiteSpace(path)) {
            ExportFolderBox.Text = path;
            Settings.ExportFolder = path;
        }
    }

	private void SetResult(object obj, RoutedEventArgs args)
	{
        result = (obj as Button)!.Name! switch
        {
            "Accept" => true,
            "Cancel" => false,
            _ => false
        };
		Close();
    }
}