using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia_Mapping_Tools.ViewModels;
using Mapping_Tools.Classes;
using Mapping_Tools.Classes.BeatmapHelper.Enums;
using Mapping_Tools.Classes.HitsoundStuff;
using Mapping_Tools.Classes.SystemTools;
using Mapping_Tools.Views;

namespace Avalonia_Mapping_Tools.Views.HitsoundStudio;
public partial class HitsoundLayerImportWindow : Window
{
	private readonly int index;
	public List<HitsoundLayer> HitsoundLayers;

	public HitsoundLayerImportWindow()
	{
		InitializeComponent();
		HitsoundLayers = [];
		index = 0;
		string[] currentMaps = MainWindowViewModel.GetCurrentMaps();
		if(currentMaps.Length > 0)
		{
			BeatmapPathBox.Text = currentMaps[0];
			BeatmapPathBox2.Text = string.Join('|', currentMaps);
		}
	}

	public HitsoundLayerImportWindow(int i)
	{
        InitializeComponent();
        HitsoundLayers = new List<HitsoundLayer>();
        index = i;
        var suggestedName = $"Layer {index + 1}";
        NameBox0.Text = suggestedName;
        NameBox.Text = suggestedName;
        NameBox2.Text = suggestedName;
        NameBox3.Text = suggestedName;
        NameBox4.Text = suggestedName;
		string[] currentMaps = MainWindowViewModel.GetCurrentMaps();
		if(currentMaps.Length > 0)
		{
			BeatmapPathBox.Text = currentMaps[0];
			BeatmapPathBox2.Text = string.Join('|', currentMaps);
			BeatmapPathBox3.Text = string.Join('|', currentMaps);
		}
    }


    private async void BeatmapBrowse_Click(object sender, RoutedEventArgs e) {
        string[] paths = await IOHelper.BeatmapFileDialog(restore: SettingsManager.Settings.CurrentBeatmapDefaultFolder);
        if( paths.Length != 0 ) { BeatmapPathBox.Text = paths[0]; }
    }

	private async void BeatmapLoad_Click(object sender, RoutedEventArgs e) {
        try {
            string path = await IOHelper.GetCurrentBeatmap();
            if (path != "") { BeatmapPathBox.Text = path; }
        }
        catch (Exception ex) {
            ex.Show();
        }
    }

	private async void BeatmapBrowse2_Click(object sender, RoutedEventArgs e) {
        string[] paths = await IOHelper.BeatmapFileDialog(restore: SettingsManager.Settings.CurrentBeatmapDefaultFolder);
        if( paths.Length != 0 ) { BeatmapPathBox2.Text = string.Join("|", paths); }
    }

	private async void BeatmapLoad2_Click(object sender, RoutedEventArgs e) {
        try {
            string path = await IOHelper.GetCurrentBeatmap();
            if (path != "") { BeatmapPathBox2.Text = path; }
        }
        catch (Exception ex) {
            ex.Show();
        }
    }

	private async void BeatmapBrowse4_Click(object sender, RoutedEventArgs e) {
        string[] paths = await IOHelper.BeatmapFileDialog(restore: SettingsManager.Settings.CurrentBeatmapDefaultFolder);
        if( paths.Length != 0 ) { BeatmapPathBox4.Text = string.Join("|", paths); }
    }

    private async void BeatmapLoad4_Click(object sender, RoutedEventArgs e) {
        try {
            string path = await IOHelper.GetCurrentBeatmap();
            if (path != "") { BeatmapPathBox4.Text = path; }
        }
        catch (Exception ex) {
            ex.Show();
        }
    }

    private async void MIDIBrowse3_Click(object sender, RoutedEventArgs e) {
        string path = await IOHelper.MidiFileDialog();
        if( path != "" ) { BeatmapPathBox3.Text = path; }
    }

    private async void SampleBrowse0_Click(object sender, RoutedEventArgs e) {
        string path = await IOHelper.SampleFileDialog();
        if( path != "" ) { SamplePathBox0.Text = path; }
    }

    private async void SampleBrowse_Click(object sender, RoutedEventArgs e) {
        string path = await IOHelper.SampleFileDialog();
        if( path != "" ) { SamplePathBox.Text = path; }
    }

    private void Add_Click(object sender, RoutedEventArgs e) {
        try {
            if( Tabs.SelectedIndex == 1 ) {
                // Import one layer
                HitsoundLayer layer = HitsoundImporter.ImportStack(BeatmapPathBox.Text!, XCoordBox.GetDouble(), YCoordBox.GetDouble());
                layer.Name = NameBox.Text!;
                layer.SampleSet = (SampleSet) ( SampleSetBox.SelectedIndex + 1 );
                layer.Hitsound = (Hitsound) HitsoundBox.SelectedIndex;
                layer.SampleArgs.Path = SamplePathBox.Text!;

                HitsoundLayers.Add(layer);
            }
            else if( Tabs.SelectedIndex == 2 ) {
                // Import complete hitsounds
                foreach( string path in BeatmapPathBox2!.Text!.Split('|') ) {
                    HitsoundLayers.AddRange(HitsoundImporter.ImportHitsounds(path, VolumesBox2.IsChecked.GetValueOrDefault(),
                        DetectDuplicateSamplesBox2.IsChecked.GetValueOrDefault(),
                        RemoveDuplicatesBox2.IsChecked.GetValueOrDefault(),
                        IncludeStoryboardBox2.IsChecked.GetValueOrDefault()));
                }
                HitsoundLayers.ForEach(o => o.Name = $"{NameBox2.Text}: {o.Name}");
            }
            else if( Tabs.SelectedIndex == 3 ) {
                // Import MIDI
                HitsoundLayers = HitsoundImporter.ImportMidi(BeatmapPathBox3.Text!, OffsetBox3.GetDouble(0),
                    InstrumentBox3.IsChecked.GetValueOrDefault(), KeysoundBox3.IsChecked.GetValueOrDefault(),
                    LengthBox3.IsChecked.GetValueOrDefault(), LengthRoughnessBox3.GetDouble(2),
                    VelocityBox3.IsChecked.GetValueOrDefault(), VelocityRoughnessBox3.GetDouble(10));
                HitsoundLayers.ForEach(o => o.Name = $"{NameBox3.Text}: {o.Name}");
            }
            else if( Tabs.SelectedIndex == 4 ) {
                // Import storyboarded samples
                foreach( string path in BeatmapPathBox4.Text!.Split('|') ) {
                    HitsoundLayers.AddRange(HitsoundImporter.ImportStoryboard(path, VolumesBox4.IsChecked.GetValueOrDefault(), 
                        RemoveDuplicatesBox4.IsChecked.GetValueOrDefault()));
                }
                HitsoundLayers.ForEach(o => o.Name = $"{NameBox4.Text}: {o.Name}");
            }
            else {
                // Import none
                HitsoundLayer layer = new HitsoundLayer(NameBox0.Text!, ImportType.None, (SampleSet) ( SampleSetBox0.SelectedIndex + 1 ), (Hitsound) HitsoundBox0.SelectedIndex, SamplePathBox0.Text);
                HitsoundLayers.Add(layer);
            }

            Close();
        }
        catch( Exception ex ) {
            ex.Show();
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) {
        Close();
    }
}
