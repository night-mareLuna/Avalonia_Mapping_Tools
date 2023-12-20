using Avalonia;
using Avalonia_Mapping_Tools.Models;
using System;

namespace Avalonia_Mapping_Tools;

public class Program
{
	public static readonly string configPath = AppDomain.CurrentDomain.BaseDirectory + "MappingToolsConfigs";
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) 
	{
		BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
	}

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
