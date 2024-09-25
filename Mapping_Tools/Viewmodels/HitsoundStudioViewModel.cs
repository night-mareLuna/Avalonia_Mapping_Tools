using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia_Mapping_Tools.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using Mapping_Tools.Classes;
using Mapping_Tools.Classes.BeatmapHelper.Enums;
using Mapping_Tools.Classes.HitsoundStuff;
using Mapping_Tools.Components.Domain;
using Newtonsoft.Json;

namespace Avalonia_Mapping_Tools.ViewModels;

public partial class HitsoundStudioViewModel : ViewModelBase
{
	private static HitsoundStudioViewModel? Me;
	[ObservableProperty] [property: JsonIgnore] private int _Progress = 0;
	[ObservableProperty] private string _BaseBeatmap;
	[ObservableProperty] private Sample _DefaultSample;
	[ObservableProperty] private string _ExportFolder;
	[ObservableProperty] private string _HitsoundDiffName;
	[ObservableProperty] private bool _ShowResults;
	[ObservableProperty] private bool _ExportMap;
	[ObservableProperty] private bool _ExportSamples;
	[ObservableProperty] private bool _DeleteAllInExportFirst;
	[ObservableProperty] private bool _UsePreviousSampleSchema;
	[ObservableProperty] private bool _AllowGrowthPreviousSampleSchema;
	[ObservableProperty] private bool _AddCoincidingRegularHitsounds;
	[ObservableProperty] private bool _AddGreenLineVolumeToMidi;
	public SampleSchema? PreviousSampleSchema { get; set; }
	[ObservableProperty] private HitsoundExportMode _HitsoundExportModeSetting;

	// Make sure view updates when changing export mode
	[ObservableProperty] [property: JsonIgnore] private bool _StandardExtraSettingsVisibility;
	[ObservableProperty] [property: JsonIgnore] private bool _CoincidingExtraSettingsVisibility;
	[ObservableProperty] [property: JsonIgnore] private bool _StoryboardExtraSettingsVisibility;
	[ObservableProperty] [property: JsonIgnore] private bool _MidiExtraSettingsVisibility;
	[ObservableProperty] [property: JsonIgnore] private bool _SampleExportSettingsVisibility;

	public IEnumerable<HitsoundExportMode> HitsoundExportModes => Enum.GetValues(typeof(HitsoundExportMode)).Cast<HitsoundExportMode>();
	[ObservableProperty] private GameMode _HitsoundExportGameMode;
	[JsonIgnore] public IEnumerable<GameMode> HitsoundExportGameModes => Enum.GetValues(typeof(GameMode)).Cast<GameMode>();
	[ObservableProperty] [property: GreaterThanOrEqual(0)] private double _ZipLayersLeniency;
	[ObservableProperty] [property: GreaterThanOrEqual(0)] private int _FirstCustomIndex;
	[ObservableProperty] private HitsoundExporter.SampleExportFormat _SingleSampleExportFormat;
	[ObservableProperty] private HitsoundExporter.SampleExportFormat _MixedSampleExportFormat;
	[JsonIgnore] public readonly Dictionary<HitsoundExporter.SampleExportFormat, string> SampleExportFormatDisplayNameMapping =
		new Dictionary<HitsoundExporter.SampleExportFormat, string> {{HitsoundExporter.SampleExportFormat.Default, "Default"},
			{HitsoundExporter.SampleExportFormat.WaveIeeeFloat, "IEEE Float (.wav)"},
			{HitsoundExporter.SampleExportFormat.WavePcm, "PCM 16-bit (.wav)"},
			{HitsoundExporter.SampleExportFormat.OggVorbis, "Vorbis (.ogg)"},
			{HitsoundExporter.SampleExportFormat.MidiChords, "Single-chord MIDI (.mid)"}
		};
	[JsonIgnore] public IEnumerable<string> SampleExportFormatDisplayNames => SampleExportFormatDisplayNameMapping.Values;
	public string SingleSampleExportFormatDisplay
	{
		get => SampleExportFormatDisplayNameMapping[SingleSampleExportFormat];
		set
		{
			foreach (var kvp in SampleExportFormatDisplayNameMapping.Where(kvp => kvp.Value == value))
			{
				SingleSampleExportFormat = kvp.Key;
				break;
			}
		}
	}
	public string MixedSampleExportFormatDisplay
	{
		get => SampleExportFormatDisplayNameMapping[MixedSampleExportFormat];
		set
		{
			foreach (var kvp in SampleExportFormatDisplayNameMapping.Where(kvp => kvp.Value == value))
			{
				MixedSampleExportFormat = kvp.Key;
				break;
			}
		}
	}
	public ObservableCollection<HitsoundLayer> HitsoundLayers { get; set; }
	private string _EditTimes = "";
	public string EditTimes
	{
		get => _EditTimes;
		set
		{
			if(!Regex.IsMatch((value ?? "").ToString()!, @"^([0-9]+(\.[0-9]+)?(,[0-9]+(\.[0-9]+)?)*)?$"))
				throw new ValidationException("Field cannot be parsed.");
			else
				_EditTimes = value!;
		}
	}
	public HitsoundStudioViewModel() : this("", new Sample {Priority = int.MaxValue}, new ObservableCollection<HitsoundLayer>()) { }
	public HitsoundStudioViewModel(string baseBeatmap, Sample defaultSample, ObservableCollection<HitsoundLayer> hitsoundLayers)
	{
		Me = this;
		BaseBeatmap = baseBeatmap;
        DefaultSample = defaultSample;
        HitsoundLayers = hitsoundLayers;
        ExportFolder = MainWindow.ExportPath;
        HitsoundDiffName = "Hitsounds";
        ShowResults = false;
        ExportMap = true;
        ExportSamples = true;
        DeleteAllInExportFirst = false;
        AddCoincidingRegularHitsounds = true;
		AddGreenLineVolumeToMidi = true;
        HitsoundExportModeSetting = HitsoundExportMode.Standard;
        HitsoundExportGameMode = GameMode.Standard;
        ZipLayersLeniency = 15;
        FirstCustomIndex = 1;
        SingleSampleExportFormat = HitsoundExporter.SampleExportFormat.Default;
        MixedSampleExportFormat = HitsoundExporter.SampleExportFormat.Default;

		StandardExtraSettingsVisibility = HitsoundExportModeSetting == HitsoundExportMode.Standard;
		CoincidingExtraSettingsVisibility = false;
		StoryboardExtraSettingsVisibility = false;
		MidiExtraSettingsVisibility = false;
		SampleExportSettingsVisibility = true;
	}

