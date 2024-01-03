using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Mapping_Tools.Classes.HitsoundStuff;

public class Playback
{
	private static Process? AudioPlayerProcess;
	public Playback(string path)
	{
		if(string.IsNullOrEmpty(path))
			throw new Exception("Path to file is empty");
		if(!File.Exists(path))
			throw new FileNotFoundException($"{path} does not exist!");
		if(!IsHitsound(path))
			throw new Exception($"{path.Split('/')[^1]} is not a sound file!");


		if(AudioPlayerProcess is not null)
			Stop();

		AudioPlayerProcess = new Process
		{
			StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"play \'{path}\'\"",
				CreateNoWindow = true
            }
		};
	}

	public void Play()
	{
		new Thread(async delegate ()
		{
			AudioPlayerProcess!.Start();
			await AudioPlayerProcess.WaitForExitAsync();
			if(AudioPlayerProcess.ExitCode!=0)
			{
				var box = MessageBoxManager.GetMessageBoxStandard("Sox not found!",
					"To support hitsound preview, install \"sox\" and its codecs using your distro's package manager",
					ButtonEnum.Ok);
				box.ShowAsync();
			}
			Stop();
		}).Start();
	}

	public void Stop()
	{
		AudioPlayerProcess!.Dispose();
		AudioPlayerProcess = null;
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