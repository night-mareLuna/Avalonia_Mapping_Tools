using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mapping_Tools.Classes.SystemTools
{
    public class DownloadManager
    {
        public static async Task<bool> Download(string url, string path)
        {
            try
            {
                var hc = new HttpClient();
                await File.WriteAllBytesAsync(path, await hc.GetByteArrayAsync(url));
            }
            catch(Exception e)
            {
                e.Show();
                return false;
            }
            return true;
        }

        public static bool Unzip(string zipPath, string extractPath)
        {
            try
            {
                ZipFile.ExtractToDirectory(zipPath, extractPath);
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