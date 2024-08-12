using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Mapping_Tools.Classes.HitsoundStuff;

public class Playback
{
	/// Use a list to prevent issues caused by multiple hitsounds playing at the same time (spam clicking the hitsounds)
	private static readonly List<Process> AudioPlayerProcesses = [];
	public Playback(string path)
	{
		if(string.IsNullOrWhiteSpace(path))
			throw new ArgumentNullException("Path to file is null or empty");
		if(!File.Exists(path))
			throw new FileNotFoundException($"{path} does not exist or is not accessible!");
		if(!IsHitsound(path))
			throw new ArgumentOutOfRangeException($"{path.Split('/')[^1]} is not a sound file!");

		Process hitsound = new Process
		{
			StartInfo = new ProcessStartInfo
            {
                FileName = "play",
                Arguments = $"\"{path}\"",
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true // Prevent sox from printing to console
            }
		};

		AudioPlayerProcesses.Add(hitsound);
	}

	public async void Play()
	{
		using Process hitsound = AudioPlayerProcesses[^1];
		hitsound.Start();
		await hitsound.WaitForExitAsync();
		if(hitsound.ExitCode!=0)
		{
			var box = MessageBoxManager.GetMessageBoxStandard("Sox not found!",
				"To support hitsound preview, install \"sox\" and its codecs using your distro's package manager.",
				ButtonEnum.Ok);
			await box.ShowAsync();
		}
		AudioPlayerProcesses.Remove(AudioPlayerProcesses[AudioPlayerProcesses.IndexOf(hitsound)]);
	}

	private static bool IsHitsound(string path)
	{
		string extension = path.Split('.')[^1];
        return extension.ToLower() switch
        {
            "wav" => true,
            "ogg" => true,
			"mp3" => true,
            _ => false,
        };
    }
}