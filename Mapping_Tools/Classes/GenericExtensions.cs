using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NAudio.Utils;

namespace Mapping_Tools.Classes {
    public static class GenericExtensions
    {
        public static int RemoveAll<T>(this ObservableCollection<T> coll, Func<T, bool> condition) {
            var itemsToRemove = coll.Where(condition).ToList();

            foreach (var itemToRemove in itemsToRemove) {
                coll.Remove(itemToRemove);
            }

            return itemsToRemove.Count;
        }

        // public static MessageBoxResult Show(this Exception exception) {
        //     var result = MessageBox.Show(exception.MessageStackTrace(), "Error", MessageBoxButton.OKCancel);
        //     if (result == MessageBoxResult.Cancel) return result;
        //     var ex = exception.InnerException;
        //     while (ex != null) {
        //         result = MessageBox.Show(ex.MessageStackTrace(), "Inner exception", MessageBoxButton.OKCancel);
        //         ex = result == MessageBoxResult.OK ? ex.InnerException : null;
        //     }

        //     return MessageBoxResult.OK;
        // }

		public static async Task<ButtonResult> Show(this Exception exception) {
            var box = MessageBoxManager.GetMessageBoxStandard(exception.MessageStackTrace(), "Error", ButtonEnum.OkCancel);
			var result = await box.ShowAsync();
            if (result == ButtonResult.Cancel) return result;
            var ex = exception.InnerException;
            while (ex != null) {
                box = MessageBoxManager.GetMessageBoxStandard(ex.MessageStackTrace(), "Inner exception", ButtonEnum.OkCancel);
				result = await box.ShowAsync();
                ex = result == ButtonResult.Ok ? ex.InnerException : null;
            }

            return ButtonResult.Ok;
        }

        public static string MessageStackTrace(this Exception exception) {
            return exception.Message + "\n\n" + exception.StackTrace;
        }

        /// <summary>
        /// Converts a <see cref="System.Drawing.Image"/> into a WPF <see cref="BitmapSource"/>.
        /// </summary>
        /// <param name="source">The source image.</param>
        /// <returns>A BitmapSource</returns>
		/// 
        // public static BitmapSource ToBitmapSource(this System.Drawing.Image source)
        // {
        //     System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(source);

        //     var bitSrc = bitmap.ToBitmapSource();

        //     bitmap.Dispose();
        //     bitmap = null;

        //     return bitSrc;
        // }

        /// <summary>
        /// Converts a <see cref="System.Drawing.Bitmap"/> into a WPF <see cref="BitmapSource"/>.
        /// </summary>
        /// <remarks>Uses GDI to do the conversion. Hence the call to the marshalled DeleteObject.
        /// </remarks>
        /// <param name="source">The source bitmap.</param>
        /// <returns>A BitmapSource</returns>
        // public static BitmapSource ToBitmapSource(this System.Drawing.Bitmap source)
        // {
        //     BitmapSource bitSrc = null;

        //     var hBitmap = source.GetHbitmap();

        //     try
        //     {
        //         bitSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
        //             hBitmap,
        //             IntPtr.Zero,
        //             Int32Rect.Empty,
        //             BitmapSizeOptions.FromEmptyOptions());
        //     }
        //     catch (Win32Exception)
        //     {
        //         bitSrc = null;
        //     }
        //     finally
        //     {
        //         DeleteObject(hBitmap);
        //     }

        //     return bitSrc;
        // }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
    }
}
