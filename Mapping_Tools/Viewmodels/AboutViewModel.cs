namespace Avalonia_Mapping_Tools.ViewModels;

public partial class AboutViewModel : ViewModelBase
{
	public string URLGitHub { get; } = "https://github.com/night-mareLuna/Avalonia_Mapping_Tools";
	public string URLMappingTools { get; } = "https://github.com/OliBomby/Mapping_Tools";
	public string URLAvalonia { get; } = "https://www.avaloniaui.net/";
	public string URLGosumemory { get; } = "https://github.com/l3lackShark/gosumemory";
	public string Version { get; } = "v0.2.0";
	public AboutViewModel()
	{

	}

	public void OpenLink(string link)
	{
		var psi = new System.Diagnostics.ProcessStartInfo
		{
			UseShellExecute = true,
			FileName = link
		};
		System.Diagnostics.Process.Start(psi);
	}
}