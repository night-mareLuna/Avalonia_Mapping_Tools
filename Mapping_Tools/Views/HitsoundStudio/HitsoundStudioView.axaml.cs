using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia_Mapping_Tools.ViewModels;
using Avalonia_Mapping_Tools.Views.HitsoundStudio;
using Mapping_Tools.Classes;
using Mapping_Tools.Classes.BeatmapHelper.Enums;
using Mapping_Tools.Classes.HitsoundStuff;
using Mapping_Tools.Classes.MathUtil;
using Mapping_Tools.Classes.SystemTools;
using Mapping_Tools.Classes.ToolHelpers;
using Mapping_Tools.Views;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Avalonia_Mapping_Tools.Views;
public partial class HitsoundStudioView : SingleRunMappingTool, ISavable<HitsoundStudioViewModel>, IHaveExtraProjectMenuItems
{
	private HitsoundStudioViewModel settings;
	private bool suppressEvents;
	private List<HitsoundLayer> selectedLayers;
	private HitsoundLayer? selectedLayer;
	private static HitsoundStudioView? Me;
	public HitsoundStudioView()
	{
		Me = this;
		InitializeComponent();
		SetLostFocusEvents();
		settings = new HitsoundStudioViewModel();
		DataContext = settings;
		LayersList.SelectedIndex = 0;
		Num_Layers_Changed();
		GetSelectedLayers();
		if(File.Exists(AutoSavePath))
			ProjectManager.LoadProject(this, message: false);
		else
			ProjectManager.SaveProject(this, AutoSavePath);
		Verbose = true;

		DefaultSampleSetBox.PlaceholderText = settings.DefaultSample.SampleSet.ToString();
	}

	private void SetLostFocusEvents()
	{
		SelectedNameBox.LostFocus += SelectedNameBox_TextChanged;
		SelectedSamplePathBox.LostFocus += SelectedSamplePathBox_TextChanged;
		SelectedSampleVolumeBox.LostFocus += SelectedSampleVolumeBox_TextChanged;
		SelectedSamplePanningBox.LostFocus += SelectedSamplePanningBox_TextChanged;
		SelectedSamplePitchShiftBox.LostFocus += SelectedSamplePitchShiftBox_TextChanged;
		SelectedSampleBankBox.LostFocus += SelectedSampleBankBox_TextChanged;
		SelectedSamplePatchBox.LostFocus += SelectedSamplePatchBox_TextChanged;
		SelectedSampleInstrumentBox.LostFocus += SelectedSampleInstrumentBox_TextChanged;
		SelectedSampleKeyBox.LostFocus += SelectedSampleKeyBox_TextChanged;
		SelectedSampleLengthBox.LostFocus += SelectedSampleLengthBox_TextChanged;
		SelectedSampleVelocityBox.LostFocus += SelectedSampleVelocityBox_TextChanged;
		SelectedImportPathBox.LostFocus += SelectedImportPathBox_TextChanged;
		SelectedImportXCoordBox.LostFocus += SelectedImportXCoordBox_TextChanged;
		SelectedImportYCoordBox.LostFocus += SelectedImportYCoordBox_TextChanged;
		SelectedImportSamplePathBox.LostFocus += SelectedImportSamplePathBox_TextChanged;
		SelectedStoryboardImportSamplePathBox.LostFocus += SelectedImportSamplePathBox_TextChanged;
		SelectedImportBankBox.LostFocus += SelectedImportBankBox_TextChanged;
		SelectedImportPatchBox.LostFocus += SelectedImportPatchBox_TextChanged;
		SelectedImportKeyBox.LostFocus += SelectedImportKeyBox_TextChanged;
		SelectedImportLengthBox.LostFocus += SelectedImportLengthBox_TextChanged;
		SelectedImportLengthRoughnessBox.LostFocus += SelectedImportLengthRoughnessBox_TextChanged;
		SelectedImportVelocityBox.LostFocus += SelectedImportVelocityBox_TextChanged;
		SelectedImportVelocityRoughnessBox.LostFocus += SelectedImportVelocityRoughnessBox_TextChanged;
		SelectedImportOffsetBox.LostFocus += SelectedImportOffsetBox_TextChanged;
	}

