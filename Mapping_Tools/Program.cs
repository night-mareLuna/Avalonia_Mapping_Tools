using Avalonia;
using System;

namespace Avalonia_Mapping_Tools;

public class Program
{
#if DEBUG
	public static readonly string configPath = AppDomain.CurrentDomain.BaseDirectory + "MappingTools";
#else
	public static readonly string configPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Mapping Tools";
#endif

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
            .LogToTrace()
			.With(new X11PlatformOptions
			{
				UseDBusFilePicker = false // to disable FreeDesktop file picker
			});

}
