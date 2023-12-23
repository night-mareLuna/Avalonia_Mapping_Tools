using Mapping_Tools.Classes;
using System.ComponentModel;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Mapping_Tools.Views {
    public class SingleRunMappingTool : MappingTool {
        protected readonly BackgroundWorker BackgroundWorker;
        public int Progress { get; set; }
        public bool Verbose { get; set; }
        public SingleRunMappingTool() {
            BackgroundWorker = new BackgroundWorker();
            BackgroundWorker.DoWork += BackgroundWorker_DoWork;
            BackgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
            BackgroundWorker.WorkerReportsProgress = true;
            BackgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
        }

        protected virtual void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e) { }

        protected virtual void BackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e) {
            Progress = e.ProgressPercentage;
        }

        /// <summary>
        /// Displays any errors, displays the result if not empty, resets progress and CanRun
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		protected virtual void BackgroundWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null) {
				e.Error.Show();
			} else if (!string.IsNullOrEmpty(e.Result as string)) {
				if (Verbose) {
					var box = MessageBoxManager.GetMessageBoxStandard("", e.Result.ToString(), ButtonEnum.Ok);
					box.ShowAsync();
				// } else {
				//     Task.Factory.StartNew(() => MainWindow.MessageQueue.Enqueue(e.Result.ToString(), true));
				}
			}
			Progress = 0;
		}

        protected static void UpdateProgressBar(BackgroundWorker worker, int progress) {
            if (worker != null && worker.WorkerReportsProgress) {
                worker.ReportProgress(progress);
            }
        }
    }
}