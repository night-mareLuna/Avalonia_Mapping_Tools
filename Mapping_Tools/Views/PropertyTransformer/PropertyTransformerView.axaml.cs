using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Avalonia.Interactivity;
using Avalonia_Mapping_Tools.ViewModels;
using Mapping_Tools.Classes.BeatmapHelper;
using Mapping_Tools.Classes.BeatmapHelper.Events;
using Mapping_Tools.Classes.MathUtil;
using Mapping_Tools.Classes.SystemTools;
using Mapping_Tools.Classes.ToolHelpers;
using Mapping_Tools.Views;
using MsBox.Avalonia.Enums;

namespace Avalonia_Mapping_Tools.Views;
public partial class PropertyTransformerView : SingleRunMappingTool, ISavable<PropertyTransformerViewModel>
{
	public PropertyTransformerView()
	{
		DataContext = new PropertyTransformerViewModel();
		InitializeComponent();
		Verbose = true;

		if(File.Exists(AutoSavePath))
			ProjectManager.LoadProject(this, message: false);
		else
			ProjectManager.SaveProject(this, AutoSavePath);
	}

	private async void Start_Click(object? obj, RoutedEventArgs args) {
		if(await MainWindow.ShowSaveDialog() == ButtonResult.Ok)
		{
			string[] filesToCopy = MainWindowViewModel.GetCurrentMaps();
			await BackupManager.SaveMapBackup(filesToCopy);

			((PropertyTransformerViewModel)DataContext!).ExportPaths = filesToCopy;
			BackgroundWorker.RunWorkerAsync(DataContext);
		}
    }

