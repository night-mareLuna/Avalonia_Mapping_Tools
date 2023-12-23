using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Avalonia.Interactivity;
using Avalonia_Mapping_Tools.ViewModels;
using Mapping_Tools.Classes.BeatmapHelper;
using Mapping_Tools.Classes.MathUtil;
using Mapping_Tools.Classes.SystemTools;
using Mapping_Tools.Classes.SystemTools.QuickRun;
using Mapping_Tools.Classes.Tools.MapCleanerStuff;
using Mapping_Tools.Views;

namespace Avalonia_Mapping_Tools.Views;

[SmartQuickRunUsage(SmartQuickRunTargets.Always)]
public partial class MapCleanerView : SingleRunMappingTool, IQuickRun, ISavable<MapCleanerViewModel>
{
	private List<double>? timingpointsRemoved;
    private List<double>? timingpointsAdded;
    private List<double>? timingpointsChanged;
    private double endTimeMonitor;
	public event EventHandler? RunFinished;

	public MapCleanerView()
	{
		DataContext = new MapCleanerViewModel();

		if(File.Exists(AutoSavePath))
			ProjectManager.LoadProject(this, message: false);
		else
			ProjectManager.SaveProject(this, AutoSavePath);

        Verbose = true;
		InitializeComponent();
	}

	public MapCleanerViewModel ViewModel => (MapCleanerViewModel) DataContext!;

	private void Start_Click(object obj, RoutedEventArgs args)
	{
		RunTool(MainWindowViewModel.GetCurrentMaps(), quick: false);
	}

	private async void RunTool(string[] paths, bool quick = false)
	{
		if(BackgroundWorker.IsBusy) return;

		ProjectManager.SaveProject(this, AutoSavePath);
		await BackupManager.SaveMapBackup(paths);

		ViewModel.Paths = paths;
		ViewModel.Quick = quick;

		BackgroundWorker.RunWorkerAsync(ViewModel);
	}

    protected override void OnUnloaded(RoutedEventArgs e)
    {
		ProjectManager.SaveProject(this, AutoSavePath);
        base.OnUnloaded(e);
    }

    protected override void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
	{
		var bgw = sender as BackgroundWorker;
		e.Result = Run_Program((MapCleanerViewModel)e.Argument!,  bgw, e);
	}

	protected override void BackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
	{
        Progress = e.ProgressPercentage;
		MapCleanerViewModel.SetProgress(Progress);
    }

	private string Run_Program(MapCleanerViewModel args, BackgroundWorker? worker, DoWorkEventArgs _)
	{
		var result = new MapCleanerResult();

		if(args is null)
		{
			return string.Empty;
		}

		if(args.Paths.Length == 1)
		{
			var editor = new BeatmapEditor(args.Paths[0]);
			List<TimingPoint> originalTimingPoints = editor.Beatmap.BeatmapTiming.TimingPoints.Select(tp => tp.Copy()).ToList();
			int oldTimingPointsCount = editor.Beatmap.BeatmapTiming.TimingPoints.Count;

			result.Add(MapCleaner.CleanMap(editor, args.MapCleanerArgs, worker));

			int removed = oldTimingPointsCount - editor.Beatmap.BeatmapTiming.TimingPoints.Count;
			result.TimingPointsRemoved += removed;

			var newTimingPoints = editor.Beatmap.BeatmapTiming.TimingPoints;
			Monitor_Differences(originalTimingPoints, newTimingPoints);

			editor.SaveFile();
		}
		else
		{
			foreach (string path in args.Paths)
			{
				var editor = new BeatmapEditor(path);
				int oldTimingPointsCount = editor.Beatmap.BeatmapTiming.TimingPoints.Count;

				result.Add(MapCleaner.CleanMap(editor, args.MapCleanerArgs, worker));

				int removed = oldTimingPointsCount - editor.Beatmap.BeatmapTiming.TimingPoints.Count;
                result.TimingPointsRemoved += removed;

				editor.SaveFile();
			}
		}

		RunFinished?.Invoke(this, new RunToolCompletedEventArgs(true, false, args.Quick));
		string message = $"Successfully {(result.TimingPointsRemoved < 0 ? "added" : "removed")} {Math.Abs(result.TimingPointsRemoved)} {(Math.Abs(result.TimingPointsRemoved) == 1 ? "greenline" : "greenlines")}" +
            (args.MapCleanerArgs.ResnapObjects ? $" and resnapped {result.ObjectsResnapped} {(result.ObjectsResnapped == 1 ? "object" : "objects")}" : "") + 
            (args.MapCleanerArgs.RemoveUnusedSamples ? $" and removed {result.SamplesRemoved} unused {(result.SamplesRemoved == 1 ? "sample" : "samples")}" : "") + "!";
        return args.Quick ? string.Empty : message;
	}

	private void Monitor_Differences(IReadOnlyList<TimingPoint> originalTimingPoints, IReadOnlyList<TimingPoint> newTimingPoints) {
        // Take note of all the changes
        timingpointsChanged = new List<double>();

        var originalInNew = (from first in originalTimingPoints
                             join second in newTimingPoints
                             on first.Offset equals second.Offset
                             select first).ToList();

        var newInOriginal = (from first in originalTimingPoints
                             join second in newTimingPoints
                             on first.Offset equals second.Offset
                             select second).ToList();
        
        foreach (TimingPoint tp in originalInNew) {
            bool different = true;
            List<TimingPoint> newTPs = newInOriginal.Where(o => Math.Abs(o.Offset - tp.Offset) < Precision.DoubleEpsilon).ToList();
            if (newTPs.Count == 0) { different = false; }
            foreach (TimingPoint newTp in newTPs) {
                if (tp.Equals(newTp)) { different = false; }
            }
            if (different) { timingpointsChanged.Add(tp.Offset); }
        }

        List<double> originalOffsets = new List<double>();
        List<double> newOffsets = new List<double>();
        foreach (var originalTimingPoint in originalTimingPoints) {
            originalOffsets.Add(originalTimingPoint.Offset);
        }
        foreach (var newTimingPoint in newTimingPoints) {
            newOffsets.Add(newTimingPoint.Offset);
        }

        timingpointsRemoved = originalOffsets.Except(newOffsets).ToList();
        timingpointsAdded = newOffsets.Except(originalOffsets).ToList();
        double endTimeOriginal = originalTimingPoints.Count > 0 ? originalTimingPoints.Last().Offset : 0;
        double endTimeNew = newTimingPoints.Count > 0 ? newTimingPoints.Last().Offset : 0;
        endTimeMonitor = Math.Max(endTimeOriginal, endTimeNew);
    }
	
	public MapCleanerViewModel GetSaveData()
	{
		return ViewModel;
	}

	public void SetSaveData(MapCleanerViewModel saveData) => DataContext = saveData;

    public void QuickRun()
    {
        RunTool([IOHelper.GetCurrentBeatmapOrCurrentBeatmap()], quick: true);
    }

    public string AutoSavePath => Program.configPath + "/mapcleanerproject.json";

	public string DefaultSaveFolder => Program.configPath + "/Map Cleaner Projects";

}