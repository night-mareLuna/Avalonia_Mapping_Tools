using System;
using System.Data;
using Avalonia.Controls;

namespace Mapping_Tools.Views {
    /// <summary>
    /// TODO: Change class to different name as it's similar to file extensions.
    /// </summary>
    public static class Extensions {
        public static double GetDouble(this TextBox textBox, double defaultValue=-1) {
            try {
                return ParseDouble(textBox.Text);
            } catch {
                return defaultValue;
            } 
        }

        public static int GetInt(this TextBox textBox, int defaultValue = -1) {
            try {
                return ParseInt(textBox.Text);
            } catch {
                return defaultValue;
            }
        }

        public static double ParseDouble(string str) {
            using (DataTable dt = new DataTable()) {
                string text = str.Replace(",", ".");
                var v = dt.Compute(text, "");
                return Convert.ToDouble(v);
            }
        }

        public static int ParseInt(string str) {
            using (DataTable dt = new DataTable()) {
                string text = str.Replace(",", ".");
                var v = dt.Compute(text, "");
                return Convert.ToInt32(v);
            }
        }
    }
}