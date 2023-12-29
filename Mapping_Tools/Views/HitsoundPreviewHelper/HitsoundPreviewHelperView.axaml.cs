using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Avalonia.Interactivity;
using Avalonia_Mapping_Tools.ViewModels;
using Mapping_Tools.Classes.BeatmapHelper;
using Mapping_Tools.Classes.HitsoundStuff;
using Mapping_Tools.Classes.SystemTools;
using Mapping_Tools.Classes.ToolHelpers;
using Mapping_Tools.Views;
using MsBox.Avalonia.Enums;

namespace Avalonia_Mapping_Tools.Views;
public partial class HitsoundPreviewHelperView : SingleRunMappingTool, ISavable<HitsoundPreviewHelperViewModel>
{
	public event EventHandler? RunFinished;
	public HitsoundPreviewHelperView()
	{
		DataContext = new HitsoundPreviewHelperViewModel();
		InitializeComponent();
		if(File.Exists(AutoSavePath))
			ProjectManager.LoadProject(this, message: false);
		else
			ProjectManager.SaveProject(this, AutoSavePath);

		Verbose = true;
	}

	private async void Start_Click(object obj, RoutedEventArgs args)
	{
		if(await MainWindow.ShowSaveDialog() == ButtonResult.Ok)
			RunTool(MainWindowViewModel.GetCurrentMaps(), false);
	}

	private async void RunTool(string[] paths, bool quick = false)
	{
		if(BackgroundWorker.IsBusy) return;

		ProjectManager.SaveProject(this, AutoSavePath);
		await BackupManager.SaveMapBackup(paths);

		BackgroundWorker.RunWorkerAsync(new Arguments(paths, quick,
			((HitsoundPreviewHelperViewModel) DataContext!).Items.ToList()));
	}

    protected override void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
    {
		var bgw = sender as BackgroundWorker;
		e.Result = PlaceHitsounds((Arguments) e.Argument!, bgw!, e);
    }

    protected override void BackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
    {
        base.BackgroundWorker_ProgressChanged(sender, e);
		HitsoundPreviewHelperViewModel.SetProgress(Progress);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
		ProjectManager.SaveProject(this, AutoSavePath);
        base.OnUnloaded(e);
    }

    private string PlaceHitsounds(Arguments args, BackgroundWorker worker, DoWorkEventArgs _)
    {
        if (args.Zones.Count == 0)
            return "There are no zones!";

        foreach (string path in args.Paths)
        {
            var editor = EditorReaderStuff.GetNewestVersionOrNot(path);
            Beatmap beatmap = editor.Beatmap;
            Timeline timeline = beatmap.GetTimeline();

            for (int i = 0; i < timeline.TimelineObjects.Count; i++)
            {
                var tlo = timeline.TimelineObjects[i];

                var column = args.Zones.FirstOrDefault();
                double best = double.MaxValue;
                foreach (var c in args.Zones)
                {
                    double dist = c.Distance(tlo.Origin.Pos);
                    if (dist < best)
                    {
                        best = dist;
                        column = c;
                    }
                }

                if (column == null) continue;

                tlo.Filename = column.Filename;
                tlo.SampleSet = column.SampleSet;
                tlo.AdditionSet = column.AdditionsSet;
                tlo.CustomIndex = column.CustomIndex;
                tlo.SampleVolume = 0;
                tlo.SetHitsound(column.Hitsound);
                tlo.HitsoundsToOrigin();

                UpdateProgressBar(worker, (int) (100f * i / beatmap.HitObjects.Count));
            }

            // Save the file
            editor.SaveFile();
        }

        // Do stuff
        RunFinished?.Invoke(this, new RunToolCompletedEventArgs(true, false, args.Quick));

        return args.Quick ? "" : "Done!";
    }

    private struct Arguments(string[] paths, bool quick, List<HitsoundZone> zones)
    {
        public string[] Paths = paths;
        public bool Quick = quick;
        public List<HitsoundZone> Zones = zones;
    }

    public string AutoSavePath => Program.configPath + "/hspreviewproject.json";

    public string DefaultSaveFolder => Program.configPath + "/Hitsound Preview Projects";

    public HitsoundPreviewHelperViewModel GetSaveData()
    {
        return (HitsoundPreviewHelperViewModel) DataContext!;
    }

    public void SetSaveData(HitsoundPreviewHelperViewModel saveData) => DataContext = saveData;

}