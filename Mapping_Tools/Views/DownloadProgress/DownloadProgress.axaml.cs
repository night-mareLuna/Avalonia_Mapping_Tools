using System.Threading;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia_Mapping_Tools.ViewModels;

namespace Avalonia_Mapping_Tools.Views;
public partial class DownloadProgress : Window
{
	public DownloadProgress(string info, CancellationTokenSource cts)
	{
		InitializeComponent();
		DataContext = new DownloadProgressViewModel(info, cts);
		Width = 400;
		Height = 300;
		CanResize = false;
	}

	private void UserOk(object obj, RoutedEventArgs args) => Close();

	public DownloadProgressViewModel ViewModel()
	{
		return (DownloadProgressViewModel)DataContext!;
	}
}