	protected override void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e) {
        var bgw = sender as BackgroundWorker;
        e.Result = TransformProperties((PropertyTransformerViewModel) e.Argument!, bgw!, e);
    }

    private string TransformProperties(PropertyTransformerViewModel vm, BackgroundWorker worker, DoWorkEventArgs _) {
        //var reader = EditorReaderStuff.GetFullEditorReaderOrNot();

        bool filter(double value, double time) {
            bool doFilterMatch = vm.MatchFilter.Length > 0 && vm.EnableFilters;
            bool doFilterUnmatch = vm.UnmatchFilter.Length > 0 && vm.EnableFilters;
            bool doFilterRange = (vm.MinTimeFilter != -1 || vm.MaxTimeFilter != -1) && vm.EnableFilters && !double.IsNaN(time);
            double min = vm.MinTimeFilter == -1 ? double.NegativeInfinity : vm.MinTimeFilter;
            double max = vm.MaxTimeFilter == -1 ? double.PositiveInfinity : vm.MaxTimeFilter;

            return (!doFilterMatch || vm.MatchFilter.Any(o => Precision.AlmostEquals(value, o, 0.001))) &&
                   (!doFilterUnmatch || !vm.UnmatchFilter.Any(o => Precision.AlmostEquals(value, o, 0.001))) &&
                   (!doFilterRange || (time >= min && time <= max));
        }

        void transformProperty(double multiplier, double offset, Func<double> getter, Action<double> setter, double time, double? min = null, double? max = null, bool round = false) {
            if (multiplier == 1 && offset == 0) {
                return;
            }

            var value = getter();

            if (!filter(value, time)) {
                return;
            }

            var newValue = value * multiplier + offset;

            if (round) newValue = Math.Round(newValue);
            if (vm.ClipProperties) {
                if (min.HasValue) newValue = Math.Max(newValue, min.Value);
                if (max.HasValue) newValue = Math.Min(newValue, max.Value);
            }

            setter(newValue);
        }

        void transformEventTime(Beatmap beatmap, Event ev, double multiplier, double offset) {
            var version = beatmap?.Version ?? 14;

            // Commands under loops use relative time so they shouldn't get offset
            if (ev.ParentEvent is StandardLoop or TriggerLoop) {
                if (ev is IHasStartTime st && filter(st.StartTime, st.StartTime)) {
                    st.StartTime = version < 128 ? (int) Math.Round(st.StartTime * multiplier) : st.StartTime * multiplier;
                }
                if (ev is IHasEndTime et && filter(et.EndTime, et.EndTime)) {
                    et.EndTime = version < 128 ? (int) Math.Round(et.EndTime * multiplier) : et.EndTime * multiplier;
                }
                if (ev is IHasDuration d && filter(d.Duration, double.NaN)) {  // Just a duration doesnt have a time to filter
                    d.Duration *= multiplier;
                }
            } else {
                if (ev is IHasStartTime st && filter(st.StartTime, st.StartTime)) {
                    st.StartTime = version < 128 ? (int) Math.Round(st.StartTime * multiplier + offset) : st.StartTime * multiplier + offset;
                }
                if (ev is IHasEndTime et && filter(et.EndTime, et.EndTime)) {
                    et.EndTime = version < 128 ? (int) Math.Round(et.EndTime * multiplier + offset) : et.EndTime * multiplier + offset;
                }
                if (ev is IHasDuration d && filter(d.Duration, double.NaN)) {  // Just a duration doesnt have a time to filter
                    d.Duration *= multiplier;
                }
            }

            // Recurse to also transform all the children events
            foreach (var child in ev.ChildEvents) {
                transformEventTime(beatmap, child, multiplier, offset);
            }
        }

        foreach (string path in vm.ExportPaths) {
            Editor editor;
            if (Path.GetExtension(path).ToLower() == ".osb") {
                editor = new StoryboardEditor(path);
            } else {
                editor = EditorReaderStuff.GetNewestVersionOrNot(path);
            }

            if (editor is BeatmapEditor beatmapEditor) {
                Beatmap beatmap = beatmapEditor.Beatmap;

                List<TimingPointsChange> timingPointsChanges = new List<TimingPointsChange>();
                foreach (TimingPoint tp in beatmap.BeatmapTiming.TimingPoints) {
                    // Offset
                    transformProperty(vm.TimingpointOffsetMultiplier, vm.TimingpointOffsetOffset, () => tp.Offset, o => tp.Offset = o, tp.Offset, round: beatmap.Version < 128);

                    // BPM
                    if (tp.Uninherited) transformProperty(vm.TimingpointBpmMultiplier, vm.TimingpointBpmOffset, tp.GetBpm, tp.SetBpm, tp.Offset, 15, 10000);

                    // Slider Velocity
                    transformProperty(vm.TimingpointSvMultiplier, vm.TimingpointSvOffset, () => beatmap.BeatmapTiming.GetSvMultiplierAtTime(tp.Offset), o => {
                        TimingPoint tpchanger = tp.Copy();
                        tpchanger.MpB = -100 / o;
                        timingPointsChanges.Add(new TimingPointsChange(tpchanger, mpb: true, fuzzyness: 0.4));
                    }, tp.Offset, 0.1, 10);

                    // Index
                    transformProperty(vm.TimingpointIndexMultiplier, vm.TimingpointIndexOffset, () => tp.SampleIndex, o => tp.SampleIndex = (int)o, tp.Offset, 0, int.MaxValue, true);

                    // Volume
                    transformProperty(vm.TimingpointVolumeMultiplier, vm.TimingpointVolumeOffset, () => tp.Volume, o => tp.Volume = (int)o, tp.Offset, 5, 100, true);
                }

                UpdateProgressBar(worker, 20);

                // Hitobject time
                if (vm.HitObjectTimeMultiplier != 1 || vm.HitObjectTimeOffset != 0) {
                    foreach (HitObject ho in beatmap.HitObjects) {
                        // Get the end time early because the start time gets modified
                        double oldEndTime = ho.GetEndTime(false);

                        // Transform start time of hitobject
                        transformProperty(vm.HitObjectTimeMultiplier, vm.HitObjectTimeOffset, () => ho.Time, o => ho.Time = o, ho.Time, round: beatmap.Version < 128);

                        // Transform end time of hold notes and spinner
                        if (ho.IsHoldNote || ho.IsSpinner) {
                            transformProperty(vm.HitObjectTimeMultiplier, vm.HitObjectTimeOffset, () => oldEndTime, o => ho.EndTime = o, oldEndTime, round: beatmap.Version < 128);
                        }
                    }
                }

                UpdateProgressBar(worker, 25);

                // Hitobject volume
                if (vm.HitObjectVolumeMultiplier != 1 || vm.HitObjectVolumeOffset != 0) {
                    foreach (HitObject ho in beatmap.HitObjects) {
                        transformProperty(vm.HitObjectVolumeMultiplier, vm.HitObjectVolumeOffset, () => ho.SampleVolume, o => ho.SampleVolume = o, ho.Time, 0, 100, true);
                    }
                }

                UpdateProgressBar(worker, 30);

                // Bookmark time
                if (vm.BookmarkTimeMultiplier != 1 || vm.BookmarkTimeOffset != 0) {
                    List<double> bookmarks = beatmap.GetBookmarks();
                    List<double> newBookmarks = bookmarks.Select(bookmark => filter(bookmark, bookmark) ? beatmap.Version < 128 ? Math.Round(bookmark * vm.BookmarkTimeMultiplier + vm.BookmarkTimeOffset) : bookmark * vm.BookmarkTimeMultiplier + vm.BookmarkTimeOffset : bookmark).ToList();

                    beatmap.SetBookmarks(newBookmarks);
                }

                UpdateProgressBar(worker, 40);

                // Storyboarded event time
                if (vm.SbEventTimeMultiplier != 1 || vm.SbEventTimeOffset != 0) {
                    foreach (Event ev in beatmap.StoryboardLayerBackground.Concat(beatmap.StoryboardLayerFail)
                        .Concat(beatmap.StoryboardLayerPass).Concat(beatmap.StoryboardLayerForeground)
                        .Concat(beatmap.StoryboardLayerOverlay)) {
                        transformEventTime(beatmap, ev, vm.SbEventTimeMultiplier, vm.SbEventTimeOffset);
                    }
                }

                UpdateProgressBar(worker, 50);

                // Storyboarded sample time
                if (vm.SbSampleTimeMultiplier != 1 || vm.SbSampleTimeOffset != 0) {
                    foreach (StoryboardSoundSample ss in beatmap.StoryboardSoundSamples) {
                        transformProperty(vm.SbSampleTimeMultiplier, vm.SbSampleTimeOffset, () => ss.StartTime, o => ss.StartTime = o, ss.StartTime, round: beatmap.Version < 128);
                    }
                }

                UpdateProgressBar(worker, 55);

                // Storyboarded sample volume
                if (vm.SbSampleVolumeMultiplier != 1 || vm.SbSampleVolumeOffset != 0) {
                    foreach (StoryboardSoundSample ss in beatmap.StoryboardSoundSamples) {
                        transformProperty(vm.SbSampleVolumeMultiplier, vm.SbSampleVolumeOffset, () => ss.Volume, o => ss.Volume = o, ss.StartTime, 8, 100, true);
                    }
                }

                UpdateProgressBar(worker, 60);

                // Break time
                if (vm.BreakTimeMultiplier != 1 || vm.BreakTimeOffset != 0) {
                    foreach (Break br in beatmap.BreakPeriods) {
                        transformProperty(vm.BreakTimeMultiplier, vm.BreakTimeOffset, () => br.StartTime, o => br.StartTime = o, br.StartTime, round: beatmap.Version < 128);
                        transformProperty(vm.BreakTimeMultiplier, vm.BreakTimeOffset, () => br.EndTime, o => br.EndTime = o, br.EndTime, round: beatmap.Version < 128);
                    }
                }

                UpdateProgressBar(worker, 70);

                // Video start time
                if (vm.VideoTimeMultiplier != 1 || vm.VideoTimeOffset != 0) {
                    foreach (Event ev in beatmap.BackgroundAndVideoEvents) {
                        if (ev is not Video video) {
                            continue;
                        }

                        transformProperty(vm.VideoTimeMultiplier, vm.VideoTimeOffset, () => video.StartTime, o => video.StartTime = o, video.StartTime, round: beatmap.Version < 128);
                    }
                }

                UpdateProgressBar(worker, 80);

                // Preview point time
                if (vm.PreviewTimeMultiplier != 1 || vm.PreviewTimeOffset != 0) {
                    if (beatmap.General.ContainsKey("PreviewTime") && beatmap.General["PreviewTime"].IntValue != -1) {
                        var previewTime = beatmap.General["PreviewTime"].DoubleValue;
                        transformProperty(vm.PreviewTimeMultiplier, vm.PreviewTimeOffset, () => previewTime, o => beatmap.General["PreviewTime"].SetDouble(o), previewTime, round: beatmap.Version < 128);
                    }
                }

                UpdateProgressBar(worker, 90);

                TimingPointsChange.ApplyChanges(beatmap.BeatmapTiming, timingPointsChanges);

                // Save the file
                beatmapEditor.SaveFile();

                UpdateProgressBar(worker, 100);
            } else if (editor is StoryboardEditor storyboardEditor) {
                StoryBoard storyboard = storyboardEditor.StoryBoard;
                
                // Storyboarded event time
                if (vm.SbEventTimeMultiplier != 1 || vm.SbEventTimeOffset != 0) {
                    foreach (Event ev in storyboard.StoryboardLayerBackground.Concat(storyboard.StoryboardLayerFail)
                        .Concat(storyboard.StoryboardLayerPass).Concat(storyboard.StoryboardLayerForeground)
                        .Concat(storyboard.StoryboardLayerOverlay)) {
                        transformEventTime(null, ev, vm.SbEventTimeMultiplier, vm.SbEventTimeOffset);
                    }
                }

                UpdateProgressBar(worker, 50);

                // Storyboarded sample time
                if (vm.SbSampleTimeMultiplier != 1 || vm.SbSampleTimeOffset != 0) {
                    foreach (StoryboardSoundSample ss in storyboard.StoryboardSoundSamples) {
                        transformProperty(vm.SbSampleTimeMultiplier, vm.SbSampleTimeOffset, () => ss.StartTime, o => ss.StartTime = o, ss.StartTime, round: true);
                    }
                }

                UpdateProgressBar(worker, 60);

                // Storyboarded sample volume
                if (vm.SbSampleVolumeMultiplier != 1 || vm.SbSampleVolumeOffset != 0) {
                    foreach (StoryboardSoundSample ss in storyboard.StoryboardSoundSamples) {
                        transformProperty(vm.SbSampleVolumeMultiplier, vm.SbSampleVolumeOffset, () => ss.Volume, o => ss.Volume = o, ss.StartTime, 8, 100, true);
                    }
                }

                UpdateProgressBar(worker, 70);

                // Video start time
                if (vm.VideoTimeMultiplier != 1 || vm.VideoTimeOffset != 0) {
                    foreach (Event ev in storyboard.BackgroundAndVideoEvents) {
                        if (ev is not Video video) {
                            continue;
                        }

                        transformProperty(vm.VideoTimeMultiplier, vm.VideoTimeOffset, () => video.StartTime, o => video.StartTime = o, video.StartTime, round: true);
                    }
                }

                UpdateProgressBar(worker, 90);

                // Save the file
                storyboardEditor.SaveFile();

                UpdateProgressBar(worker, 100);
            }
        }

        return "Done!";
    }

	protected override void BackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
	{
		base.BackgroundWorker_ProgressChanged(sender, e);
		PropertyTransformerViewModel.SetProgress(Progress);
    }

	public PropertyTransformerViewModel GetSaveData() {
        return (PropertyTransformerViewModel) DataContext;
    }

    public void SetSaveData(PropertyTransformerViewModel saveData) {
        DataContext = saveData;
    }

    public string AutoSavePath => Program.configPath + "/propertytransformerproject.json";

    public string DefaultSaveFolder => Program.configPath + "/Property Transformer Projects";

	protected override void OnUnloaded(RoutedEventArgs e)
    {
		ProjectManager.SaveProject(this, AutoSavePath);
        base.OnUnloaded(e);
    }
}