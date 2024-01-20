using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Avalonia.Interactivity;
using Avalonia_Mapping_Tools.ViewModels;
using Mapping_Tools.Classes.BeatmapHelper;
using Mapping_Tools.Classes.SystemTools;
using Mapping_Tools.Classes.ToolHelpers;
using Mapping_Tools.Views;
using MsBox.Avalonia.Enums;

namespace Avalonia_Mapping_Tools.Views;
public partial class MetadataManagerView : SingleRunMappingTool, ISavable<MetadataManagerViewModel>
{
	public MetadataManagerView()
	{
		DataContext = new MetadataManagerViewModel();

		if(File.Exists(AutoSavePath))
			ProjectManager.LoadProject(this, message: false);
		else
			ProjectManager.SaveProject(this, AutoSavePath);

		Verbose = true;
		InitializeComponent();
	}

	protected override void OnUnloaded(RoutedEventArgs e)
    {
		ProjectManager.SaveProject(this, AutoSavePath);
        base.OnUnloaded(e);
    }
	
	private async void Start_Click(object obj, RoutedEventArgs args)
	{
		if(await MainWindow.ShowSaveDialog() != ButtonResult.Ok || BackgroundWorker.IsBusy) return;
		
		var filesToCopy = ((MetadataManagerViewModel)DataContext!).ExportPath.Split('|');
		foreach(var fileToCopy in filesToCopy)
			await BackupManager.SaveMapBackup(fileToCopy);

		BackgroundWorker.RunWorkerAsync((MetadataManagerViewModel)DataContext);
	}

    protected override void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
    {
        var bgw = sender as BackgroundWorker;
		e.Result = Copy_Metadata((MetadataManagerViewModel)e.Argument!, bgw!, e);
    }

    protected override void BackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
    {
        base.BackgroundWorker_ProgressChanged(sender, e);
		MetadataManagerViewModel.SetProgress(Progress);
    }

    private static string Copy_Metadata(MetadataManagerViewModel arg, BackgroundWorker worker, DoWorkEventArgs _)
	{
		var paths = arg.ExportPath.Split('|');
        var mapsDone = 0;

        //var reader = EditorReaderStuff.GetFullEditorReaderOrNot();

        foreach (var path in paths) {
            var editor = EditorReaderStuff.GetNewestVersionOrNot(path);
            var beatmap = editor.Beatmap;

            beatmap.Metadata["ArtistUnicode"].Value = arg.Artist;
            beatmap.Metadata["Artist"].Value = arg.RomanisedArtist;
            beatmap.Metadata["TitleUnicode"].Value = arg.Title;
            beatmap.Metadata["Title"].Value = arg.RomanisedTitle;
            beatmap.Metadata["Creator"].Value = arg.BeatmapCreator;
            beatmap.Metadata["Source"].Value = arg.Source;
            beatmap.Metadata["Tags"].Value = arg.Tags;

            beatmap.General["PreviewTime"] = new TValue(arg.PreviewTime.ToRoundInvariant());
            if (arg.UseComboColours) {
                beatmap.ComboColours = new List<ComboColour>(arg.ComboColours);
                beatmap.SpecialColours.Clear();
                foreach (var specialColour in arg.SpecialColours) {
                    beatmap.SpecialColours.Add(specialColour.Name, specialColour);
                }
            }

            if (arg.ResetIds) {
                beatmap.Metadata["BeatmapID"].Value = @"0";
                beatmap.Metadata["BeatmapSetID"].Value = @"-1";
            }

            // Save the file with name update because we updated the metadata
            editor.SaveFileWithNameUpdate();

            // Update progressbar
            if (worker != null && worker.WorkerReportsProgress) {
                worker.ReportProgress(++mapsDone * 100 / paths.Length);
            }
        }

        // Make an accurate message
        var message = $"Successfully exported metadata to {mapsDone} {(mapsDone == 1 ? "beatmap" : "beatmaps")}!";
        return message;
	}

    public string AutoSavePath => Program.configPath + "/metadataproject.json";

    public string DefaultSaveFolder => Program.configPath + "/Metadata Manager Projects";

    public void SetSaveData(MetadataManagerViewModel saveData)
    {
		DataContext = saveData;
    }

    public MetadataManagerViewModel GetSaveData()
    {
        return (MetadataManagerViewModel)DataContext!;
    }
}