	private async void Start_Click(object obj, RoutedEventArgs args)
	{
		if(BackgroundWorker.IsBusy) return;
		
		var exportDialog = new HitsoundStudioExportDialog(settings);
		await MainWindow.ShowSomeDialog(exportDialog);
		bool result = exportDialog.result;

		if (!result) return;

        if (string.IsNullOrWhiteSpace(settings.BaseBeatmap))
        {
			var box = MessageBoxManager.GetMessageBoxStandard("Error!",
				"Please select a base beatmap first.",
				ButtonEnum.Ok);
			await box.ShowAsync();
            return;
        }

        if (settings.UsePreviousSampleSchema && settings.PreviousSampleSchema == null)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error!",
                "Can not use previous sample schema, because it has not been set by a previous run.\nPlease run the tool first without 'Use previous sample schema' enabled.",
                ButtonEnum.Ok);
            await box.ShowAsync();
            return;
        }

        if (!Directory.Exists(settings.ExportFolder))
        {
			var folderBox = MessageBoxManager.GetMessageBoxStandard("Export path not found.",
				$"Folder at path \"{settings.ExportFolder}\" does not exist.\nCreate a new folder?",
				ButtonEnum.YesNo);

			ButtonResult folderResult = await folderBox.ShowAsync();

            if (folderResult == ButtonResult.Yes) {
                try {
                    Directory.CreateDirectory(settings.ExportFolder);
                } catch (Exception ex) {
                    ex.Show();
                    return;
                }
            } else {
                return;
            }
        }

        BackgroundWorker.RunWorkerAsync(settings);
	}

	protected override void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
    {
        var bgw = sender as BackgroundWorker;
        e.Result = Make_Hitsounds((HitsoundStudioViewModel)e.Argument!, bgw!, e);
    }

    protected override void BackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
    {
        base.BackgroundWorker_ProgressChanged(sender, e);
		HitsoundStudioViewModel.SetProgress(Progress);
    }

    private string Make_Hitsounds(HitsoundStudioViewModel arg, BackgroundWorker worker, DoWorkEventArgs _) {
        string result = string.Empty;

        bool validateSampleFile =
            !(arg.SingleSampleExportFormat == HitsoundExporter.SampleExportFormat.MidiChords ||
              arg.MixedSampleExportFormat == HitsoundExporter.SampleExportFormat.MidiChords);

        var comparer = new SampleGeneratingArgsComparer(validateSampleFile);

        if (arg.HitsoundExportModeSetting == HitsoundStudioViewModel.HitsoundExportMode.Standard) {
            // Convert the multiple layers into packages that have the samples from all the layers at one specific time
            // Don't add default sample when exporting midi files because that's not a final export.
            List<SamplePackage> samplePackages = HitsoundConverter.ZipLayers(arg.HitsoundLayers, arg.DefaultSample, arg.ZipLayersLeniency, validateSampleFile);
            UpdateProgressBar(worker, 10);

            // Balance the volume between greenlines and samples
            HitsoundConverter.BalanceVolumes(samplePackages, 0, false);
            UpdateProgressBar(worker, 20);

            // Load the samples so validation can be done
            HashSet<SampleGeneratingArgs> allSampleArgs = new HashSet<SampleGeneratingArgs>(comparer);
            foreach (SamplePackage sp in samplePackages) {
                allSampleArgs.UnionWith(sp.Samples.Select(o => o.SampleArgs));
            }

            var loadedSamples = SampleImporter.ImportSamples(allSampleArgs, comparer);
            UpdateProgressBar(worker, 30);

            // Convert the packages to hitsounds that fit on an osu standard map
            CompleteHitsounds completeHitsounds =
                HitsoundConverter.GetCompleteHitsounds(samplePackages, loadedSamples, 
                    arg.UsePreviousSampleSchema ? arg.PreviousSampleSchema.GetCustomIndices() : null, 
                    arg.AllowGrowthPreviousSampleSchema, arg.FirstCustomIndex, validateSampleFile, comparer);
            UpdateProgressBar(worker, 60);

            // Save current sample schema
            if (!arg.UsePreviousSampleSchema) {
                arg.PreviousSampleSchema = new SampleSchema(completeHitsounds.CustomIndices);
            } else if (arg.AllowGrowthPreviousSampleSchema) {
                arg.PreviousSampleSchema.MergeWith(new SampleSchema(completeHitsounds.CustomIndices));
            }

            if (arg.ShowResults) {
                // Count the number of samples
                int samples = completeHitsounds.CustomIndices.SelectMany(ci => ci.Samples.Values)
                    .Count(h => h.Any(o => 
                        SampleImporter.ValidateSampleArgs(o, loadedSamples, validateSampleFile)));

                // Count the number of changes of custom index
                int greenlines = 0;
                int lastIndex = -1;
                foreach (var hit in completeHitsounds.Hitsounds.Where(hit => hit.CustomIndex != lastIndex)) {
                    lastIndex = hit.CustomIndex;
                    greenlines++;
                }

                result = $"Number of sample indices: {completeHitsounds.CustomIndices.Count}, " +
                         $"Number of samples: {samples}, Number of greenlines: {greenlines}";
            }

            if (arg.DeleteAllInExportFirst && (arg.ExportSamples || arg.ExportMap)) {
                // Delete all files in the export folder before filling it again
                DirectoryInfo di = new DirectoryInfo(arg.ExportFolder);
                foreach (FileInfo file in di.GetFiles()) {
                    file.Delete();
                }
            }

            UpdateProgressBar(worker, 70);

            // Export the hitsound map and sound samples
            if (arg.ExportMap) {
                HitsoundExporter.ExportHitsounds(completeHitsounds.Hitsounds, 
                    arg.BaseBeatmap, arg.ExportFolder, arg.HitsoundDiffName, arg.HitsoundExportGameMode, true, false);
            }

            UpdateProgressBar(worker, 80);

            if (arg.ExportSamples) {
                HitsoundExporter.ExportCustomIndices(completeHitsounds.CustomIndices, arg.ExportFolder,
                    loadedSamples, arg.SingleSampleExportFormat, arg.MixedSampleExportFormat, comparer);
            }

            UpdateProgressBar(worker, 99);
        } else if (arg.HitsoundExportModeSetting == HitsoundStudioViewModel.HitsoundExportMode.Coinciding) {
            List<SamplePackage> samplePackages = HitsoundConverter.ZipLayers(arg.HitsoundLayers, arg.DefaultSample, 0, false);

            HitsoundConverter.BalanceVolumes(samplePackages, 0, false, true);
            UpdateProgressBar(worker, 20);

            Dictionary<SampleGeneratingArgs, SampleSoundGenerator> loadedSamples = null;
            Dictionary<SampleGeneratingArgs, string> sampleNames = arg.UsePreviousSampleSchema ? arg.PreviousSampleSchema?.GetSampleNames(comparer) : null;
            Dictionary<SampleGeneratingArgs, Vector2> samplePositions = null;
            var hitsounds = HitsoundConverter.GetHitsounds(samplePackages, ref loadedSamples, ref sampleNames, ref samplePositions,
                arg.HitsoundExportGameMode == GameMode.Mania, arg.AddCoincidingRegularHitsounds, arg.AllowGrowthPreviousSampleSchema,
                validateSampleFile, comparer);

            // Save current sample schema
            if (!arg.UsePreviousSampleSchema || arg.PreviousSampleSchema == null) {
                arg.PreviousSampleSchema = new SampleSchema(sampleNames);
            } else if (arg.AllowGrowthPreviousSampleSchema) {
                arg.PreviousSampleSchema.MergeWith(new SampleSchema(sampleNames));
            }

            // Load the samples so validation can be done
            UpdateProgressBar(worker, 50);

            if (arg.ShowResults) {
                result = "Number of sample indices: 0, " +
                         $"Number of samples: {loadedSamples.Count}, Number of greenlines: 0";
            }

            if (arg.DeleteAllInExportFirst && (arg.ExportSamples || arg.ExportMap)) {
                // Delete all files in the export folder before filling it again
                DirectoryInfo di = new DirectoryInfo(arg.ExportFolder);
                foreach (FileInfo file in di.GetFiles()) {
                    file.Delete();
                }
            }
            UpdateProgressBar(worker, 60);

            if (arg.ExportMap) {
                HitsoundExporter.ExportHitsounds(hitsounds, 
                    arg.BaseBeatmap, arg.ExportFolder, arg.HitsoundDiffName, arg.HitsoundExportGameMode, false, false);
            }
            UpdateProgressBar(worker, 70);

            if (arg.ExportSamples) {
                HitsoundExporter.ExportLoadedSamples(loadedSamples, arg.ExportFolder, sampleNames, arg.SingleSampleExportFormat, comparer);
            }
        } else if (arg.HitsoundExportModeSetting == HitsoundStudioViewModel.HitsoundExportMode.Storyboard) {
            List<SamplePackage> samplePackages = HitsoundConverter.ZipLayers(arg.HitsoundLayers, arg.DefaultSample, 0, false);

            HitsoundConverter.BalanceVolumes(samplePackages, 0, false, true);
            UpdateProgressBar(worker, 20);

            Dictionary<SampleGeneratingArgs, SampleSoundGenerator> loadedSamples = null;
            Dictionary<SampleGeneratingArgs, string> sampleNames = arg.UsePreviousSampleSchema ? arg.PreviousSampleSchema?.GetSampleNames(comparer) : null;
            Dictionary<SampleGeneratingArgs, Vector2> samplePositions = null;
            var hitsounds = HitsoundConverter.GetHitsounds(samplePackages, ref loadedSamples, ref sampleNames, ref samplePositions,
                false, false, arg.AllowGrowthPreviousSampleSchema, validateSampleFile, comparer);

            // Save current sample schema
            if (!arg.UsePreviousSampleSchema || arg.PreviousSampleSchema == null) {
                arg.PreviousSampleSchema = new SampleSchema(sampleNames);
            } else if (arg.AllowGrowthPreviousSampleSchema) {
                arg.PreviousSampleSchema.MergeWith(new SampleSchema(sampleNames));
            }

            // Load the samples so validation can be done
            UpdateProgressBar(worker, 50);

            if (arg.ShowResults) {
                result = "Number of sample indices: 0, " +
                         $"Number of samples: {loadedSamples.Count}, Number of greenlines: 0";
            }

            if (arg.DeleteAllInExportFirst && (arg.ExportSamples || arg.ExportMap)) {
                // Delete all files in the export folder before filling it again
                DirectoryInfo di = new DirectoryInfo(arg.ExportFolder);
                foreach (FileInfo file in di.GetFiles()) {
                    file.Delete();
                }
            }
            UpdateProgressBar(worker, 60);

            if (arg.ExportMap) {
                HitsoundExporter.ExportHitsounds(hitsounds, 
                    arg.BaseBeatmap, arg.ExportFolder, arg.HitsoundDiffName, arg.HitsoundExportGameMode, false, true);
            }
            UpdateProgressBar(worker, 70);

            if (arg.ExportSamples) {
                HitsoundExporter.ExportLoadedSamples(loadedSamples, arg.ExportFolder, sampleNames, arg.SingleSampleExportFormat, comparer);
            }
        } else if (arg.HitsoundExportModeSetting == HitsoundStudioViewModel.HitsoundExportMode.Midi) {
            List<SamplePackage> samplePackages = HitsoundConverter.ZipLayers(arg.HitsoundLayers, arg.DefaultSample, 0, false);
            var beatmap = EditorReaderStuff.GetNewestVersionOrNot(arg.BaseBeatmap).Beatmap;

            if (arg.ShowResults) {
                result = $"Number of notes: {samplePackages.SelectMany(o => o.Samples).Count()}, " +
                        $"Number of volume changes: {(arg.AddGreenLineVolumeToMidi ? beatmap.BeatmapTiming.TimingPoints.Count : 0)}";
            }

            UpdateProgressBar(worker, 20);

            if (arg.DeleteAllInExportFirst &&  arg.ExportMap) {
                // Delete all files in the export folder before filling it again
                DirectoryInfo di = new DirectoryInfo(arg.ExportFolder);
                foreach (FileInfo file in di.GetFiles()) {
                    file.Delete();
                }
            }

            UpdateProgressBar(worker, 40);

            if (arg.ExportMap) {
                MidiExporter.ExportAsMidi(samplePackages, beatmap, Path.Combine(arg.ExportFolder, arg.HitsoundDiffName + ".mid"), arg.AddGreenLineVolumeToMidi);
            }
        }

        // Open export folder
        if (arg.ExportSamples || arg.ExportMap) {
			MainWindow.OpenFolder(arg.ExportFolder);
        }

        // Collect garbage
        GC.Collect();

        UpdateProgressBar(worker, 100);

        return result;
    }

	private async void Add_Click(object obj, RoutedEventArgs args)
	{
		suppressEvents = true;
		try
		{
			int layerCount = settings.HitsoundLayers.Count;
			HitsoundLayerImportWindow importWindow = new HitsoundLayerImportWindow(layerCount);
			await MainWindow.ShowSomeDialog(importWindow);
			
			LayersList.SelectedItems.Clear();
			foreach(HitsoundLayer layer in importWindow.HitsoundLayers)
			{
				if(layer is not null)
				{
					settings.HitsoundLayers.Add(layer);
					LayersList.SelectedItems.Add(layer);
				}
			}

			RecalculatePriorities();
			Num_Layers_Changed();
			GetSelectedLayers();
		}
		catch(Exception e)
		{
			e.Show();
		}
		suppressEvents = false;
	}

	private async void Delete_Click(object obj, RoutedEventArgs args)
	{

        try
        {
            // Ask for confirmation
			var box = MessageBoxManager.GetMessageBoxStandard("Confirm deletion",
				"Are you sure?",
				ButtonEnum.YesNo);
			var messageBoxResult = await box.ShowAsync();
            if (messageBoxResult != ButtonResult.Yes) { return; }

            if (selectedLayers.Count == 0 || selectedLayers == null) { return; }

            suppressEvents = true;

            int index = settings.HitsoundLayers.IndexOf(selectedLayer!);

            foreach (HitsoundLayer hsl in selectedLayers)
            {
                settings.HitsoundLayers.Remove(hsl);
            }
            suppressEvents = false;

            LayersList.SelectedIndex = Math.Max(Math.Min(index - 1, settings.HitsoundLayers.Count - 1), 0);

            RecalculatePriorities();
            Num_Layers_Changed();
        }
        catch (Exception ex) { ex.Show(); }
	}

	private void Raise_Click(object obj, RoutedEventArgs args)
	{
        try
        {
            int repeats = 1;
            for (int n = 0; n < repeats; n++)
            {
                suppressEvents = true;

                int selectedIndex = settings.HitsoundLayers.IndexOf(selectedLayer!);
                List<HitsoundLayer> moveList = new List<HitsoundLayer>();
                foreach (HitsoundLayer hsl in selectedLayers)
                {
                    moveList.Add(hsl);
                }

                foreach (HitsoundLayer hsl in settings.HitsoundLayers)
                {
                    if (moveList.Contains(hsl))
                    {
                        moveList.Remove(hsl);
                    }
                    else
                        break;
                }

                foreach (HitsoundLayer hsl in moveList)
                {
                    int index = settings.HitsoundLayers.IndexOf(hsl);

                    //Dont move left if it is the first item in the list or it is not in the list
                    if (index <= 0)
                        continue;

                    //Swap with this item with the one to its left
                    settings.HitsoundLayers.Remove(hsl);
                    settings.HitsoundLayers.Insert(index - 1, hsl);
                }

                LayersList.SelectedItems.Clear();
                foreach (HitsoundLayer hsl in selectedLayers)
                {
                    LayersList.SelectedItems.Add(hsl);
                }

                suppressEvents = false;

                RecalculatePriorities();
                GetSelectedLayers();
            }
        }
        catch (Exception ex) { ex.Show(); }
	}

	private void Lower_Click(object obj, RoutedEventArgs args)
	{
		try
        {
            int repeats = 1;
            for (int n = 0; n < repeats; n++)
            {
                suppressEvents = true;

                int selectedIndex = settings.HitsoundLayers.IndexOf(selectedLayer!);
                List<HitsoundLayer> moveList = new List<HitsoundLayer>();
                foreach (HitsoundLayer hsl in selectedLayers)
                {
                    moveList.Add(hsl);
                }

                for (int i = settings.HitsoundLayers.Count - 1; i >= 0; i--)
                {
                    HitsoundLayer hsl = settings.HitsoundLayers[i];
                    if (moveList.Contains(hsl))
                    {
                        moveList.Remove(hsl);
                    }
                    else
                        break;
                }

                for (int i = moveList.Count - 1; i >= 0; i--)
                {
                    HitsoundLayer hsl = moveList[i];
                    int index = settings.HitsoundLayers.IndexOf(hsl);

                    //Dont move left if it is the first item in the list or it is not in the list
                    if (index >= settings.HitsoundLayers.Count - 1)
                        continue;

                    //Swap with this item with the one to its left
                    settings.HitsoundLayers.Remove(hsl);
                    settings.HitsoundLayers.Insert(index + 1, hsl);
                }

                LayersList.SelectedItems.Clear();
                foreach (HitsoundLayer hsl in selectedLayers)
                {
                    LayersList.SelectedItems.Add(hsl);
                }

                suppressEvents = false;

                RecalculatePriorities();
                GetSelectedLayers();
            }
        }
        catch (Exception ex) { ex.Show(); }
	}

	private async void BaseBeatmapLoad_Click(object obj, RoutedEventArgs args)
	{
        try
        {
            string path = await IOHelper.GetCurrentBeatmap();
            if (path != "")
            {
                settings.BaseBeatmap = path;
            }
        } catch (Exception ex) { ex.Show(); }
	}

	private async void BaseBeatmapBrowse_Click(object obj, RoutedEventArgs args)
	{
        try
        {
            string[] paths = await IOHelper.BeatmapFileDialog(restore: SettingsManager.Settings.CurrentBeatmapDefaultFolder);
            if (paths.Length != 0)
            {
                settings.BaseBeatmap = paths[0];
            }
        } catch (Exception ex) { ex.Show(); }

	}

	private async void DefaultSampleBrowse_Click(object obj, RoutedEventArgs args)
	{
        try
        {
            string path = await IOHelper.SampleFileDialog();
            if (path != "")
            {
                settings.DefaultSample.SampleArgs.Path = path;
                DefaultSamplePathBox.Text = path;
            }
        } catch (Exception ex) { ex.Show(); }
	}

	private void ValidateSamples_Click(object obj, RoutedEventArgs args)
	{
		var couldNotFind = new List<HitsoundLayer>();
        var invalidExtension = new List<HitsoundLayer>();
        var couldNotLoad = new List<(HitsoundLayer, Exception)>();

        var allSampleArgs = settings.HitsoundLayers.Select(o => o.SampleArgs).ToList();
        var sampleExceptions = SampleImporter.ValidateSamples(allSampleArgs);

        foreach (HitsoundLayer hitsoundLayer in settings.HitsoundLayers) {
            if (string.IsNullOrEmpty(hitsoundLayer.SampleArgs.Path))
                continue;

            if (!sampleExceptions.TryGetValue(hitsoundLayer.SampleArgs, out var exception) || exception == null)
                continue;

            switch (exception) {
                case FileNotFoundException:
                    couldNotFind.Add(hitsoundLayer);
                    break;
                case InvalidDataException:
                    invalidExtension.Add(hitsoundLayer);
                    break;
                default:
                    couldNotLoad.Add((hitsoundLayer, exception));
                    break;
            }
        }


        if (couldNotFind.Count == 0 && invalidExtension.Count == 0 && couldNotLoad.Count == 0) {
			var successBox = MessageBoxManager.GetMessageBoxStandard("Success!",
				"All samples are valid!",
				ButtonEnum.Ok);
			successBox.ShowAsync();
            return;
        }

        var message = new StringBuilder();

        if (couldNotFind.Count > 0) {
            message.AppendLine("Could not find the following samples:");
            message.AppendLine(string.Join(Environment.NewLine, couldNotFind.Select(o => o.Name)));
            message.AppendLine();
        }

        if (invalidExtension.Count > 0) {
            message.AppendLine("The following samples have an invalid extension:");
            message.AppendLine(string.Join(Environment.NewLine, invalidExtension.Select(o => o.Name)));
            message.AppendLine();
        }

        if (couldNotLoad.Count > 0) {
            message.AppendLine("Could not load the following samples because of an exception:");
            message.AppendLine(string.Join(Environment.NewLine, couldNotLoad.Select(o => $"{o.Item1.Name}: {o.Item2.Message}")));
            message.AppendLine();
        }

		var errorBox = MessageBoxManager.GetMessageBoxStandard("Error!",
			message.ToString(),
			ButtonEnum.Ok);
        errorBox.ShowAsync();
	}

	private void SelectedNameBox_TextChanged(object? obj, RoutedEventArgs args)
	{
        if (suppressEvents) return;

        string t = (obj as TextBox)!.Text!;
        foreach (HitsoundLayer hitsoundLayer in selectedLayers)
        {
            hitsoundLayer.Name = t;
        }
	}

	private void SelectedSampleSetBox_SelectionChanged(object obj, SelectionChangedEventArgs args)
	{
        if (suppressEvents) return;

        string t = ((obj as ComboBox)!.SelectedItem as ComboBoxItem)!.Content!.ToString()!;
        foreach (HitsoundLayer hitsoundLayer in selectedLayers)
        {
            hitsoundLayer.SampleSetString = t;
        }
	}

	private void SelectedHitsoundBox_SelectionChanged(object obj, SelectionChangedEventArgs args)
	{
        if (suppressEvents) return;

        string t = ((obj as ComboBox)!.SelectedItem as ComboBoxItem)!.Content!.ToString()!;
        foreach (HitsoundLayer hitsoundLayer in selectedLayers)
        {
            hitsoundLayer.HitsoundString = t;
        }
	}

	private void SelectedSamplePathBox_TextChanged(object? obj, RoutedEventArgs args)
	{
        if (suppressEvents) return;

        string t = (obj as TextBox)!.Text!;
        foreach (HitsoundLayer hitsoundLayer in selectedLayers)
        {
            hitsoundLayer.SampleArgs.Path = t!;
        }
        UpdateEditingField();
	}

	private async void SelectedSamplePathBrowse_Click(object obj, RoutedEventArgs args)
	{
        try
        {
            string path = await IOHelper.SampleFileDialog();
            if (path != "")
            {
                SelectedSamplePathBox.Text = path;
            }
        } catch (Exception ex) { ex.Show(); }
	}

	private void SelectedSampleVolumeBox_TextChanged(object? obj, RoutedEventArgs args)
	{
        if (suppressEvents) return;

        double t = Math.Clamp((obj as TextBox)!.GetDouble(100), 0, 100);
        foreach (HitsoundLayer hitsoundLayer in selectedLayers)
        {
            hitsoundLayer.SampleArgs.Volume = t / 100;
        }
        UpdateEditingField();
	}

	private void SelectedSamplePanningBox_TextChanged(object? obj, RoutedEventArgs args)
	{
        if (suppressEvents) return;

        double t = (obj as TextBox)!.GetDouble(0);
        foreach (HitsoundLayer hitsoundLayer in selectedLayers)
        {
            hitsoundLayer.SampleArgs.Panning = t;
        }
        UpdateEditingField();
	}

	private void SelectedSamplePitchShiftBox_TextChanged(object? obj, RoutedEventArgs args)
	{
        if (suppressEvents) return;

        double t = (obj as TextBox)!.GetDouble(0);
        foreach (HitsoundLayer hitsoundLayer in selectedLayers)
        {
            hitsoundLayer.SampleArgs.PitchShift = t;
        }
        UpdateEditingField();
	}

	private void SelectedSampleBankBox_TextChanged(object? obj, RoutedEventArgs args)
	{
        if (suppressEvents) return;

        int t = (obj as TextBox)!.GetInt(-1);
        foreach (HitsoundLayer hitsoundLayer in selectedLayers)
        {
            hitsoundLayer.SampleArgs.Bank = t;
        }
	}

	private void SelectedSamplePatchBox_TextChanged(object? obj, RoutedEventArgs args)
	{
        if (suppressEvents) return;

        int t = (obj as TextBox)!.GetInt(-1);
        foreach (HitsoundLayer hitsoundLayer in selectedLayers)
        {
            hitsoundLayer.SampleArgs.Patch = t;
        }
	}

	private void SelectedSampleInstrumentBox_TextChanged(object? obj, RoutedEventArgs args)
	{
        if (suppressEvents) return;

        int t = (obj as TextBox)!.GetInt(-1);
        foreach (HitsoundLayer hitsoundLayer in selectedLayers)
        {
            hitsoundLayer.SampleArgs.Instrument = t;
        }
	}

	private void SelectedSampleKeyBox_TextChanged(object? obj, RoutedEventArgs args)
	{
        if (suppressEvents) return;

        int t = (obj as TextBox)!.GetInt(-1);
        foreach (HitsoundLayer hitsoundLayer in selectedLayers)
        {
            hitsoundLayer.SampleArgs.Key = t;
        }
	}

	private void SelectedSampleLengthBox_TextChanged(object? obj, RoutedEventArgs args)
	{
        if (suppressEvents) return;

        double t = (obj as TextBox)!.GetDouble(-1);
        foreach (HitsoundLayer hitsoundLayer in selectedLayers)
        {
            hitsoundLayer.SampleArgs.Length = t;
        }
	}

	private void SelectedSampleVelocityBox_TextChanged(object? obj, RoutedEventArgs args)
	{
        if (suppressEvents) return;

        int t = (obj as TextBox)!.GetInt(127);
        foreach (HitsoundLayer hitsoundLayer in selectedLayers)
        {
            hitsoundLayer.SampleArgs.Velocity = t;
        }
        UpdateEditingField();
	}

	private void SelectedImportTypeBox_SelectionChanged(object obj, SelectionChangedEventArgs args)
	{
        if (suppressEvents) return;

        string t = ((obj as ComboBox)!.SelectedItem as ComboBoxItem)!.Content!.ToString()!;
        ImportType type = (ImportType)Enum.Parse(typeof(ImportType), t);
        foreach (HitsoundLayer hitsoundLayer in selectedLayers)
        {
            hitsoundLayer.ImportArgs.ImportType = type;
        }
        UpdateEditingField();
	}

	private void DefaultSampleSet_SelectionChanged(object obj, SelectionChangedEventArgs args)
	{	//A BANDAID FIX TO BINDING CONVERTER NOT WORKING
		string t = ((obj as ComboBox)!.SelectedItem as ComboBoxItem)!.Content!.ToString()!;
		SampleSet set = (SampleSet)Enum.Parse(typeof(SampleSet), t);
		settings.DefaultSample.SampleSet = set;
	}

	private void SelectedImportPathBox_TextChanged(object? obj, RoutedEventArgs args)
	{
        if (suppressEvents) return;

        string t = (obj as TextBox)!.Text!;
        foreach (HitsoundLayer hitsoundLayer in selectedLayers)
        {
            hitsoundLayer.ImportArgs.Path = t;
        }
	}

	private async void SelectedImportPathLoad_Click(object obj, RoutedEventArgs args)
	{
        try
        {
            string path = await IOHelper.GetCurrentBeatmap();
            if (path != "")
            {
                SelectedImportPathBox.Text = path;
            }
        }
        catch (Exception ex) { ex.Show(); }
	}

	private async void SelectedImportPathBrowse_Click(object obj, RoutedEventArgs args)
	{
        try
        {
            string path = await IOHelper.FileDialog();
            if (!string.IsNullOrEmpty(path))
            {
                SelectedImportPathBox.Text = path;
            }
        } catch (Exception ex) { ex.Show(); }
	}

	private void SelectedImportXCoordBox_TextChanged(object? obj, RoutedEventArgs args)
	{
        if (suppressEvents) return;

        double t = (obj as TextBox)!.GetDouble(-1);
        foreach (HitsoundLayer hitsoundLayer in selectedLayers)
        {
            hitsoundLayer.ImportArgs.X = t;
        }
	}

	private void SelectedImportYCoordBox_TextChanged(object? obj, RoutedEventArgs args)
	{
        if (suppressEvents) return;

        double t = (obj as TextBox)!.GetDouble(-1);
        foreach (HitsoundLayer hitsoundLayer in selectedLayers)
        {
            hitsoundLayer.ImportArgs.Y = t;
        }
	}

	private void SelectedImportSamplePathBox_TextChanged(object? obj, RoutedEventArgs args)
	{
        if (suppressEvents) return;

        string t = (obj as TextBox)!.Text!;
        foreach (HitsoundLayer hitsoundLayer in selectedLayers)
        {
            hitsoundLayer.ImportArgs.SamplePath = t;
        }
	}

	private async void SelectedImportSamplePathBrowse_Click(object obj, RoutedEventArgs args)
	{
        try
        {
            string path = await IOHelper.SampleFileDialog();
            if (path != "")
            {
                SelectedImportSamplePathBox.Text = path;
                SelectedStoryboardImportSamplePathBox.Text = path;
            }
        } catch (Exception ex) { ex.Show(); }
	}

	private void SelectedImportDiscriminateVolumesBox_OnCheck(object obj, RoutedEventArgs args)
	{
        if (suppressEvents) return;

		bool isChecked = (obj as CheckBox)!.IsChecked.GetValueOrDefault();
        foreach (HitsoundLayer hitsoundLayer in selectedLayers) {
            hitsoundLayer.ImportArgs.DiscriminateVolumes = isChecked;
        }
	}

	private void SelectedHitsoundImportDetectDuplicateSamplesBox_OnCheck(object obj, RoutedEventArgs args)
	{
        if (suppressEvents) return;

		bool isChecked = (obj as CheckBox)!.IsChecked.GetValueOrDefault();
        foreach (HitsoundLayer hitsoundLayer in selectedLayers) {
            hitsoundLayer.ImportArgs.DetectDuplicateSamples = isChecked;
        }
	}

	private void SelectedImportRemoveDuplicatesBox_OnCheck(object obj, RoutedEventArgs args)
	{
        if (suppressEvents) return;

		bool isChecked = (obj as CheckBox)!.IsChecked.GetValueOrDefault();
        foreach (HitsoundLayer hitsoundLayer in selectedLayers) {
            hitsoundLayer.ImportArgs.RemoveDuplicates = isChecked;
        }
	}

	private void SelectedImportBankBox_TextChanged(object? obj, RoutedEventArgs args)
	{
        if (suppressEvents) return;

        int t = (obj as TextBox)!.GetInt(-1);
        foreach (HitsoundLayer hitsoundLayer in selectedLayers)
        {
            hitsoundLayer.ImportArgs.Bank = t;
        }
	}

	private void SelectedImportPatchBox_TextChanged(object? obj, RoutedEventArgs args)
	{
        if (suppressEvents) return;

        int t = (obj as TextBox)!.GetInt(-1);
        foreach (HitsoundLayer hitsoundLayer in selectedLayers)
        {
            hitsoundLayer.ImportArgs.Patch = t;
        }
	}

	private void SelectedImportKeyBox_TextChanged(object? obj, RoutedEventArgs args)
	{
        if (suppressEvents) return;

        int t = (obj as TextBox)!.GetInt(-1);
        foreach (HitsoundLayer hitsoundLayer in selectedLayers)
        {
            hitsoundLayer.ImportArgs.Key = t;
        }
	}

	private void SelectedImportLengthBox_TextChanged(object? obj, RoutedEventArgs args)
	{
        if (suppressEvents) return;

        int t = (obj as TextBox)!.GetInt(-1);
        foreach (HitsoundLayer hitsoundLayer in selectedLayers)
        {
            hitsoundLayer.ImportArgs.Length = t;
        }
	}

	private void SelectedImportLengthRoughnessBox_TextChanged(object? obj, RoutedEventArgs args)
	{
        if (suppressEvents) return;

        double t = (obj as TextBox)!.GetDouble(1);
        foreach (HitsoundLayer hitsoundLayer in selectedLayers)
        {
            hitsoundLayer.ImportArgs.LengthRoughness = t;
        }
	}

	private void SelectedImportVelocityBox_TextChanged(object? obj, RoutedEventArgs args)
	{
        if (suppressEvents) return;

        int t = (obj as TextBox)!.GetInt(-1);
        foreach (HitsoundLayer hitsoundLayer in selectedLayers)
        {
            hitsoundLayer.ImportArgs.Velocity = t;
        }
	}

	private void SelectedImportVelocityRoughnessBox_TextChanged(object? obj, RoutedEventArgs args)
	{
        if (suppressEvents) return;

        double t = (obj as TextBox)!.GetDouble(1);
        foreach (HitsoundLayer hitsoundLayer in selectedLayers)
        {
            hitsoundLayer.ImportArgs.VelocityRoughness = t;
        }
	}

	private void SelectedImportOffsetBox_TextChanged(object? obj, RoutedEventArgs args)
	{
        if (suppressEvents) return;

        double t = (obj as TextBox)!.GetDouble(0);
        foreach (HitsoundLayer hitsoundLayer in selectedLayers)
        {
            hitsoundLayer.ImportArgs.Offset = t;
        }
	}

	private void ReloadFromSource_Click(object obj, RoutedEventArgs args)
	{
		try
        {
			Console.WriteLine("Reloaded");
            var seperatedByImportArgsForReloading = new Dictionary<ImportReloadingArgs, List<HitsoundLayer>>(new ImportReloadingArgsComparer());

            foreach (var layer in selectedLayers)
            {
                var reloadingArgs = layer.ImportArgs.GetImportReloadingArgs();
                if (seperatedByImportArgsForReloading.TryGetValue(reloadingArgs, out List<HitsoundLayer> value))
                {
                    value.Add(layer);
                }
                else
                {
                    seperatedByImportArgsForReloading.Add(reloadingArgs, new List<HitsoundLayer> { layer });
                }
            }

            foreach (var pair in seperatedByImportArgsForReloading)
            {
                var reloadingArgs = pair.Key;
                var layers = pair.Value;

                var importedLayers = HitsoundImporter.ImportReloading(reloadingArgs);

                layers.ForEach(o => o.Reload(importedLayers));
            }

            UpdateEditingField();
        }
        catch (Exception ex) { ex.Show(); }
	}

	private void LayersList_SelectionChanged(object obj, SelectionChangedEventArgs args)
	{
        if (suppressEvents) return;

        GetSelectedLayers();
        UpdateEditingField();
	}

	public static HitsoundLayer? GetSelectedLayer()
	{
		return Me!.selectedLayer;
	}

	private void RecalculatePriorities()
	{
		for(int i = 0; i < settings.HitsoundLayers.Count; i++)
			settings.HitsoundLayers[i].Priority = i;
	}

	private void Num_Layers_Changed()
    {
        if (settings.HitsoundLayers.Count == 0)
        {
            FirstGrid.ColumnDefinitions[0].Width = new GridLength(0);
            EditPanel.IsEnabled = false;
        }
        else if (FirstGrid.ColumnDefinitions[0].Width.Value < 100)
        {
            FirstGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
            FirstGrid.ColumnDefinitions[2].Width = new GridLength(2, GridUnitType.Star);
            EditPanel.IsEnabled = true;
        }
    }

	private void GetSelectedLayers()
    {
        selectedLayers = new List<HitsoundLayer>();

        if (LayersList.SelectedItems.Count == 0)
        {
            selectedLayer = null;
            return;
        }

        foreach (HitsoundLayer hsl in LayersList.SelectedItems)
        {
            selectedLayers.Add(hsl);
        }

        selectedLayer = selectedLayers[0];
    }

	private void UpdateEditingField()
    {
        if (selectedLayers.Count == 0) { return; }

        suppressEvents = true;
        // Populate the editing fields
        SelectedNameBox.Text = selectedLayers.AllToStringOrDefault(o => o.Name);
        SelectedSampleSetBox.PlaceholderText = selectedLayers.AllToStringOrDefault(o => o.SampleSetString);
        SelectedHitsoundBox.PlaceholderText = selectedLayers.AllToStringOrDefault(o => o.HitsoundString);
        TimesBox.Text = selectedLayers.AllToStringOrDefault(o => o.Times, HitsoundLayerExtension.DoubleListToStringConverter);

        SelectedSamplePathBox.Text = selectedLayers.AllToStringOrDefault(o => o.SampleArgs.Path);
        SelectedSampleVolumeBox.Text = selectedLayers.AllToStringOrDefault(o => Math.Round(o.SampleArgs.Volume * 100), CultureInfo.InvariantCulture);
        SelectedSamplePanningBox.Text = selectedLayers.AllToStringOrDefault(o => o.SampleArgs.Panning, CultureInfo.InvariantCulture);
        SelectedSamplePitchShiftBox.Text = selectedLayers.AllToStringOrDefault(o => o.SampleArgs.PitchShift, CultureInfo.InvariantCulture);
        SelectedSampleBankBox.Text = selectedLayers.AllToStringOrDefault(o => o.SampleArgs.Bank);
        SelectedSamplePatchBox.Text = selectedLayers.AllToStringOrDefault(o => o.SampleArgs.Patch);
        SelectedSampleInstrumentBox.Text = selectedLayers.AllToStringOrDefault(o => o.SampleArgs.Instrument);
        SelectedSampleKeyBox.Text = selectedLayers.AllToStringOrDefault(o => o.SampleArgs.Key);
        SelectedSampleLengthBox.Text = selectedLayers.AllToStringOrDefault(o => o.SampleArgs.Length, CultureInfo.InvariantCulture);
        SelectedSampleVelocityBox.Text = selectedLayers.AllToStringOrDefault(o => o.SampleArgs.Velocity);

        SelectedImportTypeBox.PlaceholderText = selectedLayers.AllToStringOrDefault(o => o.ImportArgs.ImportType);
        SelectedImportPathBox.Text = selectedLayers.AllToStringOrDefault(o => o.ImportArgs.Path);
        SelectedImportXCoordBox.Text = selectedLayers.AllToStringOrDefault(o => o.ImportArgs.X, CultureInfo.InvariantCulture);
        SelectedImportYCoordBox.Text = selectedLayers.AllToStringOrDefault(o => o.ImportArgs.Y, CultureInfo.InvariantCulture);
        SelectedImportSamplePathBox.Text = selectedLayers.AllToStringOrDefault(o => o.ImportArgs.SamplePath);
        SelectedHitsoundImportDiscriminateVolumesBox.IsChecked = selectedLayers.All(o => o.ImportArgs.DiscriminateVolumes);
        SelectedHitsoundImportDetectDuplicateSamplesBox.IsChecked = selectedLayers.All(o => o.ImportArgs.DetectDuplicateSamples);
        SelectedHitsoundImportRemoveDuplicatesBox.IsChecked = selectedLayers.All(o => o.ImportArgs.RemoveDuplicates);
        SelectedStoryboardImportSamplePathBox.Text = selectedLayers.AllToStringOrDefault(o => o.ImportArgs.SamplePath);
        SelectedStoryboardImportDiscriminateVolumesBox.IsChecked = selectedLayers.All(o => o.ImportArgs.DiscriminateVolumes);
        SelectedStoryboardImportRemoveDuplicatesBox.IsChecked = selectedLayers.All(o => o.ImportArgs.RemoveDuplicates);
        SelectedImportBankBox.Text = selectedLayers.AllToStringOrDefault(o => o.ImportArgs.Bank);
        SelectedImportPatchBox.Text = selectedLayers.AllToStringOrDefault(o => o.ImportArgs.Patch);
        SelectedImportKeyBox.Text = selectedLayers.AllToStringOrDefault(o => o.ImportArgs.Key);
        SelectedImportLengthBox.Text = selectedLayers.AllToStringOrDefault(o => o.ImportArgs.Length, CultureInfo.InvariantCulture);
        SelectedImportLengthRoughnessBox.Text = selectedLayers.AllToStringOrDefault(o => o.ImportArgs.LengthRoughness, CultureInfo.InvariantCulture);
        SelectedImportVelocityBox.Text = selectedLayers.AllToStringOrDefault(o => o.ImportArgs.Velocity);
        SelectedImportVelocityRoughnessBox.Text = selectedLayers.AllToStringOrDefault(o => o.ImportArgs.VelocityRoughness, CultureInfo.InvariantCulture);
        SelectedImportOffsetBox.Text = selectedLayers.AllToStringOrDefault(o => o.ImportArgs.Offset, CultureInfo.InvariantCulture);

        // Update visibility
        SoundFontArgsPanel.IsVisible = selectedLayers.Any(o => o.SampleArgs.UsesSoundFont || string.IsNullOrEmpty(o.SampleArgs.GetExtension()));
        SelectedStackPanel.IsVisible = selectedLayers.Any(o => o.ImportArgs.ImportType == ImportType.Stack);
        SelectedHitsoundsPanel.IsVisible = selectedLayers.Any(o => o.ImportArgs.ImportType == ImportType.Hitsounds);
        SelectedStoryboardPanel.IsVisible = selectedLayers.Any(o => o.ImportArgs.ImportType == ImportType.Storyboard);
        SelectedMidiPanel.IsVisible = selectedLayers.Any(o => o.ImportArgs.ImportType == ImportType.MIDI);
        ImportArgsPanel.IsVisible = selectedLayers.Any(o => o.ImportArgs.CanImport);

        suppressEvents = false;
    }

    public string AutoSavePath => Program.configPath + "/hsstudioproject.json";

    public string DefaultSaveFolder => Program.configPath + "/Hitsound Studio Projects";

    public MenuItem[] GetMenuItems()
    {
		var loadSampleSchemaMenu = new MenuItem {
                Header = "_Load sample schema"
            };
            loadSampleSchemaMenu.Click += LoadSampleSchemaFromFile;

            var bulkAssignSamplesMenu = new MenuItem {
                Header = "_Bulk assign samples"
            };
            bulkAssignSamplesMenu.Click += BulkAssignSamples;

            return new[] {
                loadSampleSchemaMenu,
                bulkAssignSamplesMenu,
            };
    }

	private async void LoadSampleSchemaFromFile(object sender, RoutedEventArgs e) {
        try {
            var project = await ProjectManager.GetProject(this, true);
            settings.PreviousSampleSchema = project.PreviousSampleSchema;

            //Task.Factory.StartNew(() => MainWindow.MessageQueue.Enqueue("Successfully loaded sample schema!"));
			await Task.Factory.StartNew(() =>
				MessageBoxManager.GetMessageBoxStandard("Success!",
                    "Successfully loaded sample schema!",
					ButtonEnum.Ok).ShowAsync()
			);
        } catch (ArgumentException) { }
        catch (Exception ex) {
            ex.Show();
        }
    }

	private async void BulkAssignSamples(object sender, RoutedEventArgs e) {
        try {
            var result = await IOHelper.AudioFileDialog(true);

            foreach (string path in result) {
                // The file name is expected to be in the following shape:
                // [bank]_[patch]_[key]_[length]_[velocity].[extension]
                // Example: 0_39_256_100.wav
                var fileName = Path.GetFileNameWithoutExtension(path);
                var split = fileName.Split('_');
                int? bank = split.Length > 0 && int.TryParse(split[0], out var bankParsed) ? bankParsed : null;
                int? patch = split.Length > 1 && int.TryParse(split[1], out var patchParsed) ? patchParsed : null;
                int? key = split.Length > 2 && int.TryParse(split[2], out var keyParsed) ? keyParsed : null;
                int? length = split.Length > 3 && int.TryParse(split[3], out var lengthParsed) ? lengthParsed : null;
                int? velocity = split.Length > 4 && int.TryParse(split[4], out var velocityParsed) ? velocityParsed : null;

                foreach (var layer in selectedLayers) {
                    if (bank.HasValue && bank.Value != layer.ImportArgs.Bank) continue;
                    if (patch.HasValue && patch.Value != layer.ImportArgs.Patch) continue;
                    if (key.HasValue && key.Value != layer.ImportArgs.Key) continue;
                    if (length.HasValue && length.Value != (int)Math.Round(layer.ImportArgs.Length)) continue;
                    if (velocity.HasValue && velocity.Value != layer.ImportArgs.Velocity) continue;

                    layer.SampleArgs.Path = path;
                }
            }


        } catch (Exception ex) {
            ex.Show();
        }
    }

    public HitsoundStudioViewModel GetSaveData()
    {
        return (HitsoundStudioViewModel) DataContext!;
    }

    public void SetSaveData(HitsoundStudioViewModel saveData)
    {
        suppressEvents = true;

        settings = saveData;
        DataContext = settings;

        suppressEvents = false;

        LayersList.SelectedIndex = 0;
        Num_Layers_Changed();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
		ProjectManager.SaveProject(this, AutoSavePath);
        base.OnUnloaded(e);
    }
}