using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Handlers;
using System.Threading;
using System.Threading.Tasks;
using Avalonia_Mapping_Tools.ViewModels;

namespace Mapping_Tools.Classes.SystemTools
{
    public class DownloadManager
    {
        public static async Task<bool> Download(string url, string path, CancellationTokenSource cts, DownloadProgressViewModel? vm = null)
        {
            try
            {
                HttpClient hc;
                if(vm is not null)
                {
                    HttpClientHandler handler = new();
                    ProgressMessageHandler pmh = new(handler);
                    pmh.HttpReceiveProgress += (_, args) =>
                    {
                        int progress = (int)Math.Floor((double)((double)args.BytesTransferred / args.TotalBytes * 100)!);
                        double currentDownloadSize = Math.Round((double)args.BytesTransferred / 1024 / 1024, 2, MidpointRounding.AwayFromZero);
                        double totalDownloadSize = Math.Round((double)args.TotalBytes! / 1024 / 1024, 2, MidpointRounding.AwayFromZero);
                        vm.UpdateStringProgress(currentDownloadSize, totalDownloadSize);
                        vm.Progress = progress;
                    };
                    hc = new(pmh);
                }
                else
                {
                    hc = new HttpClient();
                }

                CancellationToken ct = cts.Token;
                var download = await hc.GetByteArrayAsync(url, ct);
                await File.WriteAllBytesAsync(path, download, ct);
                return true;
            }
            catch(TaskCanceledException e)
            {
                Console.WriteLine(e.Message);
            }
            catch(Exception e)
            {
                e.Show();
            }
            return false;
        }

        public static bool Unzip(string zipPath, string extractPath)
        {
            try
            {
                ZipFile.ExtractToDirectory(zipPath, extractPath, true);
            }
            catch(Exception e)
            {
                e.Show();
                return false;
            }
            return true;
        }
    }
}