using System;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Avalonia_Mapping_Tools.ViewModels;

public partial class DownloadProgressViewModel : ViewModelBase
{
    [ObservableProperty] private string infoText = "";
    [ObservableProperty] private string stringProgress = "";
    [ObservableProperty] private int progress = 0;
    [ObservableProperty] private bool downloadComplete = false;

    private readonly CancellationTokenSource cts;

	public DownloadProgressViewModel(string info, CancellationTokenSource cancellationTokenSource)
	{
        UpdateInfo(info);
        cts = cancellationTokenSource;
	}

    public void Abort()
    {
        UpdateInfo("Aborting Download!");
        cts.Cancel();
        DownloadComplete = true;
    }
    public void UpdateInfo(string info)
    {
        InfoText = info;
        Console.WriteLine(info);
    }

    public void UpdateStringProgress(double currentDownloadSize, double totalDownloadSize) => 
        StringProgress = $"{currentDownloadSize}MB | {totalDownloadSize}MB";
}