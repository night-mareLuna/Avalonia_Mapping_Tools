using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Mapping_Tools.Classes;
using Mapping_Tools.Classes.BeatmapHelper.Enums;
using Mapping_Tools.Classes.SystemTools;
using Mapping_Tools.Classes.Tools;
using Newtonsoft.Json;

namespace Avalonia_Mapping_Tools.ViewModels;

public partial class RhythmGuideViewModel : ViewModelBase
{
	[ObservableProperty] private RhythmGuide.RhythmGuideGeneratorArgs _GuideGeneratorArgs;
	[ObservableProperty] [property: JsonIgnore] private int _Progress = 0;
	private static RhythmGuideViewModel? Me;
	public RhythmGuideViewModel()
	{
		Me = this;
		GuideGeneratorArgs = new();
	}

	public static void UpdateProgress(int prog) => Me!.Progress = prog;

	public async void ImportLoadCommand()
	{
		try 
		{
			var path = await IOHelper.GetCurrentBeatmap();
			if(!string.IsNullOrEmpty(path))
				GuideGeneratorArgs.Paths = [path];
		}
		catch (Exception ex) { ex.Show(); }
	}

	public async void ImportBrowseCommand()
	{
		var paths = await IOHelper.BeatmapFileDialog(true, SettingsManager.Settings.CurrentBeatmapDefaultFolder);
		if(paths.Length != 0)
			GuideGeneratorArgs.Paths = paths;
	}

	public async void ExportLoadCommand()
	{
		try
		{
			var path = await IOHelper.GetCurrentBeatmap();
			if(!string.IsNullOrEmpty(path))
				GuideGeneratorArgs.ExportPath = path;
		}
		catch(Exception ex) { ex.Show(); }
	}

	public async void ExportBrowseCommand()
	{
		var paths = await IOHelper.BeatmapFileDialog(restore: SettingsManager.Settings.CurrentBeatmapDefaultFolder);
		if(paths.Length != 0)
			GuideGeneratorArgs.ExportPath = paths[0];
	}

	[JsonIgnore] public IEnumerable<RhythmGuide.ExportMode> ExportModes =>
		Enum.GetValues(typeof(RhythmGuide.ExportMode)).Cast<RhythmGuide.ExportMode>();
	[JsonIgnore] public IEnumerable<GameMode> ExportGameModes =>
		Enum.GetValues(typeof(GameMode)).Cast<GameMode>();
	[JsonIgnore] public IEnumerable<RhythmGuide.SelectionMode> SelectionModes =>
		Enum.GetValues(typeof(RhythmGuide.SelectionMode)).Cast<RhythmGuide.SelectionMode>();
}