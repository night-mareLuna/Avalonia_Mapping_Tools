using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
//using System.Drawing;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Mapping_Tools.Classes;
using Mapping_Tools.Classes.BeatmapHelper;
using Mapping_Tools.Classes.MathUtil;
using Mapping_Tools.Classes.SystemTools;
using Newtonsoft.Json;

namespace Avalonia_Mapping_Tools.ViewModels;

public partial class MetadataManagerViewModel : ViewModelBase
{
	[ObservableProperty] [property: JsonIgnore] private int _Progress = 0;
	[ObservableProperty] [property: JsonIgnore] private bool _TagsOverflowErrorVisibility = false;
	[ObservableProperty] [property: JsonIgnore] private bool _BeatmapFileNameOverflowErrorVisibility = false;
	
	[ObservableProperty] private string _ImportPath = "";
	[ObservableProperty] private string _ExportPath = "";
	
	[ObservableProperty] [property: MaxLength(81, ErrorMessage = "Field cannot be over 81 characters long.")] private string _Artist = "";
	[ObservableProperty] [property: MaxLength(81, ErrorMessage = "Field cannot be over 81 characters long.")] private string _RomanisedArtist = "";
	[ObservableProperty] [property: MaxLength(81, ErrorMessage = "Field cannot be over 81 characters long.")] private string _Title = "";
	[ObservableProperty] [property: MaxLength(81, ErrorMessage = "Field cannot be over 81 characters long.")] private string _RomanisedTitle = "";
	[ObservableProperty] [property: MaxLength(81, ErrorMessage = "Field cannot be over 81 characters long.")] private string _BeatmapCreator = "";
	[ObservableProperty] [property: MaxLength(81, ErrorMessage = "Field cannot be over 81 characters long.")] private string _Source = "";
	[ObservableProperty] [property: MaxLength(1024, ErrorMessage = "Field cannot be over 1024 characters long.")] private string _Tags = "";
	[ObservableProperty] private bool _DoRemoveDuplicateTags = true;
	[ObservableProperty] private bool _ResetIds = false;

	[ObservableProperty] private double _PreviewTime = -1;
	[ObservableProperty] private bool _UseComboColours = true;
	[ObservableProperty] private ObservableCollection<ComboColour> _ComboColours = [];
	[ObservableProperty] private ObservableCollection<SpecialColour> _SpecialColours = [];

	private static MetadataManagerViewModel? Me;

    public MetadataManagerViewModel()
    {
        Me = this;
    }

	public void ImportFromBeatmap(string importPath)
	{
		try {
            var editor = new BeatmapEditor(importPath);
            var beatmap = editor.Beatmap;

            Artist = beatmap.Metadata["ArtistUnicode"].Value;
            RomanisedArtist = beatmap.Metadata["Artist"].Value;
            Title = beatmap.Metadata["TitleUnicode"].Value;
            RomanisedTitle = beatmap.Metadata["Title"].Value;
            BeatmapCreator = beatmap.Metadata["Creator"].Value;
            Source = beatmap.Metadata["Source"].Value;
            Tags = beatmap.Metadata["Tags"].Value;

            PreviewTime = beatmap.General["PreviewTime"].DoubleValue;
            ComboColours = new ObservableCollection<ComboColour>(beatmap.ComboColours);
            SpecialColours.Clear();
            foreach (var specialColour in beatmap.SpecialColours) {
                SpecialColours.Add(new SpecialColour(specialColour.Value.Color, specialColour.Key));
            }
        }
        catch( Exception ex ) {
            ex.Show();
        }
	}

	public void ImportLoadCommand()
	{
        try {
            string path = IOHelper.GetCurrentBeatmap();
            if (path != "") {
                ImportPath = path;
            }
        } catch (Exception ex) {
            ex.Show();
        }
	}

	public async void ImportBrowseCommand()
	{
        var paths = await IOHelper.BeatmapFileDialog(restore: SettingsManager.Settings.CurrentBeatmapDefaultFolder);
        if( paths.Length != 0 ) {
            ImportPath = paths[0];
        }
	}

	public void ImportCommand() => ImportFromBeatmap(ImportPath);

	public void ExportLoadCommand()
	{
        try {
            string path = IOHelper.GetCurrentBeatmap();
            if (path != "") {
                ExportPath = path;
            }
        } catch (Exception ex) {
            ex.Show();
        }
	}

	public async void ExportBrowseCommand()
	{
		string? importPathDirectory = null;
		if(!string.IsNullOrWhiteSpace(ImportPath))
			importPathDirectory = Directory.GetParent(ImportPath)?.FullName;

		importPathDirectory ??= Directory.GetParent(MainWindowViewModel.GetCurrentMaps()[0])?.FullName;
		importPathDirectory ??= SettingsManager.GetSongsPath();

        var paths = await IOHelper.BeatmapFileDialog(importPathDirectory, true);
        if( paths.Length != 0 ) {
            ExportPath = string.Join("|", paths);
        }
	}

	// public void AddCommand()
	// {
    //     if (ComboColours.Count >= 8) return;
    //     ComboColours.Add(ComboColours.Count > 0
    //         ? new ComboColour(ComboColours[ComboColours.Count - 1].Color)
    //         : new ComboColour(Color.White));
	// }

	// public void RemoveCommand()
	// {
    //     if (ComboColours.Count > 0) {
    //         ComboColours.RemoveAt(ComboColours.Count - 1);
    //     }
	// }

	// public void AddSpecialCommand()
	// {
    //     SpecialColours.Add(SpecialColours.Count > 0
    //         ? new SpecialColour(SpecialColours[SpecialColours.Count - 1].Color)
    //         : new SpecialColour(Colors.White));
	// }

	// public void RemoveSpecialCommand()
	// {
    //     if (SpecialColours.Count > 0) {
    //         SpecialColours.RemoveAt(SpecialColours.Count - 1);
    //     }
	// }

	private static string RemoveDuplicateTags(string tagsString)
	{
		string[] tags = tagsString.Split(' ');
		return string.Join(" ", new HashSet<string>(tags));
	}

    private void CheckOverflowError()
	{
		var length = 13 + (RomanisedArtist?.Length ?? 0) + (RomanisedTitle?.Length ?? 0) + (BeatmapCreator?.Length ?? 0);
		BeatmapFileNameOverflowErrorVisibility = length > 255;
	}

    partial void OnRomanisedArtistChanged(string value) => CheckOverflowError();
    partial void OnRomanisedTitleChanged(string value) => CheckOverflowError();
    partial void OnBeatmapCreatorChanged(string value) => CheckOverflowError();

    partial void OnTagsChanged(string? oldValue, string newValue)
    {
        if(newValue == oldValue) return;
		if(DoRemoveDuplicateTags)
			Tags = RemoveDuplicateTags(newValue);
		TagsOverflowErrorVisibility = newValue.Length > 1024 || Tags.Split(' ').Length > 100;
    }

    partial void OnDoRemoveDuplicateTagsChanged(bool oldValue, bool newValue)
    {
        if(oldValue == newValue) return;
		if(newValue)
			Tags = RemoveDuplicateTags(Tags);
    }

    partial void OnPreviewTimeChanged(double oldValue, double newValue)
    {
		if(oldValue == newValue) return;
        if(Math.Abs(oldValue - newValue) < Precision.DoubleEpsilon)
			PreviewTime = oldValue;
    }

    public static void SetProgress(int prog) => Me!.Progress = prog;
}