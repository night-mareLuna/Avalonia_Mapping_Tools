using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using Mapping_Tools.Components.Domain;

namespace Avalonia_Mapping_Tools.ViewModels;

public partial class SliderMergerViewModel : ViewModelBase
{
	[ObservableProperty] [property: JsonIgnore] private int _Progress = 0;
	[JsonIgnore] public string[] Paths { get; set; }
	[JsonIgnore] public bool Quick { get; set; }
	[ObservableProperty] private ImportMode _ImportModeSetting;
	[JsonIgnore] public IEnumerable<ImportMode> ImportModes => Enum.GetValues(typeof(ImportMode)).Cast<ImportMode>();
	[ObservableProperty] [property: JsonIgnore] private bool _TimeCodeBoxVisibility = false;
	[ObservableProperty] private string _TimeCode;
	[ObservableProperty] private ConnectionMode _ConnectionModeSetting;
	[JsonIgnore] public IEnumerable<ConnectionMode> ConnectionModes => Enum.GetValues(typeof(ConnectionMode)).Cast<ConnectionMode>();
	[ObservableProperty] [property: GreaterThanOrEqual(0)] private double _Leniency;
	[ObservableProperty] private bool _LinearOnLinear;
	[ObservableProperty] private bool _MergeOnSliderEnd;
	public SliderMergerViewModel()
	{
        ImportModeSetting = ImportMode.Bookmarked;
        ConnectionModeSetting = ConnectionMode.Move;
        Leniency = 256;
        LinearOnLinear = false;
        MergeOnSliderEnd = true;
	}

    partial void OnImportModeSettingChanged(ImportMode value)
    {
        TimeCodeBoxVisibility = value == ImportMode.Time;
    }

    public enum ImportMode
	{
		Bookmarked,
		Time,
		Everything
	}

	public enum ConnectionMode
	{
		Move,
		Linear
	}
}