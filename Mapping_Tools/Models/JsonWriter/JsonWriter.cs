using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Avalonia_Mapping_Tools.Models;

public class JsonWriter
{
	public static async Task<string?> GetSong()
	{
		Config? json = await ReadJson();
		return json!.SongFolder;
	}

	public static async Task<string?> GetBackup()
	{
		Config? json = await ReadJson();
		return json!.BackupFolder;
	}

	public static async Task<string?> GetCurrentMap()
	{
		Config? json = await ReadJson();
		return json!.SelectedMap;
	}

	public static async void SetBackup(string folder)
	{
		Config? json = await ReadJson();
		if(json is not null)
			json.BackupFolder = folder;
		else
			json = new Config
			{
				BackupFolder = folder
			};

		SaveJson(json);
	}

	public static async void SetSong(string folder)
	{
		Config? json = await ReadJson();
		if(json is not null)
			json.SongFolder = folder;
		else
			json = new Config
			{
				SongFolder = folder
			};

		SaveJson(json);
	}

	public static async void SetCurrentMap(string? file)
	{
		Config? json = await ReadJson();
		if(json is not null)
			json.SelectedMap = file;
		else
			json = new Config
			{
				SelectedMap = file
			};

		SaveJson(json);
	}

	public static async Task<bool?> GetTheme()
	{
		Config? json = await ReadJson();
		return json?.DarkTheme;
	}

	public static async void SetTheme(bool theme)
	{
		Config? json = await ReadJson();
		if(json is not null)
			json.DarkTheme = theme;
		else
			json = new Config
			{
				DarkTheme = theme
			};

		SaveJson(json);
	}

	private static async Task<Config?> ReadJson(string name = "config.json")
	{
		string fileName = name;
		try
		{
			using FileStream openStream = File.OpenRead(fileName);
			Config? json = 
				await JsonSerializer.DeserializeAsync<Config>(openStream);
			return json;
		}
		catch(Exception e)
		{
			Console.WriteLine(e.Message);
			return null;
		}
	}

	private static async void SaveJson(Config json, string name = "config.json")
	{
		string fileName = name;
		try
		{
			using FileStream createStream = File.Create(fileName);
			await JsonSerializer.SerializeAsync(createStream, json);
			await createStream.DisposeAsync();
		}
		catch(Exception e)
		{
			Console.WriteLine(e);
		}
	}
}

public class Config
{
	public string? SelectedMap { get; set; }
	public string? SongFolder { get; set; }
	public string? BackupFolder { get; set; }
	public bool? DarkTheme { get; set; }
}