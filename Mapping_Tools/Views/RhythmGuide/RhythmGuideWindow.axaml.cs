using System;
using Avalonia.Controls;

namespace Avalonia_Mapping_Tools.Views;
public partial class RhythmGuideWindow : Window
{
	// public bool IsHidden = true;
	public RhythmGuideWindow()
	{
		InitializeComponent();
		RhythmGuideContent.Content = new RhythmGuideView();
		// Closing += (s, e) => 
		// {
		// 	((Window)s!).Hide();
		// 	IsHidden = true;
		// 	e.Cancel = true;
		// };
	}

    // public override void Show()
    // {
	// 	IsHidden = false;
    //     base.Show();
    // }
}