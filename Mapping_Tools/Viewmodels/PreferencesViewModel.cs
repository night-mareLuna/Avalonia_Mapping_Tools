using System;
using System.IO;
using Avalonia;
using Avalonia.Styling;
using Avalonia_Mapping_Tools.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Avalonia_Mapping_Tools.ViewModels;

public partial class PreferencesViewModel : ViewModelBase
{
	[ObservableProperty] private string? _OsuSongsFolder;
	[ObservableProperty] private string? _BackupsFolder;
	[ObservableProperty] private bool? _DarkTheme;
	private static PreferencesViewModel? Me;
	private bool IsFoldersInitialised = false;

	private enum Folder
	{
		osu = 0,
		backups = 1
	}
	
	public PreferencesViewModel()
	{
		Me = this;
		DarkTheme = Application.Current!.ActualThemeVariant == ThemeVariant.Dark;

		if((OsuSongsFolder == null || BackupsFolder == null) && !IsFoldersInitialised)
			SetFolders();
	}

	private async void SetFolders()
	{
		try
		{
			OsuSongsFolder = await JsonWriter.GetSong();
			BackupsFolder = await JsonWriter.GetBackup();
		}
		catch(Exception e)
		{
			Console.WriteLine(e.Message);
		}
		IsFoldersInitialised = true;
	}

#pragma warning disable CA1822 // Mark members as static
	public void SetTheme(bool darkTheme)
#pragma warning restore CA1822 // Mark members as static
	{
		Application.Current!.RequestedThemeVariant =
			darkTheme ? ThemeVariant.Dark : ThemeVariant.Light;

		JsonWriter.SetTheme(darkTheme);
	}

	public static void SetFolder(int folder, string path)
	{
		switch((Folder)folder)
		{
			case Folder.osu:
				Me!.OsuSongsFolder = path;
				break;
			case Folder.backups:
				Me!.BackupsFolder = path;
				break;
		}
	}
	
	partial void OnBackupsFolderChanged(string? value)
	{
		if(Directory.Exists(value) && IsFoldersInitialised)
			JsonWriter.SetBackup(value);
	}

	partial void OnOsuSongsFolderChanged(string? value)
	{
		if(Directory.Exists(value) && IsFoldersInitialised)
			JsonWriter.SetSong(value);
	}
}