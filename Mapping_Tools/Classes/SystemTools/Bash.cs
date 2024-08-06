using System;
using System.Diagnostics;
using System.IO;

namespace Mapping_Tools.Classes.SystemTools
{
    public class Bash
    {
        public static string RunCommand(string command)
		{
			if(string.IsNullOrWhiteSpace(command))
				return string.Empty;

			string commandOutput = string.Empty;
			try
			{
				var process = new Process()
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = "bash",
						Arguments = $"-c \"{command}\"",
						RedirectStandardOutput = true,
						UseShellExecute = false,
						CreateNoWindow = true,
						WorkingDirectory = Path.GetDirectoryName("/usr/bin")
					}
				};
				process.Start();
				commandOutput = process.StandardOutput.ReadToEnd();
				process.WaitForExit();
			}
			catch(Exception e)
			{
                Console.WriteLine(e.Message);
            }

			return commandOutput;
		}
    }
}