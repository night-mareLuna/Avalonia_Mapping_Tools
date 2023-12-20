using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Avalonia_Mapping_Tools.Models;

public class JsonWriter
{
	public static void CreateEmpty(string name = "config.json")
	{
		if(Directory.Exists(Program.configPath))
			return;

		Config json = new Config
		{
			SelectedMaps = null,
			SongFolder = null,
			BackupFolder = Program.configPath + "/Backups",
			DarkTheme = null
		};

		Console.WriteLine($"Creating folder {Program.configPath} and subfolders");
		Directory.CreateDirectory(Program.configPath + "/Backups");
		SaveJson(json, name);
	}
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

	public static async Task<string[]?> GetCurrentMap()
	{
		Config? json = await ReadJson();
		return json!.SelectedMaps;
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

	public static async void SetCurrentMaps(string[]? file)
	{
		Config? json = await ReadJson();
		if(json is not null)
			json.SelectedMaps = file;
		else
			json = new Config
			{
				SelectedMaps = file
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
		string fileName = Program.configPath + '/' + name;
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
		string fileName = Program.configPath + '/' + name;
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
	public string[]? SelectedMaps { get; set; }
	public string? SongFolder { get; set; }
	public string? BackupFolder { get; set; }
	public bool? DarkTheme { get; set; }
}