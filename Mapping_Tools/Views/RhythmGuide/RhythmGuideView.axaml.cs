using System.ComponentModel;
using System.IO;
using Avalonia.Interactivity;
using Avalonia_Mapping_Tools.ViewModels;
using Mapping_Tools.Classes.SystemTools;
using Mapping_Tools.Classes.Tools;
using Mapping_Tools.Views;

namespace Avalonia_Mapping_Tools.Views;
public partial class RhythmGuideView : SingleRunMappingTool, ISavable<RhythmGuideViewModel>
{
	public RhythmGuideView()
	{
		DataContext = new RhythmGuideViewModel();

		if(File.Exists(AutoSavePath))
			ProjectManager.LoadProject(this, message: false);
		else
			ProjectManager.SaveProject(this, AutoSavePath);
		
		InitializeComponent();
	}

	public RhythmGuideViewModel ViewModel => (RhythmGuideViewModel) DataContext!;

	public RhythmGuideViewModel GetSaveData()
	{
		return ViewModel;
	}

	private async void Start_Click(object obj, RoutedEventArgs args)
	{
		if(BackgroundWorker.IsBusy) return;

		foreach (var fileToCopy in ViewModel.GuideGeneratorArgs.Paths)
			await BackupManager.SaveMapBackup(fileToCopy);

		BackgroundWorker.RunWorkerAsync(ViewModel.GuideGeneratorArgs);
	}

    protected override void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
    {
        var bgw = sender as BackgroundWorker;
		e.Result = GenerateRhythmGuide((RhythmGuide.RhythmGuideGeneratorArgs) e.Argument!, bgw!, e);
    }

    protected override void BackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
    {
        base.BackgroundWorker_ProgressChanged(sender, e);
		RhythmGuideViewModel.UpdateProgress(Progress);
    }

    private static string GenerateRhythmGuide(RhythmGuide.RhythmGuideGeneratorArgs args, BackgroundWorker worker, DoWorkEventArgs _)
	{
		RhythmGuide.GenerateRhythmGuide(args);

		if(worker != null && worker.WorkerReportsProgress)
			worker.ReportProgress(100);

		return args.ExportMode == RhythmGuide.ExportMode.NewMap ? "" : "Done!";
	}

    public void SetSaveData(RhythmGuideViewModel saveData)
	{
		DataContext = saveData;
	}

	public string AutoSavePath => Program.configPath + "/rhythmguideproject.json";

	public string DefaultSaveFolder => Program.configPath + "/Rhythm Guide Projects";

	protected override void OnUnloaded(RoutedEventArgs e)
    {
		ProjectManager.SaveProject(this, AutoSavePath);
        base.OnUnloaded(e);
    }
}