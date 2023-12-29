using Mapping_Tools.Classes;
using Mapping_Tools.Classes.HitsoundStuff;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Avalonia_Mapping_Tools.ViewModels;

public partial class HitsoundPreviewHelperViewModel : ViewModelBase
{
	[ObservableProperty] private ObservableCollection<HitsoundZone> _Items;
	[ObservableProperty] [property: JsonIgnore] private int _Progress = 0;
	private static HitsoundPreviewHelperViewModel? Me;
	public HitsoundPreviewHelperViewModel()
	{
		Me = this;
		Items = [];
	}

	public void AddCommand()
	{
		Items.Add(new());
	}

	public void RemoveCommand()
	{
		try
		{
			Items.RemoveAll(o => o.IsSelected);
		}
		catch(Exception e)
		{
			e.Show();
		}
	}

	public async void DeleteAllCommand()
	{
		var box = MessageBoxManager.GetMessageBoxStandard("",
			"Are you sure you want to DELETE ALL hitsound zones and start from scratch?",
			ButtonEnum.YesNo);
		var result = await box.ShowAsync();
		if(result == ButtonResult.Yes)
		{
			SelectAll(true, Items);
			RemoveCommand();
		}
		else return;
	}

	public void CopyCommand()
	{
		try
		{
            int initialCount = Items.Count;
            for (int i = 0; i < initialCount; i++) {
                if (Items[i].IsSelected) {
                    Items.Add(Items[i].Copy());
                }
            }
        } catch (Exception ex) { ex.Show(); }
	}

	private static void SelectAll(bool select, IEnumerable<HitsoundZone> models) {
        foreach (var model in models) {
            model.IsSelected = select;
        }
    }

	public static void SetProgress(int prog) => Me!.Progress = prog;
}