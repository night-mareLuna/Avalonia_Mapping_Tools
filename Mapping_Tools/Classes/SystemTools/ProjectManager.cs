using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using Mapping_Tools.Classes.JsonConverters;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System.Threading.Tasks;

namespace Mapping_Tools.Classes.SystemTools {
    public enum ErrorType
    {
        Success,
        Error,
        Warning
    }

    public static class ProjectManager {
        private static readonly JsonSerializer serializer = new() {
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Objects,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore, 
            Converters = { new Vector2Converter()}
        };

        public static void WriteJson(StreamWriter streamWriter, object obj) {
            using (JsonTextWriter reader = new JsonTextWriter(streamWriter)) {
                serializer.Serialize(reader, obj);
            }
        }

        public static void SaveJson(string path, object obj) {
            using (StreamWriter fs = new StreamWriter(path)) {
                WriteJson(fs, obj);
            }
        }
        
        public static T LoadJson<T>(string path) {
            using (StreamReader fs = new StreamReader(path)) {
                using (JsonReader reader = new JsonTextReader(fs)) {
                    return serializer.Deserialize<T>(reader);
                }
            }
        }

        public static T LoadJson<T>(Stream stream) {
            using (StreamReader fs = new StreamReader(stream)) {
                using (JsonReader reader = new JsonTextReader(fs)) {
                    return serializer.Deserialize<T>(reader);
                }
            }
        }

        public static void AutoSaveProject<T>(ISavable<T> view) {
            string path = view.AutoSavePath;
            SaveProject(view, path);

            if (view is IHasExtraAutoSaveTarget hasExtraAutoSaveTarget) {
                SaveProject(view, hasExtraAutoSaveTarget.ExtraAutoSavePath);
            }
        }

        public static async void SaveProjectDialog<T>(ISavable<T> view) {
            Directory.CreateDirectory(view.DefaultSaveFolder);
            string path = await IOHelper.SaveProjectDialog(view.DefaultSaveFolder);
            SaveProject(view, path);
        }

        public static void SaveProject<T>(ISavable<T> view, string path) {
            // If the file name is not an empty string open it for saving.
            if (string.IsNullOrEmpty(path)) return;
            try {
                SaveJson(path, view.GetSaveData());
            } catch (Exception ex) {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.Message);

				var box = MessageBoxManager.GetMessageBoxStandard("Error!",
					"Project could not be saved!",
					ButtonEnum.Ok);
				box.ShowAsync();
                ex.Show();
            }
        }

        public static async void LoadProject<T>(ISavable<T> view, bool dialog=false, bool message=true) {
            if (dialog)
                Directory.CreateDirectory(view.DefaultSaveFolder);
            string path = dialog ? await IOHelper.LoadProjectDialog(view.DefaultSaveFolder) : view.AutoSavePath;

            // If the file name is not an empty string open it for saving.  
            if (path == "") return;
            try {
                T project = LoadJson<T>(path);

                if (project == null) {
                    throw new Exception("Loaded project is a null reference.");
                }

                view.SetSaveData(project);
            } catch (Exception ex) {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.Message);

                if (message) {
					var box = MessageBoxManager.GetMessageBoxStandard("Error!",
						"Project could not be loaded!",
						ButtonEnum.Ok);
                    box.ShowAsync();
                    ex.Show();
                }
            }
        }

        public static async void NewProject<T>(ISavable<T> view, bool dialog = false, bool message = true) {
            if (dialog) {
				var messageBox = MessageBoxManager.GetMessageBoxStandard("Confirm new project",
					"Are you sure you want to start a new project? All unsaved progress will be lost.",
					ButtonEnum.YesNo);
				var messageBoxResult = await messageBox.ShowAsync();
                if (messageBoxResult != ButtonResult.Yes) return;
            }

            try {
                T project = Activator.CreateInstance<T>();
                view.SetSaveData(project);
            } catch (Exception ex) {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.Message);

                if (message) {
					var box = MessageBoxManager.GetMessageBoxStandard("Error!",
						"New project could not be initialized!",
						ButtonEnum.Ok);
					box.ShowAsync();
                    ex.Show();
                }
            }
        }

        /// <summary>
        /// Gets the project file for a savable tool with optional dialog.
        /// Uses default save path if no dialog is used.
        /// </summary>
        /// <typeparam name="T">The type of the project data</typeparam>
        /// <param name="view">The tool to get the project from</param>
        /// <param name="dialog">Whether to use a dialog</param>
        /// <returns></returns>
        public static async Task<T> GetProjectAsync<T>(ISavable<T> view, bool dialog=false) {
            if (dialog)
                Directory.CreateDirectory(view.DefaultSaveFolder);
            string path = dialog ? await IOHelper.LoadProjectDialog(view.DefaultSaveFolder) : view.AutoSavePath;

            return LoadJson<T>(path);
        }

        public static async void SaveToolFile<T, T2>(ISavable<T> view, T2 obj, bool dialog = false) {
            if (dialog)
                Directory.CreateDirectory(view.DefaultSaveFolder);
            string path = dialog ? await IOHelper.SaveProjectDialog(view.DefaultSaveFolder) : view.AutoSavePath;

            SaveJson(path, obj);
        }

        public static async Task<T2> LoadToolFile<T, T2>(ISavable<T> view, bool dialog = false) {
            if (dialog)
                Directory.CreateDirectory(view.DefaultSaveFolder);
            string path = dialog ? await IOHelper.LoadProjectDialog(view.DefaultSaveFolder) : view.AutoSavePath;

            return LoadJson<T2>(path);
        }

        public static bool IsSavable(object obj) {
            return IsSavable(obj.GetType());
        }

        public static bool IsSavable(Type type) {
            return type.GetInterfaces().Any(x =>
                x.IsGenericType &&
                x.GetGenericTypeDefinition() == typeof(ISavable<>));
        }
    }
}