	public enum HitsoundExportMode
	{
		Standard,
		Coinciding,
		Storyboard,
		Midi
	}

    partial void OnSingleSampleExportFormatChanged(HitsoundExporter.SampleExportFormat value)
    {
        if(value == HitsoundExporter.SampleExportFormat.MidiChords)
			MixedSampleExportFormat = value;
		else if(MixedSampleExportFormat == HitsoundExporter.SampleExportFormat.MidiChords)
			MixedSampleExportFormat = value;
    }

    partial void OnMixedSampleExportFormatChanged(HitsoundExporter.SampleExportFormat value)
    {
        if(value == HitsoundExporter.SampleExportFormat.MidiChords)
			SingleSampleExportFormat = value;
		else if(SingleSampleExportFormat == HitsoundExporter.SampleExportFormat.MidiChords)
			SingleSampleExportFormat = value;
    }

	public void HitsoundLayer_MouseDoubleClickCommand()
	{
		var selectedLayer = HitsoundStudioView.GetSelectedLayer();
		if(selectedLayer is null) return;

		try
		{
			var player = new Playback(selectedLayer.SampleArgs.Path);
			player.Play();
		}
		catch (Exception ex)
		{
			ex.Show();
		}
		
		// try
        // {
        //     SampleGeneratingArgs args = selectedLayer!.SampleArgs;
        //     var mainOutputStream = SampleImporter.ImportSample(args);

        //     if (mainOutputStream == null)
        //     {
		// 		var box = MessageBoxManager.GetMessageBoxStandard("Error!",
		// 			"Could not load the specified sample.",
		// 			ButtonEnum.Ok);
        //         await box.ShowAsync();
        //         return;
        //     }
            
        //     outputDevice = new WasapiOut();
        //     outputDevice.PlaybackStopped += PlayerStopped;

        //     outputDevice.Init(mainOutputStream.GetSampleProvider());

        //     outputDevice.Play();
        // }
        // catch (FileNotFoundException)
		// {
		// 	var box = MessageBoxManager.GetMessageBoxStandard("Error!",
		// 		"Could not find the specified sample.",
		// 		ButtonEnum.Ok);
		// 	await box.ShowAsync();
		// }
        // catch (DirectoryNotFoundException)
		// {
		// 	var box = MessageBoxManager.GetMessageBoxStandard("Error!",
		// 		"Could not find the specified sample's directory.",
		// 		ButtonEnum.Ok);
		// 	await box.ShowAsync();
		// }
        // catch (Exception ex) { ex.Show(); }
	}

	// private static void PlayerStopped(object? sender, StoppedEventArgs e)
    // {
    //     ((IWavePlayer)sender!).Dispose();
    // }

	partial void OnHitsoundExportModeSettingChanged(HitsoundExportMode value)
	{
		StandardExtraSettingsVisibility = value == HitsoundExportMode.Standard;
		CoincidingExtraSettingsVisibility = value == HitsoundExportMode.Coinciding;
		StoryboardExtraSettingsVisibility = value == HitsoundExportMode.Storyboard;
		MidiExtraSettingsVisibility = value == HitsoundExportMode.Midi;
		SampleExportSettingsVisibility = value != HitsoundExportMode.Midi;
	}

    public static void SetProgress(int prog) => Me!.Progress = prog;
}