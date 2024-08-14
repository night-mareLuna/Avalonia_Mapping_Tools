using System;
using System.Diagnostics;
using System.IO;

namespace Mapping_Tools.Classes.SystemTools
{
    public class Bash
    {
        public static string RunCommand(string command, bool printUsedCommand = false)
		{
			if(string.IsNullOrWhiteSpace(command))
				return string.Empty;

			return RunCommandDirect("sh", $"-c \"{command}\"", printUsedCommand);
		}

		public static string RunCommandDirect(string exe, string arguments, bool printUsedCommand = false)
		{
			if(string.IsNullOrWhiteSpace(arguments))
				return string.Empty;

			string commandOutput = string.Empty;
			if(printUsedCommand)
				Console.WriteLine($"{exe} {arguments}");
			try
			{
				var process = new Process()
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = exe,
						Arguments = arguments,
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