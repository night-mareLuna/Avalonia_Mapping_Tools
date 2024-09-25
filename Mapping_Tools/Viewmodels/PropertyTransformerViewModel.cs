using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace Avalonia_Mapping_Tools.ViewModels;

public partial class PropertyTransformerViewModel : ViewModelBase
{
	#region multipliers and offsets
	[ObservableProperty] private double timingpointOffsetMultiplier;
	[ObservableProperty] private double timingpointOffsetOffset;
	[ObservableProperty] private double timingpointBpmMultiplier;
	[ObservableProperty] private double timingpointBpmOffset;
	[ObservableProperty] private double timingpointSvMultiplier;
	[ObservableProperty] private double timingpointSvOffset;
	[ObservableProperty] private double timingpointIndexMultiplier;
	[ObservableProperty] private double timingpointIndexOffset;
	[ObservableProperty] private double timingpointVolumeMultiplier;
	[ObservableProperty] private double timingpointVolumeOffset;
	[ObservableProperty] private double hitObjectTimeMultiplier;
	[ObservableProperty] private double hitObjectTimeOffset;
	[ObservableProperty] private double hitObjectVolumeMultiplier;
	[ObservableProperty] private double hitObjectVolumeOffset;
	[ObservableProperty] private double bookmarkTimeMultiplier;
	[ObservableProperty] private double bookmarkTimeOffset;
	[ObservableProperty] private double sbEventTimeMultiplier;
	[ObservableProperty] private double sbEventTimeOffset;
	[ObservableProperty] private double sbSampleTimeMultiplier;
	[ObservableProperty] private double sbSampleTimeOffset;
	[ObservableProperty] private double sbSampleVolumeMultiplier;
	[ObservableProperty] private double sbSampleVolumeOffset;
	[ObservableProperty] private double breakTimeMultiplier;
	[ObservableProperty] private double breakTimeOffset;
	[ObservableProperty] private double videoTimeMultiplier;
	[ObservableProperty] private double videoTimeOffset;
	[ObservableProperty] private double previewTimeMultiplier;
	[ObservableProperty] private double previewTimeOffset;
	#endregion

	[ObservableProperty] private bool clipProperties;
	[ObservableProperty] private bool enableFilters;
	[ObservableProperty] private double[] matchFilter;
	[ObservableProperty] private double[] unmatchFilter;
	[ObservableProperty] private double minTimeFilter;
	[ObservableProperty] private double maxTimeFilter;
	[ObservableProperty] private bool syncTimeFields;
	[ObservableProperty] [property: JsonIgnore] private int _Progress = 0;
	[JsonIgnore] public string[] ExportPaths { get; set; }
	private static PropertyTransformerViewModel? Me;

	
	public PropertyTransformerViewModel()
	{
		Me = this;
		ResetMultipliersAndOffsets();
		
		SyncTimeFields = false;
		ClipProperties = false;
		EnableFilters = false;
		MatchFilter = [];
		UnmatchFilter = [];
		MinTimeFilter = -1;
		MaxTimeFilter = -1;
	}

	private void ResetMultipliersAndOffsets()
	{
        TimingpointOffsetMultiplier = 1;
        TimingpointOffsetOffset = 0;
        TimingpointBpmMultiplier = 1;
        TimingpointBpmOffset = 0;
        TimingpointSvMultiplier = 1;
        TimingpointSvOffset = 0;
        TimingpointIndexMultiplier = 1;
        TimingpointIndexOffset = 0;
        TimingpointVolumeMultiplier = 1;
        TimingpointVolumeOffset = 0;
        HitObjectTimeMultiplier = 1;
        HitObjectTimeOffset = 0;
        BookmarkTimeMultiplier = 1;
        BookmarkTimeOffset = 0;
        SbEventTimeMultiplier = 1;
        SbEventTimeOffset = 0;
        SbSampleTimeMultiplier = 1;
        SbSampleTimeOffset = 0;
        BreakTimeMultiplier = 1;
        BreakTimeOffset = 0;
        VideoTimeMultiplier = 1;
        VideoTimeOffset = 0;
        PreviewTimeMultiplier = 1;
        PreviewTimeOffset = 0;
        SbSampleVolumeMultiplier = 1;
        SbSampleVolumeOffset = 0;
        HitObjectVolumeMultiplier = 1;
        HitObjectVolumeOffset = 0;
    }

	private void SetAllTimeMultipliers(double value)
	{
        if(SyncTimeFields)
		{
			TimingpointOffsetMultiplier = value;
			HitObjectTimeMultiplier = value;
			BookmarkTimeMultiplier = value;
			SbEventTimeMultiplier = value;
			SbSampleTimeMultiplier = value;
			BreakTimeMultiplier = value;
			VideoTimeMultiplier = value;
			PreviewTimeMultiplier = value;
		}
		else return;
    }

	private void SetAllTimeOffsets(double value)
	{
		if(SyncTimeFields)
		{
			TimingpointOffsetOffset = value;
			HitObjectTimeOffset = value;
			BookmarkTimeOffset = value;
			SbEventTimeOffset = value;
			SbSampleTimeOffset = value;
			BreakTimeOffset = value;
			VideoTimeOffset = value;
			PreviewTimeOffset = value;
		}
		else return;
    }

	public void ResetCommand() => ResetMultipliersAndOffsets();

	partial void OnTimingpointOffsetMultiplierChanged(double value) => SetAllTimeMultipliers(value);
	partial void OnHitObjectTimeMultiplierChanged(double value) => SetAllTimeMultipliers(value);
	partial void OnBookmarkTimeMultiplierChanged(double value) => SetAllTimeMultipliers(value);
	partial void OnSbEventTimeMultiplierChanged(double value) => SetAllTimeMultipliers(value);
	partial void OnSbSampleTimeMultiplierChanged(double value) => SetAllTimeMultipliers(value);
	partial void OnBreakTimeMultiplierChanged(double value) => SetAllTimeMultipliers(value);
	partial void OnVideoTimeMultiplierChanged(double value) => SetAllTimeMultipliers(value);
	partial void OnPreviewTimeMultiplierChanged(double value) => SetAllTimeMultipliers(value);

	partial void OnTimingpointOffsetOffsetChanged(double value) => SetAllTimeOffsets(value);
	partial void OnHitObjectTimeOffsetChanged(double value) => SetAllTimeOffsets(value);
	partial void OnBookmarkTimeOffsetChanged(double value) => SetAllTimeOffsets(value);
	partial void OnSbEventTimeOffsetChanged(double value) => SetAllTimeOffsets(value);
	partial void OnSbSampleTimeOffsetChanged(double value) => SetAllTimeOffsets(value);
	partial void OnBreakTimeOffsetChanged(double value) => SetAllTimeOffsets(value);
	partial void OnVideoTimeOffsetChanged(double value) => SetAllTimeOffsets(value);
	partial void OnPreviewTimeOffsetChanged(double value) => SetAllTimeOffsets(value);

    public static void SetProgress(int prog) => Me!.Progress = prog;
}