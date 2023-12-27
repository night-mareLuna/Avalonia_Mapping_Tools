using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Mapping_Tools.Classes.BeatmapHelper.BeatDivisors;
using Mapping_Tools.Classes.BeatmapHelper.Enums;
using Mapping_Tools.Classes.SystemTools;
using Newtonsoft.Json;

namespace Avalonia_Mapping_Tools.ViewModels;

public partial class HitsoundCopierViewModel : ViewModelBase
{
	[ObservableProperty] private string pathTo;
    [ObservableProperty] private string pathFrom;
    [ObservableProperty] private int copyMode;
	[ObservableProperty] private bool smartCopyModeSelected;
    [ObservableProperty] private double temporalLeniency;
    [ObservableProperty] private bool copyHitsounds;
    [ObservableProperty] private bool copyBodyHitsounds;
    [ObservableProperty] private bool copySampleSets;
    [ObservableProperty] private bool copyVolumes;
    [ObservableProperty] private bool alwaysPreserve5Volume;
    [ObservableProperty] private bool copyStoryboardedSamples;
    [ObservableProperty] private bool ignoreHitsoundSatisfiedSamples;
    [ObservableProperty] private bool ignoreWheneverHitsound;
    [ObservableProperty] private bool copyToSliderTicks;
    [ObservableProperty] private bool copyToSliderSlides;
	[ObservableProperty] private bool startIndexBoxVisible;
    [ObservableProperty] private int startIndex;
    [ObservableProperty] private bool muteSliderends;
    [ObservableProperty] private IBeatDivisor[] beatDivisors;
    [ObservableProperty] private IBeatDivisor[] mutedDivisors;
    [ObservableProperty] private double minLength;
    [ObservableProperty] private int mutedIndex;
    [ObservableProperty] private SampleSet mutedSampleSet;
	[ObservableProperty] private int _Progress = 0;

	[JsonIgnore]
	public IEnumerable<SampleSet> MutedSampleSets => SampleSet.GetValues(typeof(SampleSet)).Cast<SampleSet>();
	private static HitsoundCopierViewModel? Me;

	public HitsoundCopierViewModel()
	{
		Me = this;
		PathFrom = string.Empty;
        PathTo = string.Empty;
        CopyMode = 0;
		SmartCopyModeSelected = false;
        TemporalLeniency = 5;
        CopyHitsounds = true;
        CopyBodyHitsounds = true;
        CopySampleSets = true;
        CopyVolumes = true;
        AlwaysPreserve5Volume = true;
        CopyStoryboardedSamples = false;
        IgnoreHitsoundSatisfiedSamples = true;
        CopyToSliderTicks = false;
        CopyToSliderSlides = false;
        StartIndex = 100;
        MuteSliderends = false;
        BeatDivisors = [
            new RationalBeatDivisor(1),
            new RationalBeatDivisor(4), new RationalBeatDivisor(3),
            new RationalBeatDivisor(8), new RationalBeatDivisor(6),
            new RationalBeatDivisor(16), new RationalBeatDivisor(12)
        ];
        MutedDivisors = [
            new RationalBeatDivisor(4), new RationalBeatDivisor(3),
            new RationalBeatDivisor(8), new RationalBeatDivisor(6),
            new RationalBeatDivisor(16), new RationalBeatDivisor(12)
        ];
        MinLength = 0.5;
        MutedIndex = -1;
        MutedSampleSet = SampleSet.None;
	}

	public void LoadCurrentBeatmap(bool copyTo)
	{
		if(SettingsManager.GetRecentMaps().Count == 0) return;
		string[] currentMaps = MainWindowViewModel.GetCurrentMaps();
		SetPath(currentMaps, copyTo);
	}

	public async void OpenBeatmap(bool copyTo)
	{
		if(!copyTo)
		{
			string[] path = await IOHelper.BeatmapFileDialog(restore: SettingsManager.Settings.CurrentBeatmapDefaultFolder);
			if(!string.IsNullOrEmpty(path[0]))
				SetPath(path, copyTo);
		}
		else
		{
			string[] paths = await IOHelper.BeatmapFileDialog(multiselect: true, restore: SettingsManager.Settings.CurrentBeatmapDefaultFolder);
			if(!string.IsNullOrEmpty(paths[0]))
				SetPath(paths, copyTo);
		}
	}

	private void SetPath(string[] paths, bool copyTo)
	{
		if(!copyTo) PathFrom = paths[0];
		else
		{
			PathTo = string.Empty;
			for(int i=0; i<paths.Length; i++)
			{
				PathTo += paths[i] + (i != paths.Length - 1 ? '|' : "");
			}
		}
	}

    partial void OnCopyModeChanged(int value) 
	{
		if(value==1) SmartCopyModeSelected = true;
		else SmartCopyModeSelected = false;
	}

    partial void OnCopyToSliderSlidesChanged(bool value)
    {
        if(CopyToSliderTicks)
			return;
		else StartIndexBoxVisible = value;
    }

    partial void OnCopyToSliderTicksChanged(bool value)
    {
        if(CopyToSliderSlides)
			return;
		else StartIndexBoxVisible = value;
    }

	public static void SetProgress(int prog) => Me!.Progress = prog;
}